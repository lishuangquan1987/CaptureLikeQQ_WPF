using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CaptureLikeQQ_WPF
{
   public class ImageHelper
    {
        public static Bitmap GetImage(Point upperleft, Size size)
        {
            Bitmap bmp = new Bitmap(size.Width, size.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(upperleft.X, upperleft.Y, 0, 0, size);
            return bmp;
        }
        public static System.Windows.Media.ImageSource GetImage(string path)
        {
            var img = new BitmapImage();
            img.BeginInit();
            img.StreamSource = new MemoryStream(File.ReadAllBytes(path));
            img.EndInit();

            return img;
        }
        public static System.Windows.Media.ImageSource GetImage(byte[] data)
        {
            var img = new BitmapImage();
            img.BeginInit();
            img.StreamSource = new MemoryStream(data);
            img.EndInit();
            return img;
        }
        public static System.Windows.Media.ImageSource GetImage(Stream stream)
        {
            var img = new BitmapImage();
            img.BeginInit();
            img.StreamSource = stream;
            img.EndInit();
            return img;
        }
    }
}
