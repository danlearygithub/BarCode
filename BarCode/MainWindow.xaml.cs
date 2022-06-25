using Microsoft.Win32;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BarCode
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      private int _ImageWidth;
      private int _ImageHeight;

      public MainWindow()
      {
         InitializeComponent();

         this.Loaded += OnMainWindowLoaded;
         this.AllowDrop = true;
         this.Drop += OnMainWindowDrop;
      }

      private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
      {
         CrossReferenceFileName.Text = Properties.Settings.Default.CrossReferenceSpreadsheet;
         NewImageWidth.Text = Properties.Settings.Default.ImageWidth.ToString();
         NewImageHeight.Text = Properties.Settings.Default.ImageHeight.ToString();
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
         var imageFile = new ImageFile(filename);


         ExistingImageWidth.Text = imageFile.Width.ToString();
         ExistingImageHeight.Text = imageFile.Height.ToString();

//         var newImage = imageFile.ResizeImage(_ImageWidth, _ImageHeight);

         //BitmapEncoder encoder = new PngBitmapEncoder();
         //encoder.Frames.Add(BitmapFrame.Create(image));

         //using (var fileStream = new System.IO.FileStream(_ImageFile.Filename + "\resized", System.IO.FileMode.Create))
         //{
         //   encoder.Save(fileStream);
         //}
      }

      private int ConvertToInt(string number)
      {
         return Int32.Parse(number);
      }

      private int ImageWidth(string width)
      {
         _ImageWidth = ConvertToInt(width);
         return _ImageWidth;
      }

      private int ImageHeight(string height)
      {
         _ImageHeight = ConvertToInt(height);
         return _ImageHeight;
      }


      private void NewImageWidth_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
      {
         Properties.Settings.Default.ImageWidth = ImageWidth(NewImageWidth.Text);
         Properties.Settings.Default.Save();
      }

      private void NewImageHeight_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
      {
         Properties.Settings.Default.ImageHeight = ImageHeight(NewImageHeight.Text);
         Properties.Settings.Default.Save();
      }
   }
}
