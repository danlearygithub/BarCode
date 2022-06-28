using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BarCodeUWP
{
   /// <summary>
   /// An empty page that can be used on its own or navigated to within a Frame.
   /// </summary>
   public sealed partial class MainPage : Page
   {
      private AppSettings _Settings = new AppSettings();

      public MainPage()
      {
         this.InitializeComponent();

         this.Loaded += MainPage_Loaded;
         this.AllowDrop = true;
      }

      private NewImageFile _NewImageFile;
      private ImageFile _ExistingImageFile;

      private void MainPage_Loaded(object sender, RoutedEventArgs e)
      {
         ApplicationView.PreferredLaunchViewSize = new Size(600, 500);
         ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
      }

      private async void Page_DragOver(object sender, DragEventArgs e)
      {
         if (e.DataView.Contains(StandardDataFormats.StorageItems))
         {
            var items = await e.DataView.GetStorageItemsAsync();

            if (items.Count > 0)
            {
               foreach (var item in items)
               {
                  if (item is StorageFolder)
                  {
                     var storageFolder = item as StorageFolder;

                     // find all files within folder
                     IReadOnlyList<StorageFile> fileList = await storageFolder.GetFilesAsync();

                     foreach (StorageFile file in fileList)
                     {
                        var softwareBitmap = await GetSoftwareBitmap(file);

                        if (softwareBitmap != null)
                        {
                           ProcessImage(softwareBitmap, file);
                        }
                     }

                  }
                  else if (item is StorageFile)
                  {
                     var file = item as StorageFile;

                     var softwareBitmap = await GetSoftwareBitmap(file);

                     if (softwareBitmap != null)
                     {
                        ProcessImage(softwareBitmap, file);
                     }


                  }
               }
            }
         }
      }

      private async Task<SoftwareBitmap> GetSoftwareBitmap(StorageFile storageFile)
      {
         if (_Settings.ImageFileTypeIsSupported(storageFile.FileType))
         {
            SoftwareBitmap softwareBitmap;

            using (IRandomAccessStream stream = await storageFile.OpenAsync(FileAccessMode.Read))
            {
               // Create the decoder from the stream
               BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

               // Get the SoftwareBitmap representation of the file
               softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            }
            return softwareBitmap;
         }
         else
         {
            await MessageBox.Show($"Can't process {storageFile} because not an image.", "Wrong file type", new List<string>() { "OK" });
            return null;
         }

      }
      private async void ProcessImage(SoftwareBitmap softwareBitmap, StorageFile storageFile)
      {

         _ExistingImageFile = new ImageFile(_Settings, softwareBitmap, storageFile);

         //ReadReadBarCode();

         ExistingImageWidthInInches.Text = _ExistingImageFile.ImageSize.WidthInInchesRounded(2);
         ExistingImageHeightInInches.Text = _ExistingImageFile.ImageSize.HeightInInchesRounded(2);

         ExistingImageWidthInPixels.Text = _ExistingImageFile.ImageSize.WidthInPixels.ToString();
         ExistingImageHeightInPixels.Text = _ExistingImageFile.ImageSize.HeightInPixels.ToString();

         ExistingImageHorizontalRes.Text = _ExistingImageFile.HorizontalResolution.ToString();
         ExistingImageVerticalRes.Text = _ExistingImageFile.VerticalResolution.ToString();
         ImageFileName.Text = _ExistingImageFile.FullPath;

         var newFilename = Path.Combine(_ExistingImageFile.DirectoryName, _ExistingImageFile.FileNameWithoutExtension + "-revised" + _ExistingImageFile.Extension);

         var proceed = await CheckIfNewFileExists(newFilename);

         if (proceed)
         {
            _NewImageFile = new NewImageFile(newFilename, _Settings, _ExistingImageFile, NewImageWidthInInches.Text, NewImageHeightInInches.Text);

            // check to see if can be overwritten
            var fileExists = await CheckIfNewFileExists(_NewImageFile.FullPath);

            if (fileExists)
            {
               var resized = await _NewImageFile.ResizeImage();
               if (resized)
               {
                  NewImageFileName.Text = _NewImageFile.FullPath;
               }
               else
               {
                  // need to do this
               }
            }



            //var saved = _NewImageFile.SaveImage();

            if (false)
            {
               //var messageBox = new MessageBox($"'{newFilename}' was saved.", "Image saved", new List<string>() { "OK" });

               //var cmd = messageBox.Show();

               //if (cmd.Result == "OK")
               //{
               //   ;
               //}
            }
         }
      }

      private async Task<bool> CheckIfNewFileExists(string newFilename)
      {
         if (File.Exists(newFilename))
         {
            var cmd = await MessageBox.Show($"'{newFilename}' already exists. Do you want to delete existing one?", "Image already exists.", buttonLabels: new List<string>() { "Yes", "No" });

            if (cmd == "Yes")
            {
               try
               {
                  File.Delete(newFilename);
                  return true;
               }
               catch
               {
                  await MessageBox.Show($"'{newFilename}' is not able to deleted. Check if open and close.", "Image can't be deleted.", buttonLabels: new List<string>() { "OK" });
                  return false;
               }
            }
         }
         return true;
      }
      private void OpenCrossReferenceButton_Click(object sender, RoutedEventArgs e)
      {
         //var openDialog = new OpenFileDialog();
         //openDialog.DefaultExt = ".xlsx";
         //openDialog.Filter = "Excel Worksheets|*.xlsx|Excel Worksheets|*.xlsm|Excel Worksheets|*.xls";

         //CrossReferenceFileName.Text = "";
         //var result = openDialog.ShowDialog();

         //if (result == true)
         //{
         //   var filename = openDialog.FileName;

         //   CrossReferenceFileName.Text = filename;
         //   Properties.Settings.Default.CrossReferenceSpreadsheet = filename;
         //   Properties.Settings.Default.Save();
         //}
      }


   }
}
