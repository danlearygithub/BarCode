using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BarCode
{
   /// <summary>
   /// Interaction logic for BarCodeWindow.xaml
   /// </summary>
   public partial class BarCodeWindow : Window
   {
      public BarCodeWindow()
      {
         InitializeComponent();
      }

      private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
      {

      }

      private void OKButton_Click(object sender, RoutedEventArgs e)
      {
         DialogResult = true;
      }

      private void CancelButton_Click(object sender, RoutedEventArgs e)
      {
         DialogResult = false;
      }

      private string _Image;

      public string Image
      {
         get { return _Image; }
         set
         {
            _Image = value;
            ImageFileName.Text = value;
         }

      }

      public string BarCode => UPCBarCode.Text.Trim();

   }
}
