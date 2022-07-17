using Aspose.BarCode.BarCodeRecognition;
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

      public async Task<(bool success, string rawBarCode, List<string> barCodes)> GetBarCodeAsync()
      {
         //var barCode = ReadBarCodeUsingIronOcr();
         //var newBarCode = ProcessBarCode("", barCode);

         var barCode = await ReadBarCodeAsync();
         var newBarCode = ProcessBarCode(barCode.text, barCode.lines);

         return newBarCode;
      }

      internal (bool success, string rawText, List<string> barCodes) ProcessBarCode(string rawText, IList<string> rawLines)
      {
         TraceBarCode.LogVerbose($"ProcessBarCode '{FullPath}'", $"rawLines.Count='{rawLines.Count}'");

         bool success = false;
         List<string> barCodes = new List<string>();

         foreach (var rawLine in rawLines)
         {
            if (string.IsNullOrEmpty(rawLine))
            {
               break;
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
               continue;
            }

            modified = modified.Substring(startingIndexOfNumbers);

            // need to remove leading zeros
            modified = modified.TrimStart('0');

            if (modified.Length < MIN_BARCODE_LENGTH)
            {
               continue;
            }

            // only select numbers
            if (Regex.IsMatch(modified, @"^\d+$"))
            {
               success = true;
               barCodes.Add(modified);
            }
         }

         return (success, rawText, barCodes.Distinct().ToList());
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
            lines.Add(a.Text);

            TraceBarCode.LogVerbose($"ReadBarCodeAsync '{FullPath}'", "OcrResult Text={0}, Lines.Count={1}", a.Text, a.Lines.Count);

            // add forward 
            for (int i = 0; i < a.Lines.Count; i++)
            {
               var line = a.Lines[i];

               lines.Add(line.Text);

               if (TraceBarCode.SourceLevel >= SourceLevels.Verbose)
               {
                  var words = string.Join(", ", line.Words.Select(x => x.Text));
                  TraceBarCode.LogVerbose($"ReadBarCodeAsync '{FullPath}'", "OcrResult.Line[{0}] Text={1}, Words={2}", i, line.Text, words);
               }
            }

            // add backward
            for (int i = a.Lines.Count - 1; i >= 0; i--)
            {
               var line = a.Lines[i];

               lines.Add(line.Text);
            }
            text = a.Text;

            stream.Close();
         }

         return (text, lines);
      }

      private string ReadBarCodeUsingIronOcr()
      {
         string text = "";

         using (BarCodeReader reader = new BarCodeReader(FullPath))
         {
            foreach (BarCodeResult result in reader.ReadBarCodes())
            {
               text = result.CodeText;
            }

         }
         return text;
      }

      //internal string ProcessOcrLines(List<string> lines)
      //{
      //   var sb = new StringBuilder();

      //   for (int i = 0; i < lines.Count; i++)
      //   {
      //      sb.Append(lines[i]);

      //      if (line.Length >= MIN_BARCODE_LENGTH)
      //      {
      //         var lineWithoutSpaces = Regex.Replace(line, @"\s+", "");

      //         //Check if only numbers and spaces 
      //         var matchFound = Regex.IsMatch(lineWithoutSpaces, @"^\d+$");

      //         if (matchFound)
      //         {
      //            return lineWithoutSpaces;
      //         }
      //      }
      //   }

      //   return string.Empty;
      //}

      //private string ReadBarCodeUsingTesseract()
      //{
      //   string text;

      //   try
      //   {
      //      var ocr = new TesseractEngine("tessdata", "eng", EngineMode.TesseractOnly);

      //      var bitmap = new Bitmap(FullPath);

      //      using (var image = PixConverter.ToPix(bitmap))
      //      {
      //         using (var page = ocr.Process(image, PageSegMode.SingleLine))
      //         {
      //            page.RegionOfInterest = new Rect(1, 1, image.Width, image.Height);
      //            text = page.GetText();
      //         }
      //      }
      //      return text;

      //   }
      //   catch (Exception)
      //   {
      //      return null;
      //   }

      //}
   }
}