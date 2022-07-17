using System.Drawing;
using System.IO;

namespace BarCode
{
   public abstract class ImageFileBase
   {
      public ImageFileBase(string fullPath)
      {
         FullPath = fullPath;
      }

      public string FullPath { get; private set; }

      /// <summary>
      /// Returns the filename and extension
      /// </summary>
      public string FileName => Path.GetFileName(FullPath);
      public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(FullPath);
      public string DirectoryName => Path.GetDirectoryName(FullPath);
      public string Extension => Path.GetExtension(FullPath);
      public float HorizontalResolution => Image.HorizontalResolution;
      public float VerticalResolution => Image.VerticalResolution;

      public Image Image { get; protected set; }
      public ImageSize ImageSize { get; protected set; }
   }
}