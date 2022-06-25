using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace BarCode
{
   public class ImageFile
   {

      private Image _Image;

      public string FullPath { get; private set; }

      public string FileName => Path.GetFileName(FullPath);
      public string Extension => Path.GetExtension(FullPath);
      public int Width => _Image.Width ;
      public float HorizontalResolution => _Image.HorizontalResolution ;
      public int Height => _Image.Height;
      public float VerticalResolution => _Image.VerticalResolution;

      public ImageFile(string fullPath)
      {
         FullPath = fullPath;

         _Image = Image.FromFile(FullPath);
      }

      // https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp
      public Bitmap ResizeImage(int width, int height)
      {
         var destRect = new Rectangle(0, 0, width, height);
         var destImage = new Bitmap(width, height);

         destImage.SetResolution(HorizontalResolution, VerticalResolution);

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
               graphics.DrawImage(_Image, destRect, 0, 0, _Image.Width, _Image.Height, GraphicsUnit.Pixel, wrapMode);
            }
         }

         return destImage;
      }
   }
}