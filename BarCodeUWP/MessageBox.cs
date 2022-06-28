using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace BarCodeUWP
{
   // // https://docs.microsoft.com/en-us/uwp/api/windows.ui.popups.messagedialog.content?view=winrt-22621

   public static class MessageBox
   {
      //private MessageDialog _MessageDialog;

      public static async Task<string> Show(string content, string title, IList<string> buttonLabels)
      {
         var messageDialog = new MessageDialog(content, title);
         messageDialog.Options = MessageDialogOptions.None;

         foreach (var buttonLabel in buttonLabels)
         {
            messageDialog.Commands.Add(new UICommand(buttonLabel, null));
         }

         // Set the command that will be invoked by default
         messageDialog.DefaultCommandIndex = 0;

         // Set the command to be invoked when escape is pressed
         messageDialog.CancelCommandIndex = 0;

         IUICommand command = await messageDialog.ShowAsync();

         return command.Label;
      }

      //public async Task<string> Show()
      //{
      //   var cmd = await _MessageDialog.ShowAsync();

      //   return cmd.Label;
      //}
   }
}
