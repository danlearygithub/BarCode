using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

// https://stackoverflow.com/questions/51546880/groupbox-control-in-uwp

namespace BarCodeUWP
{
   public sealed partial class GroupBoxControl : UserControl
   {
      public GroupBoxControl()
      {
         this.InitializeComponent();
      }

      public string Header
      {
         get { return (string)GetValue(HeaderProperty); }
         set { SetValue(HeaderProperty, value); }
      }

      // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty HeaderProperty =
          DependencyProperty.Register("Header", typeof(string), typeof(GroupBoxControl), new PropertyMetadata("Your Header", HeaderPropertyChangedCallback));

      public static void HeaderPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         if (e.NewValue != e.OldValue)
         {
            (d as GroupBoxControl).HeaderTitle.Text = e.NewValue?.ToString();
         }
      }

      public object CustomContent
      {
         get { return (object)GetValue(CustomContentProperty); }
         set { SetValue(CustomContentProperty, value); }
      }

      // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty CustomContentProperty =
          DependencyProperty.Register("CustomContent", typeof(object), typeof(GroupBoxControl), new PropertyMetadata(null, PropertyChangedCallback));

      public static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         if (e.NewValue != e.OldValue)
         {
            (d as GroupBoxControl).Content.Content = e.NewValue;
         }
      }

      private void HeaderTitle_LayoutUpdated(object sender, object e)
      {
         border.Margin = new Thickness(HeaderTitle.ActualWidth + 10, 10, 3, 3);
      }
   }
}
