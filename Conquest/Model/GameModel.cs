using Conquest.MapClasses;
using Conquest.PlayerClasses;
using Conquest.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
            Playing,
            InTurn
        }

        private const int FPS_DEFAULT = 60;
        private const double FPS_IN_TURN = 2;
        private DateTime lastTurnUpdate;

        private const int SELECTED_WIDTH = 4;

        private const int UNASSIGNED_COUNTRY = -1;
        private const int BORDER = -2;
        private const int OCEAN = -3;
        private const int STARTVALUE = -99;
        private List<Point> CompletedPoints = new List<Point>();

        private MainWindow MainWindow;
        private UIManager UIManager;
        public List<Player> Players;
        public List<Country> Countries;
        public List<Continent> Continents;
        private int[,] CountryMap;
        private int[,] DistanceToNearestBorder;
        private Map Map;
        private static Random Random;
        public static Color White;
        public static Color Black;

        private int currentPlayer;

        private GameState State;
        private DispatcherTimer ticks = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1 / FPS_DEFAULT) };
        private List<Action> ActionQueue;

        public GameModel(MainWindow main, UIManager uiManager)
        {
            MainWindow = main;
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
                    for (int i = 0; i < 20; i++)
                    {
                        Action action = ActionQueue[0];
                        ActionQueue.Remove(action);
                        action.Invoke();
                        if (ActionQueue.Count == 0)
                        {
                            FindBorders();
                            SetCenters();
                            SetState(GameState.Idle);
                            break;
                        }
                    }
                    break;

                case GameState.Idle:
                case GameState.Playing:
                    if (ActionQueue.Count > 0)
                    {
                        Action action = ActionQueue[0];
                        ActionQueue.Remove(action);
                        action.Invoke();
                    }
                    break;

                case GameState.InTurn:
                    if (ActionQueue.Count > 0)
                    {
                        if ((DateTime.Now - lastTurnUpdate).Milliseconds > 1 / FPS_IN_TURN * 1000)
                        {
                            Action action = ActionQueue[0];
                            ActionQueue.Remove(action);
                            action.Invoke();
                            lastTurnUpdate = DateTime.Now;
                            if (Players.Where(p => p.Alive).Count() == 1)
                            {
                                Player winner = Players.Where(p => p.Alive).First();
                                Console.WriteLine("{0} won the game!", winner.PrimaryColor);
                                SetState(GameState.Idle);
                            }
                        }
                    }
                    else
                    {
                        Player next;
                        do
                        {
                            currentPlayer = (currentPlayer + 1) % Players.Count;
                            next = Players[currentPlayer];
                            UIManager.SetupPlayerOrderGrid(Players, currentPlayer);
                        } while (!next.Alive);
                        SetState(GameState.Playing);
                    }
                    break;

            }
        }

        //----------------------------GAME COMMANDS----------------------------------
        public void TakeCountry(Country c, Player p)
        {
            if (c.Player != null)
            {
                c.Player.Countries.Remove(c);
            }
            c.Player = p;
            p.Countries.Add(c);
            RefreshMap();
        }

        public void DistributeArmy(Country c)
        {
            c.Army++;
            RefreshMap();
        }

        public void Attack(Country attacker, Country defender)
        {
            if (!attacker.Selected)
            {
                ActionQueue.Add(() =>
                {
                    attacker.Selected = true;
                    RefreshMap();
                });
            }
            if (!defender.Selected)
            {
                ActionQueue.Add(() =>
                {
                    defender.Selected = true;
                    RefreshMap();
                });
            }
            ActionQueue.Add(() =>
            {
                if (defender.Army == 0) {
                    attacker.Selected = false;
                    defender.Selected = false;
                    if (defender.Player != null && defender.Player.Countries.Count == 1)
                        Console.WriteLine("{0} died!", defender.Player.PrimaryColor);
                    TakeCountry(defender, attacker.Player);
                    MoveArmy(attacker, defender, attacker.Army / 2);
                    RefreshMap();
                }
                else if (attacker.Army == 0) {
                    attacker.Selected = false;
                    defender.Selected = false;
                    RefreshMap();
                }
                else if (attacker.Army > 0 && defender.Army > 0)
                {
                    if (Random.Next(2) == 0) attacker.Army--;
                    else defender.Army--;
                    RefreshMap();
                    Attack(attacker, defender);
                }
            });
        }

        private void MoveArmy(Country source, Country target, int amount)
        {
            source.Army -= amount;
            target.Army += amount;
            RefreshMap();
        }

        //-----------------------------END GAME COMMANDS-------------------------------------

        public void NextTurn()
        {
            if (State != GameState.Playing) return;
            SetState(GameState.InTurn);

            Player p = Players[currentPlayer];
            OnStartTurn(p);

            ActionQueue.Add(() => p.DoTurn(this));
        }

        private void OnStartTurn(Player p)
        {
            for(int i = 0; i < p.Countries.Count; i++)
            {
                ActionQueue.Add(() => DistributeArmy(p.Countries[Random.Next(p.Countries.Count)]));
            }
        }

        private void RefreshMap()
        {
            MainWindow.Dispatcher.Invoke(() =>
            {
                foreach (Country c in Countries)
                {
                    Map.DrawCountry(c);
                    Map.DrawArmy(c);
                }
            });
        }

        public void SetMap(BitmapImage image)
        {
            Map.SetMap(image);
            Map.RefreshMap();
            CountryMap = new int[Map.Width, Map.Height];
            DistanceToNearestBorder = new int[Map.Width, Map.Height];
            for (int y = 0; y < Map.Height; y++)
            {
                for (int x = 0; x < Map.Width; x++)
                {
                    DistanceToNearestBorder[x, y] = int.MaxValue;
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

        //------------------------------------------------------INIT--------------------------------------------------
        private void Initialize()
        {
            for (int y = 0; y < Map.Height; y++)
            {
                for (int x = 0; x < Map.Width; x++)
                {
                    int realX = x;
                    int realY = y;
                    ActionQueue.Add(() => FloodFill(true, realX, realY, STARTVALUE));
                }
            }
            ActionQueue.Add(() => FindDistancesToNearestBorder());
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
                    CompletedPoints.Clear();
                }
                CountryMap[x, y] = id;
                Countries[id].AreaPixels.Add(new System.Windows.Point(x, y));
                if (!CompletedPoints.Contains(new Point(x - 1 < 0 ? 0 : x - 1, y))) {
                    CompletedPoints.Add(new Point(x - 1 < 0 ? 0 : x - 1, y));
                    ActionQueue.Insert(0, () => FloodFill(false, x - 1 < 0 ? 0 : x - 1, y, id));
                }
                if (!CompletedPoints.Contains(new Point(x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y))) {
                    CompletedPoints.Add(new Point(x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y));
                    ActionQueue.Insert(0, () => FloodFill(false, x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y, id));
                }
                if (!CompletedPoints.Contains(new Point(x, y - 1 < 0 ? 0 : y - 1)))
                {
                    CompletedPoints.Add(new Point(x, y - 1 < 0 ? 0 : y - 1));
                    ActionQueue.Insert(0, () => FloodFill(false, x, y - 1 < 0 ? 0 : y - 1, id));
                }
                if (!CompletedPoints.Contains(new Point(x, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1)))
                {
                    CompletedPoints.Add(new Point(x, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1));
                    ActionQueue.Insert(0, () => FloodFill(false, x, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1, id));
                }
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
                    Countries[CountryMap[x, y]].AddNeighbour(Countries[id]);
                    //TODO: Border length
                }
            }
        }

        private void FindDistancesToNearestBorder()
        {
            for(int y = 0; y < Map.Height; y++)
            {
                for(int x = 0; x < Map.Width; x++)
                {
                    if (x == 0 || x == Map.Width - 1 || y == 0 || y == Map.Height - 1 || Map.GetPixel(x, y).Equals(Black)) Spread(x, y, 0);
                }
            }
        }

        private void Spread(int x, int y, int distance)
        {
            DistanceToNearestBorder[x, y] = distance;
            ActionQueue.Add(() => { if (distance + 1 < DistanceToNearestBorder[x - 1 < 0 ? 0 : x - 1, y]) Spread(x - 1 < 0 ? 0 : x - 1, y, distance + 1); });
            ActionQueue.Add(() => { if (distance + 1 < DistanceToNearestBorder[x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y]) Spread(x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y, distance + 1); });
            ActionQueue.Add(() => { if (distance + 1 < DistanceToNearestBorder[x, y - 1 < 0 ? 0 : y - 1]) Spread(x, y - 1 < 0 ? 0 : y - 1, distance + 1); });
            ActionQueue.Add(() => { if (distance + 1 < DistanceToNearestBorder[x, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1]) Spread(x, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1, distance + 1); });
        }

        private void FindBorders()
        {
            foreach(Country c in Countries)
            {
                foreach(Point p in c.AreaPixels)
                {
                    if (DistanceToNearestBorder[(int)p.X,(int)p.Y] <= SELECTED_WIDTH) c.BorderPixels.Add(new Point(p.X, p.Y));
                }
            }
        }

        private void SetCenters()
        {
            foreach(Country c in Countries)
            {
                Point tempCenter = new Point(-1, -1);
                int furthestDistance = -1;
                foreach(Point p in c.AreaPixels)
                {
                    if (DistanceToNearestBorder[(int)p.X, (int)p.Y] > furthestDistance)
                    {
                        tempCenter = new Point(p.X, p.Y);
                        furthestDistance = DistanceToNearestBorder[(int)p.X, (int)p.Y];
                    }
                }
                c.Center = new Point(tempCenter.X, tempCenter.Y);
            }
        }
        //-----------------------------------------------------------------END INIT------------------------------------------------------------

        public void MouseMove(Object sender, MouseEventArgs e)
        {
            int x = (int)e.GetPosition(Map.GetMapImage()).X;
            int y = (int)e.GetPosition(Map.GetMapImage()).Y;
            if (CoordinatesOnMap(x, y)) {
                UIManager.SetCoordinates(x, y);
                UIManager.SetNearestBorder(DistanceToNearestBorder[x, y]);
                if (CountryMap[x, y] >= 0) UIManager.SetCountryInfo(Countries[CountryMap[x, y]]);
                else UIManager.SetCountryInfo(new Country(-1));
            }
        }

        public void MouseDown(Object sender, MouseButtonEventArgs e)
        {
            int x = (int)e.GetPosition(Map.GetMapImage()).X;
            int y = (int)e.GetPosition(Map.GetMapImage()).Y;
            if (CoordinatesOnMap(x, y) && CountryMap[x, y] >= 0)
            {
                Countries[CountryMap[x, y]].Selected = !Countries[CountryMap[x, y]].Selected;
                RefreshMap();
            }
        }

        private void ResetGame()
        {
            currentPlayer = 0;
            foreach (Country c in Countries)
            {
                c.Selected = false;
                c.Player = null;
                c.Army = 0;
            }
            Players.Clear();
            RefreshMap();
        }

        public void StartGame(int numPlayers, int numCountries, int numArmy)
        {
            if (State != GameState.Idle && State != GameState.Playing) return;
            ResetGame();
            for (int i = 0; i < numPlayers; i++)
            {
                Players.Add(new Player(Players.Count, RandomColor(Players.Select(p => p.PrimaryColor).ToArray())));
            }

            List<int> countryIds = new List<int>();
            for (int j = 0; j < Countries.Count; j++) countryIds.Add(j);
            for (int i = 0; i < numPlayers; i++)
            {
                int iReal = i;
                for (int j = 0; j < numCountries; j++)
                {
                    int id = countryIds[Random.Next(countryIds.Count)];
                    countryIds.Remove(id);
                    ActionQueue.Add(() => TakeCountry(Countries[id], Players[iReal]));
                    ActionQueue.Add(() => DistributeArmy(Countries[id]));
                }

                for(int j = 0; j < numArmy - numCountries; j++)
                {
                    ActionQueue.Add(() => DistributeArmy(Players[iReal].Countries[Random.Next(Players[iReal].Countries.Count)]));
                } 
            }
            SetState(GameState.Playing);
            UIManager.SetupPlayerOrderGrid(Players, currentPlayer);
        }

        private bool CoordinatesOnMap(int x, int y)
        {
            return x >= 0 && x < Map.Width && y >= 0 && y < Map.Height;
        }

        public static Color RandomColor(Color[] others = null, int tolerance = 150)
        {
            Color toReturn = Color.FromArgb(255, 0, 0, 0);
            bool tooSimilar = true;
            int counter = 0;
            while (tooSimilar && counter <= 20)
            {
                counter++;
                tooSimilar = false;
                byte[] colorData = new byte[3];
                Random.NextBytes(colorData);
                toReturn = Color.FromArgb(255, colorData[0], colorData[1], colorData[2]);
                if (others != null)
                {
                    foreach (Color other in others)
                    {
                        int diff = Math.Abs(other.R - toReturn.R) + Math.Abs(other.G - toReturn.G) + Math.Abs(other.B - toReturn.B);
                        if (diff < tolerance) tooSimilar = true;
                    }
                }
            }
            return toReturn;
        }
    }
}
