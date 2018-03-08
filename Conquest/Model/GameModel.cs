using Conquest.MapClasses;
using Conquest.PlayerClasses;
using Conquest.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Conquest.Model
{
    class GameModel
    {
        enum GameState
        {
            Started,
            Initializing,
            Idle,
            Playing
        }
        private const int UNASSIGNED_COUNTRY = -1;
        private const int BORDER = -2;
        private const int OCEAN = -3;
        private const int STARTVALUE = -99;

        private UIManager UIManager;
        private List<Player> Players;
        private List<Country> Countries;
        private List<Continent> Continents;
        private int[,] CountryMap;
        private Map Map;
        private Random Random;
        private Color White;
        private Color Black;


        private GameState State;
        private DispatcherTimer ticks = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1 / 60) };
        private List<Action> ActionQueue;

        public GameModel(UIManager uiManager)
        {
            White = Color.FromArgb(255, 255, 255, 255);
            Black = Color.FromArgb(255, 0, 0, 0);
            UIManager = uiManager;
            State = GameState.Started;
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
            switch(State)
            {
                case GameState.Started:
                    Initialize();
                    SetState(GameState.Initializing);
                    break;

                case GameState.Initializing:
                    Action action = ActionQueue[0];
                    ActionQueue.Remove(action);
                    action.Invoke();
                    if (ActionQueue.Count == 0) SetState(GameState.Idle);
                    break;
            }
        }

        private void TakeCountry(Country c, Player p)
        {
            if(c.Player != null)
            {
                c.Player.Countries.Remove(c);
            }
            c.Player = p;
            p.Countries.Add(c);
            Map.FillCountry(c, p.Color);
        }

        private void Initialize()
        {
            for(int y = 0; y < Map.Height; y++)
            {
                for(int x = 0; x < Map.Width; x++)
                {
                    int realX = x;
                    int realY = y;
                    ActionQueue.Add(() => FloodFill(true, realX, realY, STARTVALUE));
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
                    if (Map.GetPixel(x, y).Equals(White)) CountryMap[x, y] = UNASSIGNED_COUNTRY;
                    else if (Map.GetPixel(x, y).Equals(Black)) CountryMap[x, y] = BORDER;
                    else CountryMap[x, y] = OCEAN;
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

        private void SetState(GameState state)
        {
            Console.WriteLine("Changed from state {0} to {1}.", State.ToString(), state.ToString());
            State = state;
        }

        public void FloodFill(bool start, int x, int y, int id)
        {
            bool stop = false;
            if (CountryMap[x, y] == id) stop = true;
            else if (CountryMap[x, y] != UNASSIGNED_COUNTRY)
            {
                stop = true;
                if(!start) ActionQueue.Add(() => FindNeighbour(x, y, id, 0));
            }
            if(!stop) {
                if (start)
                {
                    id = Countries.Count();
                    Countries.Add(new Country(id));
                }
                //Map.SetPixel(x, y, to);
                CountryMap[x, y] = id;
                Countries[id].Pixels.Add(new System.Windows.Point(x, y));
                ActionQueue.Insert(0, () => FloodFill(false, x - 1 < 0 ? 0 : x - 1, y, id));
                ActionQueue.Insert(0, () => FloodFill(false, x + 1 > Map.Width-1 ? Map.Width-1 : x + 1, y, id));
                ActionQueue.Insert(0, () => FloodFill(false, x, y - 1 < 0 ? 0 : y - 1, id));
                ActionQueue.Insert(0, () => FloodFill(false, x, y + 1 > Map.Height-1 ? Map.Height-1 : y + 1, id));
            }
        }

        private void FindNeighbour(int x, int y, int id, int step)
        {
            bool stop = false;
            if (step > 2 || CountryMap[x, y] == id) stop = true;
            if (!stop)
            {
                if (CountryMap[x, y] == BORDER)
                {
                    ActionQueue.Add(() => FindNeighbour(x - 1 < 0 ? 0 : x - 1, y, id, step + 1));
                    ActionQueue.Add(() => FindNeighbour(x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y, id, step + 1));
                    ActionQueue.Add(() => FindNeighbour(x, y - 1 < 0 ? 0 : y - 1, id, step + 1));
                    ActionQueue.Add(() => FindNeighbour(x, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1, id, step + 1));
                }
                else if(CountryMap[x, y] == OCEAN)
                {
                    
                }
                else
                {
                    Countries[id].AddNeighbour(Countries[CountryMap[x, y]]);
                    //TODO: Border length
                }
            }
        }

        public void MouseMove(Object sender, MouseEventArgs e)
        {
            int x = (int)e.GetPosition(Map.GetMapImage()).X;
            int y = (int)e.GetPosition(Map.GetMapImage()).Y;
            if (CoordinatesOnMap(x, y)) {
                UIManager.SetCoordinates(x, y);
                if (CountryMap[x, y] >= 0) UIManager.SetCountryInfo(Countries[CountryMap[x, y]]);
                else UIManager.SetCountryInfo(new Country(-1));
            }
        }

        public void MouseDown(Object sender, MouseButtonEventArgs e)
        {

        }

        private void ResetGame()
        {
            foreach (Country c in Countries)
            {
                Map.FillCountry(c, White);
                c.Player = null;
            }
            Players.Clear();
        }

        public void StartGame(int numPlayers, int numCountries)
        {
            if (State == GameState.Started || State == GameState.Initializing) return;
            ResetGame();
            for (int i = 0; i < numPlayers; i++) Players.Add(new Player(RandomColor()));

            List<int> countryIds = new List<int>();
            for (int j = 0; j < Countries.Count; j++) countryIds.Add(j);
            for (int i = 0; i < numPlayers; i++)
            {
                for(int j = 0; j < numCountries; j++)
                {
                    int id = countryIds[Random.Next(countryIds.Count)];
                    Console.WriteLine(id);
                    countryIds.Remove(id);
                    TakeCountry(Countries[id], Players[i]);
                }
            }
            SetState(GameState.Playing);
        }

        private bool CoordinatesOnMap(int x, int y)
        {
            return x >= 0 && x < Map.Width && y >= 0 && y < Map.Height;
        }

        public Color RandomColor()
        {
            byte[] colorData = new byte[3];
            Random.NextBytes(colorData);
            return Color.FromArgb(0, colorData[0], colorData[1], colorData[2]);
        }
    }
}
