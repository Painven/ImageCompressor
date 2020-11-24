using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageCompressorLib
{
    public class ImageMultiCompressor
    {
        public event Action<string> OnError;

        #region Compress
        public void CompressImage(string imageFilePath, long qualityLevel)
        {
           
            string tempImage = Path.GetTempFileName();

            using (var image = (Bitmap)Bitmap.FromFile(imageFilePath))
            using (Graphics imageGraphics = Graphics.FromImage(image))
            {
                ImageCodecInfo formatEncoder = GetEncoder(Path.GetExtension(imageFilePath));
                Encoder myEncoder = Encoder.Quality;
                EncoderParameters myEncoderParameters = new EncoderParameters(1);
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, qualityLevel);
                myEncoderParameters.Param[0] = myEncoderParameter;

                image.Save(tempImage, formatEncoder, myEncoderParameters);
            }

            File.Delete(imageFilePath);
            File.Move(tempImage, imageFilePath);

        }

        public async Task CompressImages(IEnumerable<string> images, long qualityLevel, IProgress<Tuple<int, int>> indicator)
        {
            int total = images.Count();
            int current = 0;

            foreach (var image in images)
            {
                if (!image.ToLower().EndsWith(".png"))
                {
                    try
                    {
                        await Task.Run(() => CompressImage(image, qualityLevel)).ConfigureAwait(false);
                    }
                    catch(Exception ex)
                    {
                        OnError?.Invoke(ex.Message + $" ({image})");
                    }
                }
                indicator?.Report(new Tuple<int, int>(++current, total));
            }
        }
        #endregion

        #region SaveAsJpg
        public async Task SaveAllAsJpg(List<string> images, long qualityLevel, IProgress<Tuple<int, int>> indicator)
        {
            images = images.Where(img => Path.GetExtension(img).ToLower() != ".jpg").ToList();
            int total = images.Count;
            int current = 0;

            foreach (var file in images)
            {
                try
                {
                    await Task.Run(() => SaveFileAsJpg(file, (int)qualityLevel)).ConfigureAwait(false) ;
                }
                catch(Exception ex)
                {
                    OnError?.Invoke(ex.Message + $" ({file})");
                }
                indicator?.Report(new Tuple<int, int>(++current, total));
            }
        }

        private void SaveFileAsJpg(string webpFile, int qualityLevel)
        {
            var format = new JpegFormat { Quality = 100 };
            string newPath = Path.Combine(Path.GetDirectoryName(webpFile), Path.GetFileNameWithoutExtension(webpFile)) + ".jpg";

            using (var fs = new FileStream(webpFile, FileMode.Open, FileAccess.Read))
            {
                using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                {
                    // Load, resize, set the format and quality and save an image.
                    using (var newFile = new FileStream(newPath, FileMode.Create))
                    {
                        imageFactory.Load(fs).Format(format).Save(newFile);
                    }
                }
            }
        }
        #endregion

        #region Resize
        public async Task ResizeImages(IEnumerable<string> images, Size newSize, IProgress<Tuple<int, int>> indicator)
        {
            int total = images.Count();
            int current = 0;

            foreach (var image in images)
            {
                try
                {
                    await Task.Run(() => ResizeImage(image, newSize)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex.Message + $" ({image})");
                }
                indicator?.Report(new Tuple<int, int>(++current, total));
            }
        }

        private void ResizeImage(string image, Size newSize)
        {
            ResizeLayer resizeLayer = new ResizeLayer(newSize, ResizeMode.Pad);

            var tempFile = Path.GetTempFileName();

            using (var fs = new FileStream(image, FileMode.Open, FileAccess.Read))
            {
                using (ImageFactory imageFactory = new ImageFactory(false))
                {
                    // Load, resize, set the format and quality and save an image.
                    using (var newFile = new FileStream(tempFile, FileMode.Create))
                    {
                            imageFactory
                            .Load(fs)
                            .Resize(resizeLayer)
                            .BackgroundColor(Color.White)
                            .Save(newFile);
                    }
                }
            }

            File.Delete(image);
            File.Move(tempFile, image);
        }
        #endregion
               
        private ImageCodecInfo GetEncoder(string fileExt)
        {
            ImageFormat format = null;
            switch (fileExt.ToLower())
            {
                case ".jpeg":
                case ".jpg": format = ImageFormat.Jpeg; break;
                case ".png": format = ImageFormat.Png; break;
                default: throw new NotSupportedException();
            }

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
