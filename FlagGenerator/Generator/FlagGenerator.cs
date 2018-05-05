using FlagGeneration.FlagClasses;
using FlagGeneration.FlagPatterns;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FlagGeneration.Generator
{
    class FlagGenerator
    {

        private Random Random;
        private List<Action> ActionQueue;

        public Flag Generate(List<Action> actionQueue)
        {
            Flag flag = new Flag();
            Random = new Random();
            this.ActionQueue = actionQueue;
            WriteableBitmap img = new WriteableBitmap(900, 600, 96, 96, PixelFormats.Bgr32, null);
            flag.SetFlag(ConvertWriteableBitmapToBitmapImage(img));
            MainPattern pattern = MainPattern.RandomMainPattern();
            pattern.Apply(flag.writeableBitmap);
            flag.writeableBitmap.DrawRectangle(0, 0, (int)flag.writeableBitmap.Width, (int)(flag.writeableBitmap.Height), Color.FromArgb(255, 0, 0, 0));
            return flag;
        }

        public static BitmapImage ConvertWriteableBitmapToBitmapImage(WriteableBitmap wbm)
        {
            BitmapImage bmImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbm));
                encoder.Save(stream);
                bmImage.BeginInit();
                bmImage.CacheOption = BitmapCacheOption.OnLoad;
                bmImage.StreamSource = stream;
                bmImage.EndInit();
                bmImage.Freeze();
            }
            return bmImage;
        }
    }
}
