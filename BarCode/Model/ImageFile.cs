using System.Drawing;

namespace BarCode
{
   public class ImageFile : ImageFileBase
   {
      public ImageFile(string fullPath)
         :base(fullPath)
      {
         Image = Image.FromFile(FullPath);
         ImageSize = new ImageSize(Image.Width, Image.Height);
      }

     
   }
}