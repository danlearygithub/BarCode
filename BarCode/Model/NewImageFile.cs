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

      private const int COMMENT_HEIGHT = 40;

      public NewImageFile(string fullPath, ImageFile existingImageFile, AppSettings settings, Product product)
         : base(fullPath)
      {
         _ExistingImageFile = existingImageFile;

         var newWidthInches = settings.ImageWidthInInches;
         var horizontalPixelsPerInch = _ExistingImageFile.ImageSize.HorizontalPixelsPerInch;

         var newHeightInInches = settings.ImageHeightInInches;
         var verticalPixelsPerInch = _ExistingImageFile.ImageSize.VerticalPixelsPerInch;

         ImageSize = new ImageSize(newWidthInches, newHeightInInches, horizontalPixelsPerInch, verticalPixelsPerInch);
         Image = ResizeImage(_ExistingImageFile.Image, ImageSize.WidthInPixels+100, ImageSize.HeightInPixels, product);
      }

      // https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp
      private Bitmap ResizeImage(Image existingImage, int widthInPixels, int imageHeightInPixels, Product product)
      {
         var imageDestRect = new Rectangle(0, COMMENT_HEIGHT, widthInPixels, imageHeightInPixels);
         var destImage = new Bitmap(widthInPixels, imageHeightInPixels + COMMENT_HEIGHT);

         destImage.SetResolution(existingImage.HorizontalResolution, existingImage.VerticalResolution);

         using (var graphics = Graphics.FromImage(destImage))
         {
            // Can't use this or exception thrown - known issue online
            //    graphics.CompositingMode = CompositingMode.SourceCopy;

            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (var wrapMode = new ImageAttributes())
            {
              // wrapMode.SetWrapMode(WrapMode.TileFlipXY);
               graphics.DrawImage(existingImage, imageDestRect, 0, 0, existingImage.Width, existingImage.Height, GraphicsUnit.Pixel, wrapMode);
            }

            //set area for comment background to white
            graphics.FillRectangle(Brushes.White, 0, 0, widthInPixels, COMMENT_HEIGHT);

            AddText(graphics, product.Vendor, widthInPixels, 0, 8);
            AddText(graphics, product.RegisDescription, widthInPixels, 14, 6);
         }

         return destImage;
      }

      private void AddText(Graphics graphics, string text, int width, int y, float emSize)
      {
         // add comment
         var font = new Font(FontFamily.GenericSerif, emSize);

         var stringRect = new Rectangle(0, y, width, COMMENT_HEIGHT);

         var stringFormat = new StringFormat();
         stringFormat.Alignment = StringAlignment.Center;
         stringFormat.LineAlignment = StringAlignment.Center;

         graphics.DrawString(text, font, Brushes.Black, stringRect, stringFormat);
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
