using Conquest.MapClasses;
using Conquest.PlayerClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Conquest.Model
{
    class GameModel
    {
        private List<Player> Players;
        private List<Country> Countries;
        private List<Continent> Continents;
        private int[,] CountryMap;
        private Map Map;
        private Random Random;
        private Color white;
        private DateTime time;
        private bool loggedTime = false;

        private bool initialized = false;
        private DispatcherTimer ticks = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1 / 60) };
        private List<Action> ActionQueue;

        public GameModel()
        {
            Random = new Random();
            Players = new List<Player>();
            Countries = new List<Country>();
            Continents = new List<Continent>();
            ActionQueue = new List<Action>();
            Map = new Map();
            ticks.Tick += new EventHandler(Update);
            ticks.Start();
        }

        private void Update(object sender, EventArgs e)
        {
            if(ActionQueue.Count == 0)
            {
                if (!loggedTime && initialized)
                {
                    loggedTime = true;
                    Console.WriteLine((DateTime.Now - time).Minutes + ":" + (DateTime.Now - time).Seconds);
                }
                if (!initialized)
                {
                    Initialize();
                    initialized = true;
                }
            }
            else
            {
                Action action = ActionQueue[0];
                ActionQueue.Remove(action);
                action.Invoke();
            }
        }

        private void Initialize()
        {
            time = DateTime.Now;
            white = Map.GetPixel(0, 0);
            for(int y = 0; y < Map.Height; y++)
            {
                for(int x = 0; x < Map.Width; x++)
                {
                    byte[] colorData = new byte[3];
                    Random.NextBytes(colorData);
                    int realX = x;
                    int realY = y;
                    ActionQueue.Add(() => FloodFill(true, realX, realY, white, Color.FromArgb(0, colorData[0], colorData[1], colorData[2]), -1));
                }
            }
        }

        public void SetMap(BitmapImage image)
        {
            Map.SetMap(image);
            Map.RefreshMap();
            CountryMap = new int[Map.Width, Map.Height];
            for (int y = 0; y < Map.Height; y++)
            {
                for (int x = 0; x < Map.Width; x++)
                {
                    CountryMap[x, y] = -1;
                }
            }
        }

        public Image GetMapImage()
        {
            return Map.GetMapImage();
        }

        public Color GetMapPixel(int x, int y)
        {
            return Map.GetPixel(x, y);
        }

        public void SetMapPixel(int x, int y, Color c)
        {
            Map.SetPixel(x, y, c);
        }

        public void FloodFill(bool start, int x, int y, Color from, Color to, int id)
        {
            bool stop = false;
            if (CountryMap[x,y] != -1 || !Map.GetPixel(x, y).Equals(from)) stop = true;
            if(!stop) {
                if (start)
                {
                    id = Countries.Count();
                    Countries.Add(new Country(id));
                }
                Map.SetPixel(x, y, to);
                CountryMap[x, y] = id;
                ActionQueue.Insert(0, () => FloodFill(false, x - 1 < 0 ? 0 : x - 1, y, from, to, id));
                ActionQueue.Insert(0, () => FloodFill(false, x + 1 > Map.Width-1 ? Map.Width-1 : x + 1, y, from, to, id));
                ActionQueue.Insert(0, () => FloodFill(false, x, y - 1 < 0 ? 0 : y - 1, from, to, id));
                ActionQueue.Insert(0, () => FloodFill(false, x, y + 1 > Map.Height-1 ? Map.Height-1 : y + 1, from, to, id));
            }
        }
    }
}
