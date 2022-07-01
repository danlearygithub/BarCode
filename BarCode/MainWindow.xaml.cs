using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

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
         this.DataContext = this;
      }

      private NewImageFile _NewImageFile;
      private ImageFile _ExistingImageFile;

      private CrossReferenceSpreadsheet _CrossReferenceSpreadsheet;

      private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
      {
         
      }

      private void Border_Drop(object sender, DragEventArgs e)
      {

         if (!string.IsNullOrEmpty(Settings.CrossReferenceSpreadsheet))
         {
            _CrossReferenceSpreadsheet = new CrossReferenceSpreadsheet(Settings.CrossReferenceSpreadsheet);

            if (!_CrossReferenceSpreadsheet.ValidSpreadsheet)
            {
               return;
            }
         }
         else
         {
            return;
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

               ProcessMultipleImages(files);
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

      private void ProcessOneImage(string filename)
      {
         (bool saved, string newFilename) = ProcessImage(filename);

         if (saved)
         {
            MessageBox.Show($"Created '{newFilename}'", $"Created '{newFilename}'", MessageBoxButton.OK, MessageBoxImage.Information);
         }
         else
         {
            MessageBox.Show($"Unable to create '{newFilename}' from '{filename}'", $"Unable to save '{newFilename}'", MessageBoxButton.OK, MessageBoxImage.Stop);
         }
      }
      private void ProcessMultipleImages(string[] files)
      {
         foreach (var filename in files)
         {
            if (!IsFolder(filename))
            {
               // only process files - not folders
               ProcessImage(filename);
            }
         }

         var msgBox = MessageBox.Show($"Processed {files.Length} images", "Processed multiple images", MessageBoxButton.OK, MessageBoxImage.Question);
      }


      private void ProcessFolder(string folderName)
      {
         var allFiles = GetFiles(folderName, Settings.SupportedImageFileTypes);

         var msgBox = MessageBox.Show($"Do you want to process {allFiles.Count} images in '{folderName}' folder?", $"Processing '{folderName}' folder", MessageBoxButton.OKCancel, MessageBoxImage.Question);

         if (msgBox == MessageBoxResult.OK)
         {
            // need to determine new folder for saving images

            var newFolder = folderName + "-revised";

            var newFolderExists = CheckIfFolderExists(newFolder);

            if (newFolderExists)
            {
               Directory.CreateDirectory(newFolder);

               foreach (var file in allFiles)
               {
                  ProcessImage(file, newFolder);
               }

               MessageBox.Show($"Created {allFiles.Count} images in '{newFolder}' folder", $"Created images in '{newFolder}' folder", MessageBoxButton.OK, MessageBoxImage.Information);

            }
         }
      }
      private static bool CheckIfFolderExists(string folder)
      {
         if (Directory.Exists(folder))
         {
            var messageBoxResult = MessageBox.Show($"'{folder}' already exists. Do you want to delete?", "Already exists.", MessageBoxButton.YesNo, MessageBoxImage.Stop, MessageBoxResult.Yes);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
               try
               {
                  Directory.Delete(folder, true);

                  return true;
               }
               catch (Exception ex)
               {
                  MessageBox.Show($"'{folder}' is not able to be deleted due to '{ex.Message}'.", "Can't be deleted.", MessageBoxButton.OK, MessageBoxImage.Stop);
                  return false;
               }
            }
            return false;
         }
         return true;
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

      private (bool saved, string newFilename) ProcessImage(string filename, string newDirectoryName = null)
      {
         var extension = Path.GetExtension(filename);

         if (Settings.ImageFileTypeIsSupported(extension))
         {
            _ExistingImageFile = new ImageFile(filename);

            var product = _CrossReferenceSpreadsheet.FindProduct(Settings.CrossReferenceSpreadsheet, _ExistingImageFile.BarCode);

            if (product != null)
            {
               string newFilename;
               if (newDirectoryName == null)
               {
                  newFilename = Path.Combine(_ExistingImageFile.DirectoryName, _ExistingImageFile.FileNameWithoutExtension + "-revised" + _ExistingImageFile.Extension);
               }
               else
               {
                  newFilename = Path.Combine(newDirectoryName, _ExistingImageFile.FileNameWithoutExtension + "-revised" + _ExistingImageFile.Extension);
               }

               var newFileExists = CheckIfNewFileExists(newFilename);

               if (newFileExists)
               {
                  _NewImageFile = new NewImageFile(newFilename, _ExistingImageFile);

                  return (_NewImageFile.SaveImage(), newFilename);
               }
               return (false, null);
            }
         }
         return (false, null);
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

      private static bool CheckIfNewFileExists(string newFilename)
      {
         if (File.Exists(newFilename))
         {
            var messageBoxResult = MessageBox.Show($"'{newFilename}' already exists. Do you want to delete?", "Already exists.", MessageBoxButton.YesNo, MessageBoxImage.Stop, MessageBoxResult.Yes);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
               try
               {
                  File.Delete(newFilename);
                  return true;
               }
               catch (Exception ex)
               {
                  MessageBox.Show($"'{newFilename}' is not able to be deleted due to '{ex.Message}'.", "Can't be deleted.", MessageBoxButton.OK, MessageBoxImage.Stop);
                  return false;
               }
            }
            return false;
         }
         return true;
      }

      public string DropMessage
      { 
         get 
         {
            return "Drop file, multiple files or folder here.";
         }
      }


   }
}
