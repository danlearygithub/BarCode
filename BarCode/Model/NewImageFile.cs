using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

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

      private const int COMMENT_HEIGHT = 55;
      private const int EXTRA_WIDTH = 150;

      public NewImageFile(string fullPath, ImageFile existingImageFile, AppSettings settings, Product product)
         : base(fullPath)
      {
         _ExistingImageFile = existingImageFile;

         var barCodeImageWidthInches = settings.ImageWidthInInches;
         var horizontalPixelsPerInch = _ExistingImageFile.ImageSize.HorizontalPixelsPerInch;

         var barCodeImageHeightHeightInInches = settings.ImageHeightInInches;
         var verticalPixelsPerInch = _ExistingImageFile.ImageSize.VerticalPixelsPerInch;

         ImageSize = new ImageSize(barCodeImageWidthInches, barCodeImageHeightHeightInInches, horizontalPixelsPerInch, verticalPixelsPerInch);
         Image = _ExistingImageFile.Image;
         _Product = product;
      }

      // https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp
      public (ImageResult result, string exceptionMessage) ResizeImage()
      {
         var imageWidthInPixels = ImageSize.WidthInPixels;
         var widthInPixels = imageWidthInPixels + EXTRA_WIDTH;

         var imageHeightInPixels = ImageSize.HeightInPixels;
         var fullHeightInPixels = imageHeightInPixels + COMMENT_HEIGHT;

         try
         {

            var imageDestRect = new Rectangle(0, COMMENT_HEIGHT, widthInPixels, imageHeightInPixels);

            using (var newImage = new Bitmap(widthInPixels, fullHeightInPixels))
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

                  //set area for comment background to white
                  graphics.FillRectangle(Brushes.White, 0, 0, widthInPixels, COMMENT_HEIGHT);

                  AddText(graphics, _Product.Vendor, width: widthInPixels, y: 0, emSize: 10);
                  AddText(graphics, _Product.RegisDescription, width: widthInPixels, y: 20, emSize: 8);
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

   }
}
