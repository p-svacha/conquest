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

        private const int SNAP_DISTANCE = 2; //px
        private float LINE_DENSITY = 80;  // active lines at the beginning per 1 000 000 pixels
        private int activeLines;
        private float SPLIT_CHANCE = 35; // per 1000 ticks
        private const int MAX_TURN_ANGLE = 40; //°
        private const int MIN_SEGMENT_LENGTH = 3; //px
        private const int MAX_SEGMENT_LENGTH = 4; //px

        private Random Random;
        private Map Map;
        private List<Action> ActionQueue;

        public MapGenerator(int width, int height, Map map, List<Action> actionQueue, float countryAmountScale)
        {
            SPLIT_CHANCE *= countryAmountScale;
            LINE_DENSITY *= countryAmountScale;
            Random = new Random();
            this.Map = map;
            this.ActionQueue = actionQueue;
            WriteableBitmap img = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
            Map.SetMap(ConvertWriteableBitmapToBitmapImage(img));
        }

        public void GenerateMap()
        {
            Map.GetWriteableBitmap().FillRectangle(0, 0, Map.Width, Map.Height, Map.White);
            for(int y= 0; y < Map.Height; y++)
            {
                for(int x = 0; x < Map.Width; x++)
                {
                    Map.CountryMap[x, y] = MapPixelType.UNASSIGNED_COUNTRY;
                }
            }

            for (int i = 0; i < Map.Width * Map.Height / 1000000f * LINE_DENSITY; i++)
            {
                activeLines += 2;
                int x = Random.Next(Map.Width - 6) + 3;
                int y = Random.Next(Map.Height - 6) + 3;

                int dir = Random.Next(360);
                ActionQueue.Add(() => Spread(x, y, dir));
                ActionQueue.Add(() => Spread(x, y, (dir + 180) % 360));
            }
        }

        private void Spread(int x, int y, int dir)
        {
            
            if (x < 0 || x >= Map.Width || y < 0 || y >= Map.Height) return;
            int dirChange = Random.Next(MAX_TURN_ANGLE);
            int newDir = (dir + dirChange - MAX_TURN_ANGLE / 2) % 360;

            CreateSegment(x, y, newDir);
            //Split
            if (Random.Next(1000) < SPLIT_CHANCE)
            {
                activeLines++;
                int mir = Random.Next(2) == 0 ? -1 : 1;
                int splitDir = mir * 90 + dir + Random.Next(60) - 30;
                CreateSegment(x, y, splitDir);
            }
        }

        private void CreateSegment(int x, int y, int dir)
        {
            int segLen = Random.Next(MAX_SEGMENT_LENGTH - MIN_SEGMENT_LENGTH) + MIN_SEGMENT_LENGTH;

            int newX = (int)(x + (Math.Sin(ToRad(dir)) * segLen));
            int newY = (int)(y + (Math.Cos(ToRad(dir)) * segLen));

            bool stop = false;
            for (int i = newX - SNAP_DISTANCE; i < newX + SNAP_DISTANCE; i++)
            {
                for (int j = newY - SNAP_DISTANCE; j < newY + SNAP_DISTANCE; j++)
                {
                    if (i >= 0 && i < Map.Width && j > 0 && j < Map.Height && Map.CountryMap[i, j] == MapPixelType.BORDER && !(i == x && j == y))
                    {
                        newX = i;
                        newY = j;
                        stop = true;
                    }
                }
            }

            if (newX < 0)
            {
                newX = 0;
                stop = true;
            }
            if (newX >= Map.Width)
            {
                newX = Map.Width - 1;
                stop = true;
            }
            if (newY < 0)
            {
                newY = 0;
                stop = true;
            }
            if (newY >= Map.Height)
            {
                newY = Map.Height - 1;
                stop = true;
            }

            Map.GetWriteableBitmap().DrawLine(x, y, newX, newY, Map.Black);
            Map.CountryMap[newX, newY] = MapPixelType.BORDER;
            if (!stop)
            {
                ActionQueue.Add(() => Spread(newX, newY, dir));
            }
            else activeLines--;
        }

        public double ToRad(double angle)
        {
            return (Math.PI / 180) * angle;
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
