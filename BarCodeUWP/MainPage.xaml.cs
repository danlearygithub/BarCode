using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BarCodeUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
      private Settings _Settings = new Settings();

      public MainPage()
        {
            this.InitializeComponent();

         this.Loaded += MainPage_Loaded;

         this.AllowDrop = true;
         this.Drop += MainPage_Drop;
      }

      private bool _Loading;
      //private NewImageFile _NewImageFile;
      //private ImageFile _ExistingImageFile;

      private void MainPage_Loaded(object sender, RoutedEventArgs e)
      {
         _Loading = true;

         CrossReferenceFileName.Text = _Settings.CrossReferenceSpreadsheet;

         //NewImageWidthInInches.Text = Properties.Settings.Default.ImageWidthInInches.ToString();
         //NewImageHeightInInches.Text = Properties.Settings.Default.ImageHeightInInches.ToString();

         _Loading = false;
      }

      private void MainPage_Drop(object sender, DragEventArgs e)
      {
         throw new NotImplementedException();
      }

   }
}
