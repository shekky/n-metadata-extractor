/*
 * Copyright 2002-2015 Drew Noakes
 *
 *    Modified by Yakov Danilov <yakodani@gmail.com> for Imazen LLC (Ported from Java to C#)
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *
 * More information about this project is available at:
 *
 *    https://drewnoakes.com/code/exif/
 *    https://github.com/drewnoakes/metadata-extractor
 */
using Com.Drew.Imaging.Jpeg;
using Com.Drew.Lang;
using Sharpen;

namespace Com.Drew.Metadata.Exif
{
	/// <summary>
	/// Unit tests for
	/// <see cref="ExifSubIFDDirectory"/>
	/// ,
	/// <see cref="ExifIFD0Directory"/>
	/// ,
	/// <see cref="ExifThumbnailDirectory"/>
	/// .
	/// </summary>
	/// <author>Drew Noakes https://drewnoakes.com</author>
	public class ExifDirectoryTest
	{
		/// <exception cref="System.Exception"/>
		[NUnit.Framework.Test]
		public virtual void TestGetDirectoryName()
		{
			Com.Drew.Metadata.Directory subIFDDirectory = new ExifSubIFDDirectory();
			Com.Drew.Metadata.Directory ifd0Directory = new ExifIFD0Directory();
			Com.Drew.Metadata.Directory thumbDirectory = new ExifThumbnailDirectory();
			Sharpen.Tests.IsFalse(subIFDDirectory.HasErrors());
			Sharpen.Tests.IsFalse(ifd0Directory.HasErrors());
			Sharpen.Tests.IsFalse(thumbDirectory.HasErrors());
			Sharpen.Tests.AreEqual("Exif IFD0", ifd0Directory.GetName());
			Sharpen.Tests.AreEqual("Exif SubIFD", subIFDDirectory.GetName());
			Sharpen.Tests.AreEqual("Exif Thumbnail", thumbDirectory.GetName());
		}

		/// <exception cref="System.Exception"/>
		[NUnit.Framework.Test]
		public virtual void TestGetThumbnailData()
		{
			ExifThumbnailDirectory directory = ExifReaderTest.ProcessBytes<ExifThumbnailDirectory>("Tests/Data/withExif.jpg.app1");
			sbyte[] thumbData = directory.GetThumbnailData();
			NUnit.Framework.Assert.IsNotNull(thumbData);
			try
			{
				// attempt to read the thumbnail -- it should be a legal Jpeg file
				JpegSegmentReader.ReadSegments(new SequentialByteArrayReader(thumbData), null);
			}
			catch (JpegProcessingException)
			{
				NUnit.Framework.Assert.Fail("Unable to construct JpegSegmentReader from thumbnail data");
			}
		}

		/// <exception cref="System.Exception"/>
		[NUnit.Framework.Test]
		public virtual void TestWriteThumbnail()
		{
			ExifThumbnailDirectory directory = ExifReaderTest.ProcessBytes<ExifThumbnailDirectory>("Tests/Data/manuallyAddedThumbnail.jpg.app1");
			Sharpen.Tests.IsTrue(directory.HasThumbnailData());
			FilePath thumbnailFile = FilePath.CreateTempFile("thumbnail", ".jpg");
			try
			{
				directory.WriteThumbnail(thumbnailFile.GetAbsolutePath());
				FilePath file = new FilePath(thumbnailFile.GetAbsolutePath());
				Sharpen.Tests.AreEqual(2970, file.Length());
				Sharpen.Tests.IsTrue(file.Exists());
			}
			finally
			{
				if (!thumbnailFile.Delete())
				{
					NUnit.Framework.Assert.Fail("Unable to delete temp thumbnail file.");
				}
			}
		}

		//    @Test
		//    public void testContainsThumbnail()
		//    {
		//        ExifSubIFDDirectory exifDirectory = new ExifSubIFDDirectory();
		//
		//        assertTrue(!exifDirectory.hasThumbnailData());
		//
		//        exifDirectory.setObject(ExifSubIFDDirectory.TAG_THUMBNAIL_DATA, "foo");
		//
		//        assertTrue(exifDirectory.hasThumbnailData());
		//    }
		/// <exception cref="Com.Drew.Imaging.Jpeg.JpegProcessingException"/>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="Com.Drew.Metadata.MetadataException"/>
		[NUnit.Framework.Test]
		public virtual void TestResolution()
		{
			Com.Drew.Metadata.Metadata metadata = ExifReaderTest.ProcessBytes("Tests/Data/withUncompressedRGBThumbnail.jpg.app1");
			ExifThumbnailDirectory thumbnailDirectory = metadata.GetFirstDirectoryOfType<ExifThumbnailDirectory>();
			NUnit.Framework.Assert.IsNotNull(thumbnailDirectory);
			Sharpen.Tests.AreEqual(72, thumbnailDirectory.GetInt(ExifThumbnailDirectory.TagXResolution));
			ExifIFD0Directory exifIFD0Directory = metadata.GetFirstDirectoryOfType<ExifIFD0Directory>();
			NUnit.Framework.Assert.IsNotNull(exifIFD0Directory);
			Sharpen.Tests.AreEqual(216, exifIFD0Directory.GetInt(ExifIFD0Directory.TagXResolution));
		}
	}
}
