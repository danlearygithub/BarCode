using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using IronOcr;

[assembly: InternalsVisibleToAttribute("BarCodeTest")]

namespace BarCode
{
   public class ImageFile : ImageFileBase
   {
      private int MIN_BARCODE_LENGTH = 6;

      public ImageFile(string fullPath)
         : base(fullPath)
      {
         Image = Image.FromFile(FullPath);
         ImageSize = new ImageSize(widthInPixels: Image.Width, heightInPixels: Image.Height, horizontalPixelsPerInch: Image.HorizontalResolution, verticalPixelsPerInch: Image.VerticalResolution);
      }

      // unit testing only
      internal ImageFile()
         : base("fullPath")
      {
      }

      public async Task<(bool success, string rawBarCode, string modifiedText, List<string> barCodes)> GetBarCodeAsync()
      {
         var barCode = await ReadBarCodeAsync();
         var newBarCode = ProcessBarCode(barCode.text, barCode.lines);

         return newBarCode;
      }

      internal (bool success, string rawText, string modifiedText, List<string> barCodes) ProcessBarCode(string rawText, IList<string> rawLines)
      {
         TraceBarCode.LogVerbose($"ProcessBarCode '{FullPath}'", $"rawLines.Count='{rawLines.Count}'");

         bool success;
         string modifiedBarCodeText;
         List<string> barCodes = new List<string>();

         (success, modifiedBarCodeText) = Process(rawText);

         foreach (var rawLine in rawLines)
         {
            (bool processRawLineSuccess, string modifiedBarCode) = Process(rawLine);

            if (processRawLineSuccess == true)
            {
               barCodes.Add(modifiedBarCode);
            }
         }

         var distinctBarCodes = barCodes.Distinct().ToList();

         return (success, rawText, modifiedBarCodeText, distinctBarCodes);
      }

      private (bool success, string modifiedBarCode) Process(string rawLine)
      {
         if (string.IsNullOrEmpty(rawLine))
         {
            return (false, null);
         }

         var modified = Regex.Replace(rawLine, @"\n", ""); // remove new line
         modified = Regex.Replace(modified, @"\r", ""); // remove return
         modified = Regex.Replace(modified, "|", ""); // remove vertical
         modified = Regex.Replace(modified, @"[a-zA-Z]", ""); // remove letters
         modified = Regex.Replace(modified, @"\s", ""); // remove spaces
         modified = Regex.Replace(modified, "[.]", "");
         modified = Regex.Replace(modified, "[*]", "");

         // try to extract starting from whatever that looks like numbers until the end
         var startingIndexOfNumbers = modified.IndexOfAny("0123456789".ToCharArray());
         if (startingIndexOfNumbers < 0)
         {
            return (false, null);
         }

         modified = modified.Substring(startingIndexOfNumbers);

         // need to remove leading zeros
         modified = modified.TrimStart('0');

         if (modified.Length < MIN_BARCODE_LENGTH)
         {
            return (false, null);
         }

         // only select numbers
         if (Regex.IsMatch(modified, @"^\d+$"))
         {
            return (true, modified);
         }

         return (false, null);
      }

      // https://medium.com/dataseries/using-windows-10-built-in-ocr-with-c-b5ca8665a14e
      private async Task<(string text, IList<string> lines)> ReadBarCodeAsync()
      {
         string text;

         IList<string> lines = new List<string>();

         var language = new Language("en");

         using (var stream = File.OpenRead(FullPath))
         {
            var decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());

            var bitmap = await decoder.GetSoftwareBitmapAsync();

            var engine = OcrEngine.TryCreateFromLanguage(language);

            var a = await engine.RecognizeAsync(bitmap).AsTask();

            TraceBarCode.LogVerbose($"ReadBarCodeAsync '{FullPath}'", "OcrResult Text={0}, Lines.Count={1}", a.Text, a.Lines.Count);

            lines = ProcessOcrLines(a);

            text = a.Text;

            stream.Close();
         }

         return (text, lines);
      }

      private IList<string> ProcessOcrLines(Windows.Media.Ocr.OcrResult ocrResult)
      {
         IList<string> lines = new List<string>();

         lines.Add(ocrResult.Text);

         // add forward 
         for (int i = 0; i < ocrResult.Lines.Count; i++)
         {
            var line = ocrResult.Lines[i];

            lines.Add(line.Text);

            if (TraceBarCode.SourceLevel >= SourceLevels.Verbose)
            {
               var words = string.Join(", ", line.Words.Select(x => x.Text));
               TraceBarCode.LogVerbose($"ProcessOcrLines '{FullPath}'", "OcrResult.Line[{0}] Text={1}, Words={2}", i, line.Text, words);
            }
         }
         // add backward
         for (int i = ocrResult.Lines.Count - 1; i >= 0; i--)
         {
            var line = ocrResult.Lines[i];

            lines.Add(line.Text);
         }

         return lines;
      }


      //public (bool success, string rawBarCode, List<string> barCodes) GetBarCodeUsingIronOcr()
      //{
      //   var barCode = ReadBarCodeUsingIronOcr();
      //   var newBarCode = ProcessBarCode(barCode.text, barCode.lines);

      //   return newBarCode;
      //}

      //private (string text, IList<string> lines) ReadBarCodeUsingIronOcr()
      //{
      //   IList<string> lines = new List<string>();

      //   var ocr = new IronTesseract();
      //   ocr.Language = OcrLanguage.English;
      //   ocr.Configuration.TesseractVersion = TesseractVersion.Tesseract5;

      //   using (var Input = new OcrInput(FullPath))
      //   {
      //      var Result = ocr.Read(Input);
            
      //         lines = ProcessOcrLinesForIronOcr(Result);

      //      Console.WriteLine(Result.Text);
      //      return (Result.Text, lines) ;
      //   }
      //}

      //private IList<string> ProcessOcrLinesForIronOcr(IronOcr.OcrResult ocrResult)
      //{
      //   IList<string> lines = new List<string>();

      //   lines.Add(ocrResult.Text);

      //   // add forward 
      //   for (int i = 0; i < ocrResult.Lines.Length; i++)
      //   {
      //      var line = ocrResult.Lines[i];

      //      lines.Add(line.Text);

      //      if (TraceBarCode.SourceLevel >= SourceLevels.Verbose)
      //      {
      //         var words = string.Join(", ", line.Words.Select(x => x.Text));
      //         TraceBarCode.LogVerbose($"ProcessOcrLinesForIronOcr '{FullPath}'", "OcrResult.Line[{0}] Text={1}, Words={2}", i, line.Text, words);
      //      }
      //   }
      //   // add backward
      //   for (int i = ocrResult.Lines.Length - 1; i >= 0; i--)
      //   {
      //      var line = ocrResult.Lines[i];

      //      lines.Add(line.Text);
      //   }

      //   return lines;
      //}
   }
}