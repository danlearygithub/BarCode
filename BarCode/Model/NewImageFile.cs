using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.Streams;

namespace BarCode
{
   public class NewImageFile : ImageFileBase
   {
      private ImageFile _ExistingImageFile;

      public NewImageFile(string fullPath, ImageFile existingImageFile)
         : base(fullPath)
      {
         _ExistingImageFile = existingImageFile;

         ImageSize = new ImageSize(_ExistingImageFile.ImageSize.WidthInInches, _ExistingImageFile.ImageSize.HeightInInches, _ExistingImageFile.ImageSize.HorizontalPixelsPerInch, _ExistingImageFile.ImageSize.VerticalPixelsPerInch);
         Image = ResizeImage(_ExistingImageFile.Image, ImageSize.WidthInPixels, ImageSize.HeightInPixels);
      }

      // https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp
      private Bitmap ResizeImage(Image existingImage, int width, int height)
      {
         var destRect = new Rectangle(0, 0, width, height);
         var destImage = new Bitmap(width, height);

         destImage.SetResolution(existingImage.HorizontalResolution, existingImage.VerticalResolution);

         using (var graphics = Graphics.FromImage(destImage))
         {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (var wrapMode = new ImageAttributes())
            {
               wrapMode.SetWrapMode(WrapMode.TileFlipXY);
               graphics.DrawImage(existingImage, destRect, 0, 0, existingImage.Width, existingImage.Height, GraphicsUnit.Pixel, wrapMode);
            }
         }

         return destImage;
      }

      public bool SaveImage()
      {
         try
         {
            Image.Save(FullPath);
            return true;
         }
         catch (System.Exception)
         {
            return false;
         }


      }

   }
}
