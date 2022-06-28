using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace BarCodeUWP
{
   public class ImageFile : ImageFileBase
   {
      public SoftwareBitmap Image { get; protected set; }
      public StorageFile StorageFile { get; protected set; }

      public double HorizontalResolution => Image.DpiX;
      public double VerticalResolution => Image.DpiY;


      public ImageFile(AppSettings settings, SoftwareBitmap image, StorageFile storageFile)
         :base(storageFile.Path)
      {
         Image = image;
         StorageFile = storageFile;
         ImageSize = new ImageSize(settings.InchesPerPixelSetting, Image.PixelWidth, Image.PixelHeight);
      }

      // https://medium.com/dataseries/using-windows-10-built-in-ocr-with-c-b5ca8665a14e
      public async void ReadBarCodeAsync()
      {
         var language = new Language("en");

         var stream = File.OpenRead(FullPath);

         var decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());

         var bitmap = await decoder.GetSoftwareBitmapAsync();

         var engine = OcrEngine.TryCreateFromLanguage(language);

         var ocrResult = await engine.RecognizeAsync(bitmap).AsTask();

      }


   }
}