using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage;

namespace BarCodeUWP
{
   public class AppSettings : INotifyPropertyChanged
   {
      public ApplicationDataContainer LocalSettings { get; set; }

      public AppSettings()
      {
         LocalSettings = ApplicationData.Current.LocalSettings;
      }

      // https://edi.wang/post/2017/9/8/uwp-read-write-settings

      private void SaveSettings(string key, object value)
      {
         LocalSettings.Values[key] = value;
      }

      private T ReadSettings<T>(string key, T defaultValue)
      {
         if (LocalSettings.Values.ContainsKey(key))
         {
            return (T)LocalSettings.Values[key];
         }
         if (null != defaultValue)
         {
            return defaultValue;
         }
         return default(T);
      }

      public event PropertyChangedEventHandler PropertyChanged;

      protected void NotifyPropertyChanged([CallerMemberName] string propName = "")
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
      }
      
      public string CrossReferenceSpreadsheetSetting
      {
         get
         {
            return ReadSettings(nameof(CrossReferenceSpreadsheetSetting), "");
         }
         set
         {
            SaveSettings(nameof(CrossReferenceSpreadsheetSetting), value);
            NotifyPropertyChanged();
         }
      }
      public float ImageWidthInInchesSetting
      {
         get
         {
            return ReadSettings(nameof(ImageWidthInInchesSetting), (float)1.25);
         }
         set
         {
            SaveSettings(nameof(ImageWidthInInchesSetting), value);
            NotifyPropertyChanged();
         }
      }

      public float ImageHeightInInchesSetting
      {
         get
         {
            return ReadSettings(nameof(ImageHeightInInchesSetting), (float)0.94);
         }
         set
         {
            SaveSettings(nameof(ImageHeightInInchesSetting), value);
            NotifyPropertyChanged();
         }
      }

      public float InchesPerPixelSetting
      {
         get
         {
            return ReadSettings(nameof(InchesPerPixelSetting), (float)0.010417);
         }
         set
         {
            SaveSettings(nameof(InchesPerPixelSetting), value);
            NotifyPropertyChanged();
         }
      }

      public List<string> SupportedImageFileTypesSetting
      {
         get
         {
            return ReadSettings(nameof(SupportedImageFileTypesSetting), new List<string>() { ".jpg", ".tiff", ".exf", ".exp" });
         }
         set
         {
            SaveSettings(nameof(InchesPerPixelSetting), value);
            NotifyPropertyChanged();
         }
      }

      public bool ImageFileTypeIsSupported(string imageFileType)
      {
         return SupportedImageFileTypesSetting.Contains(imageFileType.ToLower());
      }
   }
}
