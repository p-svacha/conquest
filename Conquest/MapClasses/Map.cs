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
            writeableBitmap = new WriteableBitmap((int)(bitmapImage.PixelWidth), (int)(bitmapImage.PixelHeight), 96, 96, PixelFormats.Bgr32, null);
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

        public void RefreshMap()
        {
            writeableBitmap.Lock();
            unsafe
            {
                for (int y = 0; y < writeableBitmap.PixelHeight; y++)
                {
                    for (int x = 0; x < writeableBitmap.PixelWidth; x++)
                    {
                        Color c = GetPixel(x, y);
                        int pBackBuffer = (int)writeableBitmap.BackBuffer;
                        pBackBuffer += y * writeableBitmap.BackBufferStride;
                        pBackBuffer += x * 4;
                        int color_data = c.R << 0; // R
                        color_data |= c.G << 8;   // G
                        color_data |= c.B << 16;   // B
                        *((int*)pBackBuffer) = color_data;
                        writeableBitmap.AddDirtyRect(new Int32Rect(x, y, 1, 1));
                    }
                }
            }
            writeableBitmap.Unlock();
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
                int color_data = c.R << 16; // R
                color_data |= c.G << 8;   // G
                color_data |= c.B << 0;   // B
                *((int*)pBackBuffer) = color_data;
            }
            writeableBitmap.AddDirtyRect(new Int32Rect(x, y, 1, 1));
            writeableBitmap.Unlock();
            writeableBitmap.CopyPixels(Pixels, Stride, 0);
        }

        public void FillCountry(Country country, Color color)
        {
            writeableBitmap.Lock();
            unsafe
            {
                foreach (Point p in country.Pixels)
                {
                    int pBackBuffer = (int)writeableBitmap.BackBuffer;
                    pBackBuffer += (int)p.Y * writeableBitmap.BackBufferStride;
                    pBackBuffer += (int)p.X * 4;
                    int color_data = color.R << 0; // R
                    color_data |= color.G << 8;   // G
                    color_data |= color.B << 16;   // B
                    *((int*)pBackBuffer) = color_data;
                    writeableBitmap.AddDirtyRect(new Int32Rect((int)p.X, (int)p.Y, 1, 1));
                }
            }
            writeableBitmap.Unlock();
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
