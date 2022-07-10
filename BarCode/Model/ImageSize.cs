using System;
using System.Drawing;

namespace BarCode
{
   public class ImageSize
   {
      private Size _SizeInPixels;

      // DPI
      public float HorizontalPixelsPerInch { get; private set; }

      // DPI
      public float VerticalPixelsPerInch { get; private set; }

      public ImageSize(float widthInInches, double widthToHeightRatio, float horizontalPixelsPerInch, float verticalPixelsPerInch)
      {
         var widthInPixels = PixelConverter.ConvertInchesToPixels(horizontalPixelsPerInch, widthInInches);
         var heightInPixels = (int)(widthInPixels / widthToHeightRatio);

         HorizontalPixelsPerInch = horizontalPixelsPerInch;
         VerticalPixelsPerInch = verticalPixelsPerInch;

         if ((widthInInches != 0) && (heightInPixels != 0))
         {
            _SizeInPixels = new Size(widthInPixels, heightInPixels);
         }
         else
         {
            throw new InvalidOperationException($"Can't convert 'widthInInches' 'heightInInches'");
         }
      }

      public ImageSize(int widthInPixels, int heightInPixels, float horizontalPixelsPerInch, float verticalPixelsPerInch)
      {
         _SizeInPixels = new Size(widthInPixels, heightInPixels);

         HorizontalPixelsPerInch = horizontalPixelsPerInch;
         VerticalPixelsPerInch = verticalPixelsPerInch;
      }

      public int WidthInPixels => _SizeInPixels.Width;
      public int HeightInPixels => _SizeInPixels.Height;

      public float WidthToHeightRatioFromPixels => (float)WidthInPixels / (float)HeightInPixels;
      public float WidthInInches => (float)WidthInPixels / HorizontalPixelsPerInch;
      public string WidthInInchesRounded(int numberOfDecimals) => Math.Round(WidthInInches, numberOfDecimals).ToString();

      public float HeightInInches => (float)HeightInPixels / VerticalPixelsPerInch;
      public string HeightInInchesRounded(int numberOfDecimals) => Math.Round(HeightInInches, numberOfDecimals).ToString();
   }
}
