namespace BarCodeUWP
{
   internal class Settings
   {
      private Setting<string> _CrossReferenceSpreadsheetSetting;
      private Setting<float> _ImageWidthInInchesSetting;
      private Setting<float> _ImageHeightInInchesSetting;
      private Setting<float> _InchesPerPixelSetting;

      public Settings()
      {
         _CrossReferenceSpreadsheetSetting = new Setting<string>("CrossReferenceSpreadsheet", "");
         _ImageWidthInInchesSetting = new Setting<float>("ImageWidthInInches", (float)1.25);
         _ImageHeightInInchesSetting = new Setting<float>("ImageHeightInInches", (float)0.94);
         _InchesPerPixelSetting = new Setting<float>("InchesPerPixel", (float)0.010417);
      }

      public string CrossReferenceSpreadsheet
      {
         get { return _CrossReferenceSpreadsheetSetting.Value; }
         set { _CrossReferenceSpreadsheetSetting.Value = value; }
      }

      public float ImageWidthInInches
      {
         get { return _ImageWidthInInchesSetting.Value; }
         set { _ImageWidthInInchesSetting.Value = value; }
      }

      public float ImageHeightInInches
      {
         get { return _ImageHeightInInchesSetting.Value; }
         set { _ImageHeightInInchesSetting.Value = value; }
      }

      public float InchesPerPixel
      {
         get { return _InchesPerPixelSetting.Value; }
         set { _InchesPerPixelSetting.Value = value; }
      }
   }

   internal class Setting<T>
   {
      private static Windows.Storage.ApplicationDataContainer _LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

      private string _SettingName;

      public Setting(string settingName, T defaultValue)
      {
         _SettingName = settingName;

         if (!SettingExists(_SettingName))
         {
            Value = defaultValue;
         }
      }

      public T Value
      {
         get
         {
            if (SettingExists(_SettingName))
            {
               return (T)_LocalSettings.Values[_SettingName];
            }
            return default(T);
         }
         set { _LocalSettings.Values[_SettingName] = value; }
      }

      private static bool SettingExists(string propertyName)
      {
         var setting = _LocalSettings.Values[propertyName];

         if (setting is null)
         {
            return false;
         }
         return true;
      }

   }

}
