using Microsoft.Office.Interop.Excel;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;

namespace BarCode
{
   public enum SpreadsheetFormatResult
   {
      Good,
      TooManyWorksheets,
      UPCColumnMissing,
      VendorColumnMissing,
      DescriptionColumnMissing,
      RegisDescriptionColumnMissing
   }


   public class Product
   {
      public string Vendor { get; private set; }
      public string Description { get; private set; }

      public string RegisDescription { get; private set; }

      public string UPC { get; private set; }

      public Product(string vendor, string description, string regisDescription, string upc)
      {
         Vendor = vendor;
         Description = description;
         RegisDescription = regisDescription;
         UPC = upc;
      }
   }

   public class CrossReferenceSpreadsheet
   {
      private string _FullPath;
      private AppSettings _AppSettings;
      private Excel.Application _App;
      private Workbook _Workbook;
      private Worksheet _Worksheet;
      private int? _UPCColumn;

      public CrossReferenceSpreadsheet(AppSettings appSettings)
      {
         _FullPath = appSettings.CrossReferenceSpreadsheet;
         _AppSettings = appSettings;

         _App = new Excel.Application();
         _App.DisplayAlerts = false;
         _App.Visible = false;

         _Workbook = _App.Workbooks.Open(Filename: _FullPath, ReadOnly: true);

         if (_Workbook.Worksheets.Count > 1)
         {
            // too many worksheets
            MessageBox.Show($"There are {_Workbook.Worksheets.Count} worksheets. Can't continue. There should be only 1.", "Too many worksheets", MessageBoxButton.OK, MessageBoxImage.Error);
         }
         else
         {
            _Worksheet = (Excel.Worksheet)_Workbook.Worksheets[1];

            GetAllColumnNames();
            
            var correctFormat = IsInCorrectFormat;
            _UPCColumn = FindColumn(UPCColumnName);

            if (_UPCColumn is null)
            {
               return;
            }

            if (FindColumn(VendorColumnName) is null)
            {
               return;
            }

            if (FindColumn(DescriptionColumnName) is null)
            {
               return;
            }

            if (FindColumn(RegisDescriptionColumnName) is null)
            {
               return;
            }

         }
      }


      private string UPCColumnName => _AppSettings.UPCColumnName;
      private string VendorColumnName => _AppSettings.VendorColumnName;
      private string DescriptionColumnName => _AppSettings.DescriptionColumnName;
      private string RegisDescriptionColumnName => _AppSettings.RegisDescriptionColumnName;

      public ColumnHeadings ColumnHeadings = new ColumnHeadings();

      private void GetAllColumnNames()
      {
         ColumnHeadings.Clear();

         int columnCount = _Worksheet.UsedRange.Columns.Count;

         for (int c = 1; c <= columnCount; c++)
         {
            var cell = (Excel.Range)_Worksheet.Cells[1, c];

            if (cell != null)
            {
               if (!string.IsNullOrEmpty((string)cell.Value2))
               {
                  ColumnHeadings.Add((string)cell.Value2, c);
               }
            }
         }
      }

      public SpreadsheetFormatResult IsInCorrectFormat
      {
         get
         {
            if (_Workbook.Worksheets.Count > 1)
            {
               return SpreadsheetFormatResult.TooManyWorksheets;
            }

            GetAllColumnNames();

            _UPCColumn = FindColumn(UPCColumnName);

            if (_UPCColumn is null)
            {
               return SpreadsheetFormatResult.UPCColumnMissing;
            }

            if (FindColumn(VendorColumnName) is null)
            {
               return SpreadsheetFormatResult.VendorColumnMissing;
            }

            if (FindColumn(DescriptionColumnName) is null)
            {
               return SpreadsheetFormatResult.DescriptionColumnMissing;
            }

            if (FindColumn(RegisDescriptionColumnName) is null)
            {
               return SpreadsheetFormatResult.RegisDescriptionColumnMissing;
            }

            return SpreadsheetFormatResult.Good;
         }
      }

      private int? FindColumn(string columnHeading)
      {
         string columnHeadingUpperCase = columnHeading.ToUpper();

         var heading = ColumnHeadings.Where(c => c.HeadingUpperCase == columnHeadingUpperCase).FirstOrDefault();

         if (heading is null)
         {
            //MessageBox.Show($"Did not find '{columnHeading}' in row 1\nExisting Headers = {ColumnHeadings.ToString()}");
            return null;

         }
         return heading.ColumnNumber;
      }

      public Product FindProduct(string fullPath, string UPC)
      {
         if (IsInCorrectFormat != SpreadsheetFormatResult.Good)
         {
            return null;
         }

         if (string.IsNullOrEmpty(UPC))
         {
            return null;
         }

         if (File.Exists(fullPath))
         {
            Product product = null;

            // find Vender
            var UPCRowNumber = FindUPCRowNumber(UPC);

            if (UPCRowNumber.HasValue)
            {
               var vendor = FindCell(UPCRowNumber.Value, VendorColumnName);
               var description = FindCell(UPCRowNumber.Value, DescriptionColumnName);
               var regisDescription = FindCell(UPCRowNumber.Value, RegisDescriptionColumnName);

               if (!string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(regisDescription))
               {
                  product = new Product(vendor, description, regisDescription, UPC);
               }
               else
               {
                  MessageBox.Show($"Can't get product information for {UPC}", "Missing product information", MessageBoxButton.OK, MessageBoxImage.Error);
                  return null;
               }
            }

            return product;
         }
         return null;
      }

      private int? FindUPCRowNumber(string UPC)
      {
         Excel.Range colRange = (Excel.Range)_Worksheet.Columns[_UPCColumn];

         Excel.Range resultRange = colRange.Find(
                      What: UPC,
                      LookIn: Excel.XlFindLookIn.xlValues,
                      LookAt: Excel.XlLookAt.xlPart,
                      SearchOrder: Excel.XlSearchOrder.xlByRows,
                      SearchDirection: Excel.XlSearchDirection.xlNext

                      );// search searchString in the range, if find result, return a range

         if (resultRange is null)
         {
            MessageBox.Show($"Did not find {UPC} UPC code in '{_AppSettings.UPCColumnName}' column");
            return null;
         }
         else
         {
            //then you could handle how to display the row to the label according to resultRange
            return resultRange.Row;
         }

      }

      private string FindCell(int UPCRowNumber, string columnHeading)
      {
         var cellColumn = FindColumn(columnHeading);

         if (cellColumn is null)
         {
            return null;

         }
         Excel.Range colRange = (Excel.Range)_Worksheet.Cells[UPCRowNumber, cellColumn];

         if (colRange is null)
         {
            return null;
         }
         else
         {
            return (string)colRange.Value;
         }
      }

      public void Close()
      {
         _Workbook.Close(false, null, null);
         _App.Quit();

         Marshal.ReleaseComObject(_App);
         _App = null;
      }
   }

   public class ColumnHeadings : List<ColumnHeading>
   {
      public void Add(string columnHeading, int columnNumber)
      {
         var cellValue = RemoveExtraSpacesBetweenWords(ConvertNewLineToSpace(columnHeading));

         this.Add(new ColumnHeading() { Heading = cellValue, HeadingUpperCase = cellValue.ToUpper(), ColumnNumber = columnNumber });
      }

      private string ConvertNewLineToSpace(string fullString)
      {
         var result = Regex.Replace(fullString, "\n", " ");

         if (result != null)
         {
            return result;
         }
         return null;
      }

      private string RemoveExtraSpacesBetweenWords(string fullString)
      {
         var result = Regex.Replace(fullString, @"\s+", " ");

         if (result != null)
         {
            return result;
         }
         return null;
      }

      public override string ToString()
      {
         return string.Join(", ", this.Select(columnHeading => $"'{columnHeading.Heading}'"));
      }
   }

   public class ColumnHeading
   {
      public string Heading;
      public string HeadingUpperCase;
      public int ColumnNumber;


      public string DebugString()
      {
         return $"Heading ='{Heading}', HeadingUpperCase='{HeadingUpperCase}', ColumnNumber={ColumnNumber}";
         
      }
   }
}
