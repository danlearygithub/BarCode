﻿using BarCode.Properties;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BarCode
{
   public class AppSettings : INotifyPropertyChanged
   {
      public AppSettings()
      {
         LocalSettings = Properties.Settings.Default;
      }

      internal Settings LocalSettings { get; private set; }

      // https://edi.wang/post/2017/9/8/uwp-read-write-settings

      private void SaveSettings(string key, object value)
      {
         LocalSettings[key] = value;
         LocalSettings.Save();
      }

      private T ReadSettings<T>(string key, T defaultValue)
      {
         if (LocalSettings[key] != null)
         {
            return (T)LocalSettings[key];
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

      public string CrossReferenceSpreadsheet
      {
         get
         {
            return ReadSettings("CrossReferenceSpreadsheet", "");
         }
         set
         {
            SaveSettings("CrossReferenceSpreadsheet", value);
            NotifyPropertyChanged();
         }
      }

      public string CrossReferenceUPCColumnName
      {
         get
         {
            return ReadSettings("CrossReferenceUPCColumnName", "");
         }
         set
         {
            SaveSettings("CrossReferenceUPCColumnName", value);
            NotifyPropertyChanged();
         }
      }

      public float ImageWidthInInches
      {
         get
         {
            return ReadSettings("ImageWidthInInches", (float)1.25);
         }
         set
         {
            SaveSettings("ImageWidthInInches", value);
            NotifyPropertyChanged();
         }
      }

      public float ImageHeightInInches
      {
         get
         {
            return ReadSettings("ImageHeightInInches", (float)0.94);
         }
         set
         {
            SaveSettings("ImageHeightInInches", value);
            NotifyPropertyChanged();
         }
      }

      public float InchesPerPixel
      {
         get
         {
            return ReadSettings("InchesPerPixel", (float)0.010417);
         }
         set
         {
            SaveSettings("InchesPerPixel", value);
            NotifyPropertyChanged();
         }
      }

      public List<string> SupportedImageFileTypes
      {
         get
         {
            return ReadSettings("SupportedImageFileTypes", new List<string>() { ".jpg", ".tiff", ".exf", ".exp" });
         }
         set
         {
            SaveSettings("SupportedImageFileTypes", value);
            NotifyPropertyChanged();
         }
      }




      public bool ImageFileTypeIsSupported(string imageFileType)
      {
         return SupportedImageFileTypes.Contains(imageFileType.ToLower());
      }
   }
}