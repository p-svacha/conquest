using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Conquest.MapClasses
{
    class Map
    {
        private byte[] Pixels;
        private int Stride;
        private WriteableBitmap writeableBitmap;
        private Image image;

        public void SetMap(BitmapImage bitmapImage)
        {
            Stride = bitmapImage.PixelWidth * 4;
            int size = bitmapImage.PixelHeight * Stride;
            Pixels = new byte[size];
            bitmapImage.CopyPixels(Pixels, Stride, 0);
            writeableBitmap = new WriteableBitmap(bitmapImage);
            image = new Image();
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(image, EdgeMode.Unspecified);
            image.Source = writeableBitmap;
            image.Stretch = Stretch.None;
            image.HorizontalAlignment = HorizontalAlignment.Center;
            image.VerticalAlignment = VerticalAlignment.Center;
        }

        public Image GetMapImage()
        {
            return image;
        }

        public Color GetPixel(int x, int y)
        {
            
            int index = y * Stride + 4 * x;
            byte red = Pixels[index];
            byte green = Pixels[index + 1];
            byte blue = Pixels[index + 2];
            byte alpha = Pixels[index + 3];
            return Color.FromRgb(red, green, blue);
        }

        public void SetPixel(int x, int y, Color c)
        {
            writeableBitmap.Lock();
            unsafe
            {
                int pBackBuffer = (int)writeableBitmap.BackBuffer;
                pBackBuffer += y * writeableBitmap.BackBufferStride;
                pBackBuffer += x * 4;
                int color_data = c.R << 0; // R
                color_data |= c.G << 8;   // G
                color_data |= c.B << 16;   // B
                *((int*)pBackBuffer) = color_data;
            }
            writeableBitmap.AddDirtyRect(new Int32Rect(x, y, 1, 1));
            writeableBitmap.Unlock();
        }
    }
}
