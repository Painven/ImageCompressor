using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageCompressorLib
{
    public class ImageWatermarkEraser
    {

        public async Task EraseWatermarks(IEnumerable<string> images, SizeF watermarkSize, IProgress<ProgressStatus> indicator)
        {
            int total = images.Count();
            int current = 0;

            foreach (string image in images)
            {
                await Task.Run(() => EraseWatermark(image, watermarkSize));
                indicator?.Report(new ProgressStatus(++current, total));
            }
        }

        public void EraseWatermark(string fileName, SizeF watermarkSize)
        {
            var tempFile = Path.GetTempFileName();

            using (Image image = Image.FromFile(fileName))
            {
                using (Graphics g = Graphics.FromImage(image))
                {                   
                    SolidBrush whiteBrush = new SolidBrush(Color.White);

                    SizeF imageSize = image.Size;
                    PointF location = new PointF(imageSize.Width - watermarkSize.Width, imageSize.Height - watermarkSize.Height);

                    var rect = new RectangleF(location, watermarkSize);

                    g.FillRectangle(whiteBrush, rect);
                }

                image.Save(tempFile);
            }

            File.Delete(fileName);
            File.Move(tempFile, fileName);
            File.Delete(tempFile);
        }

    }
}
