using Microsoft.Office.Interop.Excel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;

namespace BarCode
{
   public class Product
   {
      public string Vendor { get; private set; }
      public string Description { get; private set; }
      public string UPC { get; private set; }

      public Product(string vendor, string description, string upc)
      {
         Vendor = vendor;
         Description = description;
         UPC = upc;
      }
   }

   public class CrossReferenceSpreadsheet
   {
      private string _FullPath;

      private Excel.Application _App;
      private Workbook _Workbook;
      private Worksheet _Worksheet;
      public bool ValidSpreadsheet { get; private set; }

      private int? _UPCColumn;
      private int? _VendorColumn;
      private int? _DescriptionColumn;

      public CrossReferenceSpreadsheet(string fullPath)
      {
         var excelApp = new Excel.Application();
         excelApp.DisplayAlerts = false;
         excelApp.Visible = false;

         _FullPath = fullPath;

         _App = new Excel.Application();
         _App.DisplayAlerts = false;
         _App.Visible = false;

         _Workbook = _App.Workbooks.Open(_FullPath);

         if (_Workbook.Worksheets.Count > 1)
         {
            ValidSpreadsheet = false;
            // too many worksheets
            MessageBox.Show($"There are {_Workbook.Worksheets.Count} worksheets. Can't continue. There should be only 1.", "Too many worksheets", MessageBoxButton.OK, MessageBoxImage.Error);
         }
         else
         {
            _Worksheet = (Excel.Worksheet)_Workbook.Worksheets[1];

            _UPCColumn = FindColumn(UPCColumnName);

            if (_UPCColumn == null)
            {
               return;
            }

            _VendorColumn = FindColumn(VendorColumnName);

            if (_VendorColumn == null)
            {
               return;
            }

            _DescriptionColumn = FindColumn(DescriptionColumnName);

            if (_DescriptionColumn == null)
            {
               return;
            }

            ValidSpreadsheet = true;
         }
      }

      private const string UPCColumnName = "SalonCentric UPC";
      private const string VendorColumnName = "Vendor";
      private const string DescriptionColumnName = "Description";

      private int? FindColumn(string columnHeader)
      {
         Excel.Range colRange = (Excel.Range)_Worksheet.Rows["1:1"];

         Excel.Range resultRange = colRange.Find(
                         What: columnHeader,
                         LookIn: Excel.XlFindLookIn.xlValues,
                         LookAt: Excel.XlLookAt.xlPart,
                         SearchOrder: Excel.XlSearchOrder.xlByColumns,
                         SearchDirection: Excel.XlSearchDirection.xlNext

                         );// search searchString in the range, if find result, return a range

         if (resultRange is null)
         {
            MessageBox.Show("Did not found " + UPCColumnName + " in row 1");
            return null;
         }
         else
         {
            return resultRange.Column;
         }
      }


      public Product FindProduct(string fullPath, string UPC)
      {
         if (!ValidSpreadsheet)
         {
            return null;
         }

         if (File.Exists(fullPath))
         {
            // need to remove any spaces 
            var UPCCode = Regex.Replace(UPC, @"\s+", "");

            Product product = null;

            // find Vender
            var UPCRowNumber = FindUPCRowNumber(UPCCode);

            if (UPCRowNumber.HasValue)
            {
               var vendor = FindVendor(UPCRowNumber.Value);
               var description = FindDescription(UPCRowNumber.Value);

               if (!string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(description))
               {
                  product = new Product(vendor, description, UPCCode);
               }
               else
               {
                  // should not do this
                  throw new InvalidDataException();
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
            MessageBox.Show("Did not found " + UPC + " in column G");
            return null;
         }
         else
         {
            //then you could handle how to display the row to the label according to resultRange
            return resultRange.Row;
         }

      }

      private string FindVendor(int UPCRowNumber)
      {
         Excel.Range colRange = (Excel.Range)_Worksheet.Cells[UPCRowNumber, _VendorColumn];

         if (colRange is null)
         {
            return null;
         }
         else
         {
            return (string)colRange.Value;
         }
      }
      private string FindDescription(int UPCRowNumber)
      {
         Excel.Range colRange = (Excel.Range)_Worksheet.Cells[UPCRowNumber, _DescriptionColumn];

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
}
