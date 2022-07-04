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

      public ImageSize(float widthInInches, float heightInInches, float horizontalPixelsPerInch, float verticalPixelsPerInch)
      {
         var widthInPixels = PixelConverter.ConvertInchesToPixels(horizontalPixelsPerInch, widthInInches);
         var heightInPixels = PixelConverter.ConvertInchesToPixels(verticalPixelsPerInch, heightInInches);

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

      public float WidthInInches => WidthInPixels / HorizontalPixelsPerInch;
      public string WidthInInchesRounded(int numberOfDecimals) => Math.Round(WidthInInches, numberOfDecimals).ToString();

      public float HeightInInches => HeightInPixels / VerticalPixelsPerInch;
      public string HeightInInchesRounded(int numberOfDecimals) => Math.Round(HeightInInches, numberOfDecimals).ToString();

      //private float? ConvertPixelsToInches(float inchesPerPixel, string pixels)
      //{
      //   int? convertedPixels = ConvertToPixels(pixels);

      //   if (convertedPixels is null)
      //   {
      //      return null;
      //   }

      //   return (int)(convertedPixels.Value * inchesPerPixel);
      //}

      //private int? ConvertToPixels(string measurement)
      //{
      //   int result;

      //   if (int.TryParse(measurement, out result))
      //   {
      //      return result;
      //   }
      //   else
      //   {
      //      return null;
      //   }
      //}

      //private int? ConvertInchesToPixels(float pixelsPerInch, string inches)
      //{
      //   float? convertedInches = ConvertToInches(inches);

      //   if (convertedInches is null)
      //   {
      //      return null;
      //   }

      //   return (int)(convertedInches.Value * pixelsPerInch);
      //}

      //private float? ConvertToInches(string measurement)
      //{
      //   float result;

      //   if (float.TryParse(measurement, out result))
      //   {
      //      return result;
      //   }
      //   else
      //   {
      //      return null;
      //   }
      //}
   }
}
