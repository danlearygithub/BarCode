using System.Drawing;
using System.IO;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

namespace BarCodeUWP
{
   public abstract class ImageFileBase
   {
      public ImageFileBase(string fullPath)
      {
         FullPath = fullPath;
      }

      public string FullPath { get; private set; }

      public string FileName => Path.GetFileName(FullPath);
      public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(FullPath);
      public string DirectoryName => Path.GetDirectoryName(FullPath);
      public string Extension => Path.GetExtension(FullPath);
     
      public ImageSize ImageSize { get; protected set; }
   }
}