using System;
using System.Drawing;

namespace BarCodeUWP
{
   public class ImageSize
   {
      private Size _SizeInPixels;

      public ImageSize(float inchesPerPixel, string widthInInches, string heightInInches)
      {

         var widthInPixels = ConvertInchesToPixels(widthInInches);
         var heightInPixels = ConvertInchesToPixels(heightInInches);

         if ((widthInInches != null) && (heightInPixels != null))
         {
            _SizeInPixels = new Size(widthInPixels.Value, heightInPixels.Value);
         }
         else
         {
            throw new InvalidOperationException($"Can't convert 'widthInInches' 'heightInInches'");
         }

         InchesPerPixel = inchesPerPixel;
      }

      public ImageSize(float inchesPerPixel, int widthInPixels, int heightInPixels)
      {
         InchesPerPixel = inchesPerPixel;
         _SizeInPixels = new Size(widthInPixels, heightInPixels);
      }

      public int WidthInPixels => _SizeInPixels.Width;
      public int HeightInPixels => _SizeInPixels.Height;

      public static float InchesPerPixel { get; private set; }

      public static int PixelsPerInch => (int)(1 / InchesPerPixel);

      public float WidthInInches => WidthInPixels * InchesPerPixel;
      public string WidthInInchesRounded(int numberOfDecimals) => Math.Round(WidthInInches, numberOfDecimals).ToString();

      public float HeightInInches => HeightInPixels * InchesPerPixel;
      public string HeightInInchesRounded(int numberOfDecimals) => Math.Round(HeightInInches, numberOfDecimals).ToString();

      public static float? ConvertPixelsToInches(string measurementInPixels)
      {
         int? pixels = ConvertToPixels(measurementInPixels);

         if (pixels is null)
         {
            return null;
         }

         return (int)(pixels.Value * InchesPerPixel);
      }

      public static int? ConvertToPixels(string measurement)
      {
         int result;

         if (int.TryParse(measurement, out result))
         {
            return result;
         }
         else
         {
            return null;
         }
      }

      public static int? ConvertInchesToPixels(string measurementInInches)
      {
         float? inches = ConvertToInches(measurementInInches);

         if (inches is null)
         {
            return null;
         }

         return (int)(inches.Value * PixelsPerInch);
      }

      public static float? ConvertToInches(string measurement)
      {
         float result;

         if (float.TryParse(measurement, out result))
         {
            return result;
         }
         else
         {
            return null;
         }
      }
   }
}
