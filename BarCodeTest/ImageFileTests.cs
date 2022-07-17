using BarCode;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using Windows.Media.Ocr;

namespace BarCodeTest
{
   [TestClass]
   public class ImageFileTests
   {

      private ImageFile _ImageFile;

      private TestContext testContextInstance;
      public TestContext TestContext
      {
         get { return testContextInstance; }
         set { testContextInstance = value; }
      }

      [TestInitialize]
      public void TestInitialize()
      {
         _ImageFile = new ImageFile();
      }

      [DataTestMethod]
      [DataRow(null, false, null)] // null
      [DataRow("", false, "")] // empty
      [DataRow("||||9531112213", true, "9531112213")] // bar code with non-numbers at beginning
      [DataRow("09531112213", true, "9531112213")] // bar code with leading zeros
      [DataRow("8 84486151315", true, "884486151315")] // with spaces 
      [DataRow("some bar code", false, "some bar code")] // bar code with no numbers
      [DataRow("1232", false, "1232")] // not long enough
      public void ProcessBarCode(string rawText, bool expectedSuccess, string expectedBarCode)
      {
         //(bool success, string barCodeResult) = _ImageFile.ProcessBarCode("", rawText);

         //Assert.AreEqual<bool>(expectedSuccess, success);
         //Assert.AreEqual<string>(expectedBarCode, barCodeResult);
      }

      //[DataTestMethod]
      //[DataRow("1232", "1234", "")] // not long enough
      //[DataRow("|||||", "9531112213", "9531112213")] 
      //[DataRow("8 8448615131", "5", "884486151315")] // with spaces and one 2 lines
      //public void ProcessOcrLines(string line1, string line2, string expectedString)
      //{
      //   var list = new List<string>();
      //   list.Add(line1);
      //   list.Add(line2);

      //   Assert.AreEqual<string>(expectedString, _ImageFile.ProcessOcrLines(list));


      //}
   }
}
