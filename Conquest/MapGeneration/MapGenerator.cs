using Conquest.MapClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Conquest.MapGeneration
{
    class MapGenerator
    {

        private const int STARTING_POINTS = 8;
        private const float SPLIT_CHANCE = 0.02f;

        private Random Random;
        private Map Map;
        private List<Action> ActionQueue;

        public MapGenerator(int width, int height, Map map, List<Action> actionQueue)
        {
            Random = new Random();
            this.Map = map;
            this.ActionQueue = actionQueue;
            WriteableBitmap img = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
            Map.SetMap(ConvertWriteableBitmapToBitmapImage(img));
        }

        public void GenerateMap()
        {
            for(int y= 0; y < Map.Height; y++)
            {
                for(int x = 0; x < Map.Width; x++)
                {
                    Map.SetPixel(x, y, Map.White);
                    Map.CountryMap[x, y] = MapPixelType.UNASSIGNED_COUNTRY;
                }
            }

            for(int i = 0; i < STARTING_POINTS; i++)
            {
                int x = Random.Next(Map.Width - 6) + 3;
                int y = Random.Next(Map.Height - 6) + 3;

                int dir = Random.Next(8);
                ActionQueue.Add(() => Spread(x, y, dir));
                switch(dir)
                {
                    case 0:
                        ActionQueue.Add(() => Spread(x, y + 1, (dir + 4) % 8));
                        break;

                    case 1:
                        ActionQueue.Add(() => Spread(x - 1, y + 1, (dir + 4) % 8));
                        break;

                    case 2:
                        ActionQueue.Add(() => Spread(x - 1, y, (dir + 4) % 8));
                        break;

                    case 3:
                        ActionQueue.Add(() => Spread(x - 1, y - 1, (dir + 4) % 8));
                        break;

                    case 4:
                        ActionQueue.Add(() => Spread(x, y - 1, (dir + 4) % 8));
                        break;

                    case 5:
                        ActionQueue.Add(() => Spread(x + 1, y - 1, (dir + 4) % 8));
                        break;

                    case 6:
                        ActionQueue.Add(() => Spread(x + 1, y, (dir + 4) % 8));
                        break;

                    case 7:
                        ActionQueue.Add(() => Spread(x + 1, y + 1, (dir + 4) % 8));
                        break;

                }
            }
        }

        private void Spread(int x, int y, int dir)
        {
            if (x < 0 || x >= Map.Width || y < 0 || y >= Map.Height) return;
            Map.SetPixel(x, y, Map.Black);
            Map.CountryMap[x, y] = MapPixelType.BORDER;
            int dirChange = Random.Next(100);
            int newDir;
            if (dirChange < 5) newDir = (dir - 2) % 8;
            else if (dirChange < 15) newDir = (dir - 1) % 8;
            else if (dirChange < 80) newDir = dir;
            else if (dirChange < 95) newDir = (dir + 1) % 8;
            else newDir = (dir + 2) % 8;

            switch(newDir)
            {
                case 0:
                    ActionQueue.Add(() => Spread(x, y - 1, newDir));
                    break;

                case 1:
                    ActionQueue.Add(() => Spread(x + 1, y - 1, newDir));
                    break;

                case 2:
                    ActionQueue.Add(() => Spread(x + 1, y, newDir));
                    break;

                case 3:
                    ActionQueue.Add(() => Spread(x + 1, y + 1, newDir));
                    break;

                case 4:
                    ActionQueue.Add(() => Spread(x, y + 1, newDir));
                    break;

                case 5:
                    ActionQueue.Add(() => Spread(x - 1, y + 1, newDir));
                    break;

                case 6:
                    ActionQueue.Add(() => Spread(x - 1, y, newDir));
                    break;

                case 7:
                    ActionQueue.Add(() => Spread(x - 1, y - 1, newDir));
                    break;
            }
        }

        public BitmapImage ConvertWriteableBitmapToBitmapImage(WriteableBitmap wbm)
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
