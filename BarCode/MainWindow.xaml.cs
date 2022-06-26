using Microsoft.Win32;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BarCode
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {

      public MainWindow()
      {
         InitializeComponent();

         this.Loaded += OnMainWindowLoaded;

         this.AllowDrop = true;
         this.Drop += OnMainWindowDrop;
      }

      private bool _Loading;
      private NewImageFile _NewImageFile;
      private ImageFile _ExistingImageFile;

      private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
      {
         _Loading = true;

         CrossReferenceFileName.Text = Properties.Settings.Default.CrossReferenceSpreadsheet;

         NewImageWidthInInches.Text = Properties.Settings.Default.ImageWidthInInches.ToString();
         NewImageHeightInInches.Text = Properties.Settings.Default.ImageHeightInInches.ToString();

         _Loading = false;
      }

      private void OnMainWindowDrop(object sender, DragEventArgs e)
      {
         if (e.Data.GetDataPresent(DataFormats.FileDrop))
         {
            // Note that you can have more than one file.
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            // Need to determine if folder or files

            // Assuming you have one file that you care about, pass it off to whatever
            // handling code you have defined.
            ProcessImage(files[0]);
         }
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
            Properties.Settings.Default.CrossReferenceSpreadsheet = filename;
            Properties.Settings.Default.Save();
         }
      }

      private void ProcessImage(string filename)
      {
         _ExistingImageFile = new ImageFile(filename);

         ReadReadBarCode();

         ExistingImageWidthInInches.Text = _ExistingImageFile.ImageSize.WidthInInchesRounded(2);
         ExistingImageHeightInInches.Text = _ExistingImageFile.ImageSize.HeightInInchesRounded(2);

         ExistingImageWidthInPixels.Text = _ExistingImageFile.ImageSize.WidthInPixels.ToString();
         ExistingImageHeightInPixels.Text = _ExistingImageFile.ImageSize.HeightInPixels.ToString();

         ExistingImageHorizontalRes.Text = _ExistingImageFile.HorizontalResolution.ToString();
         ExistingImageVerticalRes.Text = _ExistingImageFile.VerticalResolution.ToString();
         ImageFileName.Text = _ExistingImageFile.FullPath;

         var newFilename = Path.Combine(_ExistingImageFile.DirectoryName, _ExistingImageFile.FileNameWithoutExtension + "-revised" + _ExistingImageFile.Extension);

         var newFileExists = CheckIfNewFileExists(newFilename);

         if (newFileExists)
         {
            _NewImageFile = new NewImageFile(newFilename, _ExistingImageFile, NewImageWidthInInches.Text, NewImageHeightInInches.Text);

            NewImageFileName.Text = _NewImageFile.FullPath;

            var saved = _NewImageFile.SaveImage();
            if (saved)
            {
               MessageBox.Show($"'{newFilename}' was saved.", "Image saved", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
         }
      }

      private void ReadReadBarCode()
      {
         var task = Task.Factory.StartNew(() => _ExistingImageFile.ReadBarCodeAsync());

         Task.WaitAll();
   
      }

      private static bool CheckIfNewFileExists(string newFilename)
      {
         if (File.Exists(newFilename))
         {
            var messageBoxResult = MessageBox.Show($"'{newFilename}' already exists. Do you want to delete existing one?", "Image already exists.", MessageBoxButton.YesNo, MessageBoxImage.Stop, MessageBoxResult.Yes);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
               try
               {
                  File.Delete(newFilename);
                  return true;
               }
               catch
               {
                  var errorResult = MessageBox.Show($"'{newFilename}' is not able to deleted. Check if open and close.", "Image can't be deleted.", MessageBoxButton.OK, MessageBoxImage.Stop);
                  return false;
               }
            }
         }
         return true;
      }

      private void NewImageWidth_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
      {
         if (!_Loading)
         {
            var textBox = sender as TextBox;

            var newWidth = ImageSize.ConvertToInches(textBox.Text);

            if (newWidth != null)
            {
               Properties.Settings.Default.ImageWidthInInches = newWidth.Value;
               Properties.Settings.Default.Save();
            }
         }
      }

      private void NewImageHeight_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
      {
         if (!_Loading)
         {
            var textBox = sender as TextBox;

            var newHeight = ImageSize.ConvertToInches(textBox.Text);

            if (newHeight != null)
            {
               Properties.Settings.Default.ImageHeightInInches = newHeight.Value;
               Properties.Settings.Default.Save();
            }
         }
      }
   }
}
