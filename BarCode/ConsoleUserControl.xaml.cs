using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace BarCode
{

   public interface IConsole
   {
      void Clear();

      void Write(string message, params object[] parameters);

      void WriteInfoLine(string message, params object[] parameters);
      void WriteInfoLine(string fullPath, string message);
      void WriteInfoLineWithTime(string message, params object[] parameters);

      void WriteAttemptLine(string message, params object[] parameters);

      void WriteGreenInfoLine(string message, params object[] parameters);
      void WriteGreenInfoLine(string fullPath, string message);
      void WriteRedInfoLine(string message, params object[] parameters);
      void WriteRedInfoLine(string fullPath, string message);

   }

   /// <summary>
   /// Interaction logic for ConsoleUserControl.xaml
   /// </summary>
   public partial class ConsoleUserControl : UserControl, IConsole
   {
      public ConsoleUserControl()
      {
         InitializeComponent();
      }

      public void Clear()
      {
         this.Dispatcher.Invoke(() =>
         {
            _Panel.Children.Clear();
         });
      }

      public void Write(string message, params object[] parameters)
      {
         this.Dispatcher.Invoke(() =>
         {
            _Panel.Children.Clear();

            _Panel.Children.Add(CreateTextBlock(string.Format(message, parameters), Colors.Black));
         });
      }

      public void WriteInfoLine(string message, params object[] parameters)
      {
         this.Dispatcher.Invoke(() =>
         {
            _Panel.Children.Add(CreateTextBlock(string.Format(message, parameters), Colors.Black));
         });
      }

      public void WriteInfoLine(string fullPath, string message)
      {
         this.Dispatcher.Invoke(() =>
         {
            _Panel.Children.Add(CreateTextBlock($"'{fullPath}': {message}", Colors.Black));
         });
      }
      
      public void WriteInfoLineWithTime(string message, params object[] parameters)
      {
         this.Dispatcher.Invoke(() =>
         {
            _Panel.Children.Add(CreateTextBlock(DateTime.Now + ": " + string.Format(message, parameters), Colors.Black));
         });
      }

      public void WriteGreenInfoLine(string message, params object[] parameters)
      {
         this.Dispatcher.Invoke(() =>
         {
            _Panel.Children.Add(CreateTextBlock(string.Format(message, parameters), Colors.Green));
         });
      }
      public void WriteGreenInfoLine(string fullPath, string message)
      {
         this.Dispatcher.Invoke(() =>
         {
            _Panel.Children.Add(CreateTextBlock($"'{fullPath}': {message}", Colors.Green));
         });
      }

      public void WriteRedInfoLine(string message, params object[] parameters)
      {
         this.Dispatcher.Invoke(() =>
         {
            _Panel.Children.Add(CreateTextBlock(string.Format(message, parameters), Colors.Red));
         });
      }

      public void WriteRedInfoLine(string fullPath, string message)
      {
         this.Dispatcher.Invoke(() =>
         {
            _Panel.Children.Add(CreateTextBlock($"'{fullPath}': {message}", Colors.Red));
         });
      }

      public void WriteAttemptLine(string message, params object[] parameters)
      {
         this.Dispatcher.Invoke(() =>
         {
            _Panel.Children.Add(CreateTextBlock(string.Format(message, parameters), color: Colors.DarkGreen, fontSize: 12));
         });
      }

      private TextBlock CreateTextBlock(string text, Color color, double fontSize=13)
      {
         var textBlock = new TextBlock();
         textBlock.Foreground = new SolidColorBrush(color);
         textBlock.Background = Brushes.White;
         textBlock.TextWrapping = System.Windows.TextWrapping.Wrap;
         textBlock.FontSize = fontSize;
         textBlock.Text = text;

         return textBlock;
      }
  }
}
