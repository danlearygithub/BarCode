﻿using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;

namespace BarCode
{
   public enum ImageResult
   {
      NotSaved,
      Saved
   }

   public class NewImageFile : ImageFileBase
   {
      private ImageFile _ExistingImageFile;
      private Product _Product;

      private const int COMMENT_HEIGHT = 80;
      private const int EXTRA_WIDTH = 50;

      public NewImageFile(string fullPath, ImageFile existingImageFile, AppSettings settings, Product product)
         : base(fullPath)
      {
         _ExistingImageFile = existingImageFile;

         var barCodeImageWidthInInches = settings.ImageWidthInInches;
         var horizontalPixelsPerInch = _ExistingImageFile.ImageSize.HorizontalPixelsPerInch;
         var verticalPixelsPerInch = _ExistingImageFile.ImageSize.VerticalPixelsPerInch;

         ImageSize = new ImageSize(barCodeImageWidthInInches, existingImageFile.ImageSize.WidthToHeightRatioFromPixels, horizontalPixelsPerInch, verticalPixelsPerInch);

         Image = _ExistingImageFile.Image;

         _Product = product;
      }

      // https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp
      public (ImageResult result, string exceptionMessage) ResizeImage()
      {
         var imageWidthInPixels = ImageSize.WidthInPixels;
         var fullWidthInPixels = imageWidthInPixels + EXTRA_WIDTH;

         var imageHeightInPixels = ImageSize.HeightInPixels;
         var fullHeightInPixels = imageHeightInPixels + COMMENT_HEIGHT;

         try
         {
            var x = EXTRA_WIDTH / 2;

            var imageDestRect = new Rectangle(x, COMMENT_HEIGHT, imageWidthInPixels, imageHeightInPixels);

            using (var newImage = new Bitmap(fullWidthInPixels, fullHeightInPixels))
            {
               newImage.SetResolution(Image.HorizontalResolution, Image.VerticalResolution);

               using (var graphics = Graphics.FromImage(newImage))
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
                     graphics.DrawImage(Image, imageDestRect, 0, 0, Image.Width, Image.Height, GraphicsUnit.Pixel, wrapMode);
                  }

                  // fill area left of image with white
                  graphics.FillRectangle(Brushes.White, 0, COMMENT_HEIGHT, x, fullHeightInPixels);

                  // fill area right of image with white
                  graphics.FillRectangle(Brushes.White, x + imageWidthInPixels, COMMENT_HEIGHT, fullWidthInPixels, fullHeightInPixels);

                  //set area for comment background to white
                  graphics.FillRectangle(Brushes.White, 0, 0, fullWidthInPixels, COMMENT_HEIGHT);

                  var gapBetweenLines = 0;

                  var rect = AddText(graphics, _Product.Vendor, width: fullWidthInPixels, y: 5, emSize: 10);
                  
                  rect = AddText(graphics, _Product.RegisDescription, width: fullWidthInPixels, y: rect.Top + rect.Height + gapBetweenLines, emSize: 8);

                  AddText(graphics, _ExistingImageFile.FullPath, width: fullWidthInPixels, y: rect.Top + rect.Height + gapBetweenLines, emSize: 3);
               }

               newImage.Save(FullPath);
            }

            return (ImageResult.Saved, null);
         }
         catch (System.Exception ex)
         {
            return (ImageResult.NotSaved, ex.Message);
         }
      }

      private Rectangle AddText(Graphics graphics, string text, int width, int y, float emSize)
      {
         // add comment
         var font = new Font(FontFamily.GenericSerif, emSize);

         var textSize = graphics.MeasureString(text, font);

         var stringRect = new Rectangle(0, y, width, (int)textSize.Height);

         var stringFormat = new StringFormat();
         stringFormat.Alignment = StringAlignment.Center;
         stringFormat.LineAlignment = StringAlignment.Near;

         // for testing 
         //  graphics.DrawRectangle(Pens.Black, stringRect);
         graphics.DrawString(text, font, Brushes.Black, stringRect, stringFormat);

         return stringRect;
      }

   }
}
