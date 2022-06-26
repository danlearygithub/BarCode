using System;
using System.Drawing;
using System.IO;

using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.Streams;

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