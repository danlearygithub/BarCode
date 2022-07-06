using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.Streams;

namespace BarCode
{
   public class ImageFile : ImageFileBase
   {
      //public string BarCode { get; private set; }

      public ImageFile(string fullPath)
         : base(fullPath)
      {
         Image = Image.FromFile(FullPath);
         ImageSize = new ImageSize(Image.Width, Image.Height, Image.HorizontalResolution, Image.VerticalResolution);
      }

      public string BarCode
      {
         get
         {
            var readTask = Task.Factory.StartNew(() => ReadBarCode());
            readTask.Wait();

            var barCode = readTask.Result.Result;

            if (Regex.IsMatch(barCode, @"^\d+$"))
            {
               return barCode;
            }
            else
            {
               return null;
            }
         }
      }

      private async Task<string> ReadBarCode()
      {
         var ocrResult = await ReadBarCodeAsync();

         return ocrResult.Text;
      }

      // https://medium.com/dataseries/using-windows-10-built-in-ocr-with-c-b5ca8665a14e
      public async Task<OcrResult> ReadBarCodeAsync()
      {
         var language = new Language("en");

         var stream = File.OpenRead(FullPath);

         var decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());

         var bitmap = await decoder.GetSoftwareBitmapAsync();

         var engine = OcrEngine.TryCreateFromLanguage(language);

         var a = await engine.RecognizeAsync(bitmap).AsTask();
         return a;
      }

   }
}