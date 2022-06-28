using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace BarCodeUWP
{
   public class NewImageFile : ImageFileBase
   {
      private ImageFile _ExistingImageFile;
      public SoftwareBitmap Image { get; protected set; }

      public NewImageFile(string fullPath, AppSettings settings, ImageFile existingImageFile, string widthInInches, string heightInInches)
         : base(fullPath)
      {
         _ExistingImageFile = existingImageFile;

         ImageSize = new ImageSize(settings.InchesPerPixelSetting, widthInInches, heightInInches);
         //ResizeImage(_ExistingImageFile.StorageFile, ImageSize.WidthInPixels, ImageSize.HeightInPixels);
      }

      // https://social.msdn.microsoft.com/Forums/en-US/3e21356d-133f-4609-8a39-a07f272473b5/uwphow-to-compress-image-and-change-its-size?forum=wpdevelop
      public async Task<bool> ResizeImage()
      {
         //open file as stream
         using (IRandomAccessStream fileStream = await _ExistingImageFile.StorageFile.OpenAsync(FileAccessMode.Read))
         {
            var reqWidth = (double)ImageSize.WidthInPixels;
            var reqHeight = (double)ImageSize.HeightInPixels;

            var decoder = await BitmapDecoder.CreateAsync(fileStream);

            var resizedStream = new InMemoryRandomAccessStream();

            BitmapEncoder encoder = await BitmapEncoder.CreateForTranscodingAsync(resizedStream, decoder);
            double widthRatio = reqWidth / decoder.PixelWidth;
            double heightRatio = reqHeight / decoder.PixelHeight;

            double scaleRatio = Math.Min(widthRatio, heightRatio);

            if (reqWidth == 0)
               scaleRatio = heightRatio;

            if (reqHeight == 0)
               scaleRatio = widthRatio;

            uint aspectHeight = (uint)Math.Floor(decoder.PixelHeight * scaleRatio);
            uint aspectWidth = (uint)Math.Floor(decoder.PixelWidth * scaleRatio);

            encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Linear;

            encoder.BitmapTransform.ScaledHeight = aspectHeight;
            encoder.BitmapTransform.ScaledWidth = aspectWidth;

            await encoder.FlushAsync();
            resizedStream.Seek(0);
            var outBuffer = new byte[resizedStream.Size];
            await resizedStream.ReadAsync(outBuffer.AsBuffer(), (uint)resizedStream.Size, InputStreamOptions.None);

            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(DirectoryName);

            StorageFile newFile = await storageFolder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(newFile, outBuffer);

            return true;
         }
      }
      //// https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp
      //private Bitmap ResizeImage(Image existingImage, int width, int height)
      //{
      //   var destRect = new Rectangle(0, 0, width, height);
      //   var destImage = new Bitmap(width, height);

      //   destImage.SetResolution(existingImage.HorizontalResolution, existingImage.VerticalResolution);

      //   using (var graphics = Graphics.FromImage(destImage))
      //   {
      //      graphics.CompositingMode = CompositingMode.SourceCopy;
      //      graphics.CompositingQuality = CompositingQuality.HighQuality;
      //      graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
      //      graphics.SmoothingMode = SmoothingMode.HighQuality;
      //      graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

      //      using (var wrapMode = new ImageAttributes())
      //      {
      //         wrapMode.SetWrapMode(WrapMode.TileFlipXY);
      //         graphics.DrawImage(existingImage, destRect, 0, 0, existingImage.Width, existingImage.Height, GraphicsUnit.Pixel, wrapMode);
      //      }
      //   }

      //   return destImage;
      //}

      //public bool SaveImage()
      //{
      //   try
      //   {
      //      //StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
      //      //Debug.WriteLine(storageFolder.Path);

      //      //Image.Save(FullPath);
      //      return true;
      //   }
      //   catch (System.Exception)
      //   {
      //      return false;
      //   }


      //}

   }
}
