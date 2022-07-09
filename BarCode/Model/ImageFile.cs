using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
      public ImageFile(string fullPath)
         : base(fullPath)
      {
         Image = Image.FromFile(FullPath);
         ImageSize = new ImageSize(widthInPixels: Image.Width, heightInPixels: Image.Height, horizontalPixelsPerInch: Image.HorizontalResolution, verticalPixelsPerInch:Image.VerticalResolution);
      }

      public async Task<(bool success, string barCode)> GetBarCodeAsync()
      {
         var barCode = await ReadBarCode();

         if (string.IsNullOrEmpty(barCode))
         {
            return (false, barCode);
         }

         // try to extract starting from whatever that looks like numbers until the end
         var index = barCode.IndexOfAny("0123456789".ToCharArray());
         if (index < 0)
         {
            return (false, barCode);
         }

         string substring = barCode.Substring(index);

         // need to remove any spaces 
         var UPCCode = Regex.Replace(substring, @"\s+", "");

         // need to remove leading zeros
         UPCCode = UPCCode.TrimStart('0');

         // only select numbers
         if (Regex.IsMatch(UPCCode, @"^\d+$"))
         {
            return (true, UPCCode);
         }
         else
         {
            return (false, barCode);
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

         TraceBarCode.LogVerbose($"ReadBarCodeAsync '{FullPath}'", "OcrResult Text={0}, Lines.Count={1}", a.Text, a.Lines.Count);

         for (int i = 0; i < a.Lines.Count; i++)
         {
            var line = a.Lines[i];

            var words = string.Join(", ", line.Words.Select(x => x.Text));

            TraceBarCode.LogVerbose($"ReadBarCodeAsync '{FullPath}'", "OcrResult.Line[{0}] Text={1}, Words={2}", i, line.Text, words);
         }

         return a;
      }

   }
}