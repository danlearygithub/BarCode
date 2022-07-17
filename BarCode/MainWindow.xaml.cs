using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;

namespace BarCode
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {

      public AppSettings Settings = new AppSettings();

      public MainWindow()
      {
         InitializeComponent();

         this.Loaded += OnMainWindowLoaded;
         _Window.Closing += MainWindow_Closing;
         this.DataContext = this;

         //Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
         //Debug.AutoFlush = true;
         //Debug.Indent();

         if (!string.IsNullOrEmpty(Settings.CrossReferenceSpreadsheet))
         {
            _CrossReferenceSpreadsheet = new CrossReferenceSpreadsheet(Settings, _Console);
         }
      }

      private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
      {
         if (_CrossReferenceSpreadsheet != null)
         {
            _CrossReferenceSpreadsheet.Close();
         }
      }

      private NewImageFile _NewImageFile;
      private ImageFile _ExistingImageFile;

      private CrossReferenceSpreadsheet _CrossReferenceSpreadsheet;

      private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
      {
         ProgressBar.Visibility = Visibility.Hidden;
      }

      private async void Border_Drop(object sender, DragEventArgs e)
      {
         if (_CrossReferenceSpreadsheet.IsInCorrectFormat != SpreadsheetFormatResult.Good)
         {
            return;
         }

         if (AutoClearLog.IsChecked.Value)
         {
            _Console.Clear();
         }

         if (e.Data.GetDataPresent(DataFormats.FileDrop))
         {
            // Note that you can have more than one file.
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length == 1)
            {
               var filename = files[0];

               bool isFolder = IsFolder(filename);

               // only allow processing of one folder
               if (isFolder)
               {
                  ProcessFolder(filename);
               }
               else
               {
                  // one file
                  ProcessOneImage(filename);
               }
            }
            else
            {
               await ProcessMultipleImages(files);
            }
         }
      }

      private static bool IsFolder(string filename)
      {
         FileAttributes attr = File.GetAttributes(filename);

         bool isFolder;

         if (attr.HasFlag(FileAttributes.Directory))
         {
            isFolder = true;
         }
         else
         {
            isFolder = false;
         }

         return isFolder;
      }

      private async void ProcessOneImage(string filename)
      {
         (ProcessImageResult processImageResult, string newFullPath, Product product) = await ProcessImage(filename);

         ShowResults(filename, processImageResult, newFullPath, product);
      }

      private async Task ProcessMultipleImages(string[] files)
      {
         ProgressBar.Visibility = Visibility.Visible;

         _Console.WriteInfoLine($"Processing {files.Length} images");
         TraceBarCode.LogInfo("ProcessMultipleImages", $"Processing {files.Length} images");

         int savedFileCount = 0;

         ProgressBar.Maximum = files.Length;

         foreach (var filename in files)
         {
            if (!IsFolder(filename))
            {
               ProgressBar.Value++;

               // only process files - not folders
               (ProcessImageResult processImageResult, string newFullPath, Product product) = await ProcessImage(filename);

               ShowResults(filename, processImageResult, newFullPath, product);

               if (processImageResult == ProcessImageResult.Saved)
               {
                  savedFileCount++;
               }
            }
         }

         if (savedFileCount != files.Length)
         {
            var message = $"Saved {savedFileCount} images out of {files.Length} total images. {files.Length - savedFileCount} not saved.";
            _Console.WriteRedInfoLine(message);
            TraceBarCode.LogError("ProcessMultipleImages", message);
         }
         else
         {
            var message = $"Saved {savedFileCount} images out of {files.Length} total images.";
            _Console.WriteGreenInfoLine(message);
            TraceBarCode.LogInfo("ProcessMultipleImages", message);

         }

         ProgressBar.Visibility = Visibility.Hidden;
      }

      private async void ProcessFolder(string folderName)
      {
         var allFiles = GetFiles(folderName, Settings.SupportedImageFileTypes);

         var msgBox = MessageBox.Show($"Do you want to process {allFiles.Count} images in '{folderName}' folder?", $"Processing '{folderName}' folder", MessageBoxButton.OKCancel, MessageBoxImage.Question);

         if (msgBox == MessageBoxResult.OK)
         {
            ProgressBar.Visibility = Visibility.Visible;

            // need to determine new folder for saving images

            int savedFileCount = 0;

            _Console.WriteInfoLine($"Processing {allFiles.Count} images");
            TraceBarCode.LogInfo(folderName, $"Processing {allFiles.Count} images");

            ProgressBar.Maximum = allFiles.Count;

            foreach (var file in allFiles)
            {
               ProgressBar.Value++;

               (ProcessImageResult processImageResult, string newFullPath, Product product) = await ProcessImage(file);

               ShowResults(file, processImageResult, newFullPath, product);

               if (processImageResult == ProcessImageResult.Saved)
               {
                  savedFileCount++;
               }
            }

            if (savedFileCount != allFiles.Count)
            {
               var message = $"Saved {savedFileCount} images out of {allFiles.Count} total images.";
               _Console.WriteRedInfoLine(message);
               TraceBarCode.LogError(folderName, message);
            }
            else
            {
               var message = $"Saved {savedFileCount} images out of {allFiles.Count} total images. Missed {allFiles.Count - savedFileCount}";
               _Console.WriteGreenInfoLine(message);
               TraceBarCode.LogInfo(folderName, message);

            }
         }

         ProgressBar.Visibility = Visibility.Hidden;
      }

      private void ShowResults(string filename, ProcessImageResult processImageResult, string newFullPath, Product product)
      {
         switch (processImageResult)
         {
            case ProcessImageResult.NotSaved:
               _Console.WriteRedInfoLine($"Unable to save to '{newFullPath}'");
               break;

            case ProcessImageResult.Saved:
               _Console.WriteGreenInfoLine($"UPC Code = '{product.UPC}'. Saved to '{newFullPath}'.");
               break;

            case ProcessImageResult.UnableToDetermineBarCode:
               // do nothing
               break;

            case ProcessImageResult.UnableToFindBarCode:
              //_Console.WriteRedInfoLine($"Unable to find bar code from '{newFullPath}'");
               break;
            case ProcessImageResult.UnSupportedFileType:
               // do nothing - popup already happened
               break;

            default:
               _Console.WriteRedInfoLine($"'{processImageResult}' not supported.");
               break;
         }
      }
      public IList<string> GetFiles(string folderName, IList<string> searchPatterns)
      {
         List<string> files = new List<string>();

         foreach (string sp in searchPatterns)
         {
            string pattern = sp;

            if (!sp.StartsWith("*"))
            {
               pattern = $"*{sp}";
            }

            files.AddRange(System.IO.Directory.GetFiles(folderName, pattern, SearchOption.TopDirectoryOnly));
         }

         files.Sort();

         return files;
      }

      public enum ProcessImageResult
      {
         NotSaved,
         Saved,
         UnableToDetermineBarCode,
         UnableToFindBarCode,
         UnSupportedFileType
      }

      private async Task<(ProcessImageResult processImageResult, string newFullPath, Product product)> ProcessImage(string filename)
      {
         ProcessImageResult processImageResult;
         string newFullPath;

         var extension = Path.GetExtension(filename);

         if (Settings.ImageFileTypeIsSupported(extension))
         {
            _Console.WriteInfoLine($"Processing '{filename}'");

            _ExistingImageFile = new ImageFile(filename);

            //(bool success, string barCode) = _ExistingImageFile.GetBarCode();
            (bool success, string rawBarCode, List<string> barCodes) = await _ExistingImageFile.GetBarCodeAsync();

            if (success == false)
            {
               var m = $"Unable to determine barcode. Bar code read = '{rawBarCode}'";

               _Console.WriteRedInfoLine(filename, m);
               TraceBarCode.LogError(filename, m);

               (processImageResult, newFullPath) = ProcessUnknownBarCode(filename);

               _Console.WriteGreenInfoLine($"Saved to '{newFullPath}'.");

               return (ProcessImageResult.UnableToDetermineBarCode, newFullPath, null);
            }

            var newBarCodes = barCodes.ToList();

            // add one more with numbers on front and end and both
            foreach (var barCode in barCodes)
            {
               for (int i = 0; i < 10; i++)
               {
                  var front = i + 1;

                  newBarCodes.Add(front + barCode);
                  newBarCodes.Add(barCode + i);
                  newBarCodes.Add(front + barCode + i);
               }
            }

           

            foreach (var barCode in newBarCodes)
            {
               var tryMessage = $"Trying to find '{barCode}'";

               _Console.WriteAttemptLine(tryMessage);
               TraceBarCode.LogVerbose(filename, tryMessage);

               (SpreadsheetResult result, Product product) = _CrossReferenceSpreadsheet.FindProduct(filename, barCode);

               if (result == SpreadsheetResult.Good)
               {
                  (processImageResult, newFullPath) = ProcessBarCode(filename, product);

                  _ExistingImageFile = null;
                  _NewImageFile = null;

                  return (processImageResult, newFullPath, product);
               }
            }

            var message = $"Unable to determine barcode. Bar code read = '{rawBarCode}'";
            _Console.WriteRedInfoLine(filename, message);
            TraceBarCode.LogError(filename, message);

            (processImageResult, newFullPath) = ProcessUnknownBarCode(filename);

            _Console.WriteGreenInfoLine($"Saved to '{newFullPath}'.");

            return (ProcessImageResult.UnableToFindBarCode, newFullPath, null);
         }
         else
         {
            var message = $"Image '{filename}' type {extension} is not supported. Only {Settings.SupportedImageFileTypesString} are supported.";

            _Console.WriteRedInfoLine(filename, message);
            MessageBox.Show(message, "Unsupported image type", MessageBoxButton.OK, MessageBoxImage.Error);

            _ExistingImageFile = null;
            _NewImageFile = null;

            return (ProcessImageResult.UnSupportedFileType, null, null);
         }
      }

      private const string UNKNOWN_FOLDER = "Unknown Bar Codes";

      private (ProcessImageResult processImageResult, string newFullPath) ProcessBarCode(string filename, Product product)
      {
         var newFolder = Path.Combine(Directory.GetParent(filename).Parent.FullName, product.Vendor);
         var newFullPath = Path.Combine(newFolder, $"{product.Vendor}_{product.RegisDescription}{_ExistingImageFile.Extension}");

         return ProcessBarCode(filename, newFolder, newFullPath, product);
      }

      private (ProcessImageResult processImageResult, string newFullPath) ProcessUnknownBarCode(string filename)
      {
         var newFolder = Path.Combine(Directory.GetParent(filename).Parent.FullName, UNKNOWN_FOLDER);
         var newFullPath = Path.Combine(newFolder, $"{_ExistingImageFile.FileName}");

         return ProcessBarCode(filename, newFolder, newFullPath, null);
      }

      private (ProcessImageResult processImageResult, string newFullPath) ProcessBarCode(string filename, string newFolder, string newFullPath, Product product)
      {
         if (!Directory.Exists(newFolder))
         {
            Directory.CreateDirectory(newFolder);
         }

         var deletedOrDoesNotExist = DeleteFileIfExists(newFullPath);

         if (deletedOrDoesNotExist)
         {
            _NewImageFile = new NewImageFile(newFullPath, _ExistingImageFile, Settings, product);

            (ImageResult imageResult, string exceptionMessage) = _NewImageFile.ResizeImage();

            if (imageResult == ImageResult.Saved)
            {
               return (ProcessImageResult.Saved, newFullPath);
            }
            else
            {
               if (!string.IsNullOrEmpty(exceptionMessage))
               {

                  MessageBox.Show($"Exception occurred when processing '{filename}'. Message: {exceptionMessage}", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
               }

               return (ProcessImageResult.NotSaved, newFullPath);
            }
         }

         return (ProcessImageResult.NotSaved, newFullPath);
      }

      private bool DeleteFileIfExists(string newFilename)
      {
         if (File.Exists(newFilename))
         {
            try
            {
               File.Delete(newFilename);
               return true;
            }
            catch (Exception ex)
            {
               var message = $"'{newFilename}' is not able to be deleted due to '{ex.Message}'.";

               _Console.WriteRedInfoLine(newFilename, message);
               MessageBox.Show(message, "Can't be deleted.", MessageBoxButton.OK, MessageBoxImage.Stop);

               return false;
            }
         }
         return true;
      }

      private void OpenCrossReferenceButton_Click(object sender, RoutedEventArgs e)
      {
         var openDialog = new OpenFileDialog();
         openDialog.DefaultExt = ".xlsx";
         openDialog.Filter = "Excel Worksheets|*.xlsx|Excel Worksheets|*.xlsm|Excel Worksheets|*.xls";

         CrossReferenceFileName.Text = "";
         var result = openDialog.ShowDialog();

         if (result == true)
         {
            var filename = openDialog.FileName;

            CrossReferenceFileName.Text = filename;
            Settings.CrossReferenceSpreadsheet = filename;
         }
      }

      public string DropMessage
      {
         get
         {
            if (string.IsNullOrEmpty(CrossReferenceFileName.Text))
            {
               return "Select Cross Reference spreadsheet first.";
            }

            if (_CrossReferenceSpreadsheet is null)
            {
               return "_CrossReferenceSpreadsheet error";
            }


            switch (_CrossReferenceSpreadsheet.IsInCorrectFormat)
            {
               case SpreadsheetFormatResult.TooManyWorksheets:
                  return "There are too many worksheets in cross reference spreadsheet. There should be only 1.";

               case SpreadsheetFormatResult.UPCColumnMissing:
                  return $"UPC column is missing from cross reference spreadsheet. Check column name.\n Existing Headings = {_CrossReferenceSpreadsheet.ColumnHeadings.ToString()}";

               case SpreadsheetFormatResult.VendorColumnMissing:
                  return $"Vendor column is missing from cross reference spreadsheet. Check column name.\n Existing Headings = {_CrossReferenceSpreadsheet.ColumnHeadings.ToString()}";

               case SpreadsheetFormatResult.DescriptionColumnMissing:
                  return $"Description column is missing from cross reference spreadsheet. Check column name.\n Existing Headings = {_CrossReferenceSpreadsheet.ColumnHeadings.ToString()}";

               case SpreadsheetFormatResult.RegisDescriptionColumnMissing:
                  return $"Regis Description column is missing from cross reference spreadsheet. Check column name.\n Existing Headings = {_CrossReferenceSpreadsheet.ColumnHeadings.ToString()}";
            }

            return "Drop file, multiple files or folder here.";
         }
      }

      private void ClearLog_Click(object sender, RoutedEventArgs e)
      {
         _Console.Clear();
      }
   }
}
