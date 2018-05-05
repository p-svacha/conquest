using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FlagGeneration.FlagClasses
{
    class Flag
    {
        private byte[] Pixels;
        private int Stride;
        private BitmapImage OriginalImage;
        public WriteableBitmap writeableBitmap;
        public Image Image;

        public Flag()
        {
            Image = new Image();
            RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(Image, EdgeMode.Unspecified);
            Image.Stretch = Stretch.None;
            Image.HorizontalAlignment = HorizontalAlignment.Center;
            Image.VerticalAlignment = VerticalAlignment.Center;
        }

        public void SetFlag(BitmapImage bitmapImage)
        {
            OriginalImage = bitmapImage;
            Stride = bitmapImage.PixelWidth * 4;
            int size = bitmapImage.PixelHeight * Stride;
            Pixels = new byte[size];
            bitmapImage.CopyPixels(Pixels, Stride, 0);
            writeableBitmap = new WriteableBitmap((int)(bitmapImage.PixelWidth), (int)(bitmapImage.PixelHeight), 96, 96, PixelFormats.Bgr32, null);
            Image.Source = writeableBitmap;
        }

        public int Width
        {
            get
            {
                return writeableBitmap.PixelWidth;
            }
        }
        public int Height
        {
            get
            {
                return writeableBitmap.PixelHeight;
            }
        }
    }
}
