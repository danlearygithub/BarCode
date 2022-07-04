using System;
using System.Drawing;

namespace BarCode
{
   public class PixelConverter
   {

      public static int ConvertInchesToPixels(float pixelsPerInch, float inches)
      {
         return (int)(inches * pixelsPerInch);
      }

      public static float ConvertPixelsToInches(float pixelsPerInch, int pixels)
      {
         return (float)(pixels / pixelsPerInch);
      }

   }
}
