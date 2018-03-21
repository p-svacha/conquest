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
            Initializing_FindCountries,
            Initializing_FindNeighbours,
            Initializing_FindDistancesToNearestBorder,
            ReadyToPlay,
            Playing_Idle,
            Playing_DistrubutionPhase,
            Playing_AttackPhase,
            Playing_MovePhase
        }

        private const int FPS_DEFAULT = 400;
        private const int IN_TURN_UPDATE = 200; //ms

        private const int SELECTED_WIDTH = 4;

        private const int UNASSIGNED_COUNTRY = -1;
        private const int BORDER = -2;
        private const int OCEAN = -3;
        private const int STARTVALUE = -99;
        private bool[,] CompletedPoints;
        private int initX = 0;
        private int initY = 0;

        private MainWindow MainWindow;
        private UIManager UIManager;
        public List<Player> Players;
        public List<Country> Countries;
        public List<Continent> Continents;
        private int[,] CountryMap;
        private float[,] DistanceToNearestBorder;
        private Map Map;
        private static Random Random;
        public static Color White;
        public static Color Black;

        private int currentPlayer;

        private GameState State;
        private List<Action> ActionQueue;
        private List<Action> FindNeighboursQueue;
        private DateTime NextActionInvoke;

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
            FindNeighboursQueue = new List<Action>();
            Map = new Map();
        }

        public void Update()
        {
            switch(State)
            {
                case GameState.Started:
                    Initialize();
                    break;

                case GameState.Initializing_FindCountries:
                    if (!InvokeAction()) {
                        if (initX == Map.Width - 1 && initY == Map.Height - 1)
                        {
                            ActionQueue.AddRange(FindNeighboursQueue);
                            SetState(GameState.Initializing_FindNeighbours);
                        }
                        else
                        {
                            do
                            {
                                if (initX == Map.Width - 1)
                                {
                                    initX = 0;
                                    initY++;
                                }
                                else initX++;
                            } while (!(initX == Map.Width - 1 && initY == Map.Height - 1) && CompletedPoints[initX, initY]);
                            if (!(initX == Map.Width - 1 && initY == Map.Height - 1))
                                ActionQueue.Insert(0, () => FloodFill(true, initX, initY, STARTVALUE));
                        }
                    }
                    break;

                case GameState.Initializing_FindNeighbours:
                    if(!InvokeAction())
                    {
                        ActionQueue.Add(() => FindDistancesToNearestBorder(true));
                        FindNeighboursQueue.Clear();
                        SetState(GameState.Initializing_FindDistancesToNearestBorder);
                    }
                    break;

                case GameState.Initializing_FindDistancesToNearestBorder:
                    if(!InvokeAction())
                    {
                        SetSelectedBorders();
                        SetCenters();
                        SetState(GameState.ReadyToPlay);
                    }
                    break;

                case GameState.ReadyToPlay:
                case GameState.Playing_Idle:
                    InvokeAction(IN_TURN_UPDATE);
                    break;

                case GameState.Playing_DistrubutionPhase:
                    if (!InvokeAction(IN_TURN_UPDATE))
                    {
                        ActionQueue.Add(() => Players[currentPlayer].DoTurn(this));
                        SetState(GameState.Playing_AttackPhase);
                    }
                    break;

                case GameState.Playing_AttackPhase:
                    if (!InvokeAction(IN_TURN_UPDATE))
                    {
                        ActionQueue.Add(() => Players[currentPlayer].EndTurn(this));
                        SetState(GameState.Playing_MovePhase);
                    }
                    break;

                case GameState.Playing_MovePhase:
                    if (!InvokeAction(IN_TURN_UPDATE))
                    {
                        Player next;
                        do
                        {
                            currentPlayer = (currentPlayer + 1) % Players.Count;
                            next = Players[currentPlayer];
                            UIManager.SetupPlayerOrderGrid(Players, currentPlayer);
                        } while (!next.Alive);
                        SetState(GameState.Playing_Idle);
                    }
                    break;
            }
        }

        int numActions = 0;
        DateTime lastAction = DateTime.Now;
        /// <summary>
        /// Invokes the next action in the action queue. Returns false if the queue is empty.
        /// </summary>
        private bool InvokeAction(int waitAfterAction = 0)
        {
            if (ActionQueue.Count == 0) return false;
            if (DateTime.Now > NextActionInvoke)
            {
                numActions++;
                if((DateTime.Now - lastAction).TotalMilliseconds > 1000)
                {
                    Console.WriteLine("{0} actions / sec", numActions);
                    numActions = 0;
                    lastAction = DateTime.Now;
                }
                Action action = ActionQueue[0];
                ActionQueue.Remove(action);
                action.Invoke();
                NextActionInvoke = DateTime.Now + TimeSpan.FromMilliseconds(waitAfterAction);
                if(waitAfterAction > 0) RefreshMap();
                CheckGameOver();
            }
            return true;
        }

        private void RefreshMap()
        {
            MainWindow.Dispatcher.Invoke(() =>
            {
                Map.RefreshMap();
                UIManager.RefreshGraphs(Players);
                foreach (Country c in Countries)
                {
                    Map.DrawCountry(c);
                    Map.DrawArmy(c);
                }
            });
        }

        private void CheckGameOver()
        {
            if (Players.Where(p => p.Alive).Count() == 1)
            {
                Player winner = Players.Where(p => p.Alive).First();
                Console.WriteLine("{0} won the game!", winner.PrimaryColor);
                SetState(GameState.ReadyToPlay);
            }
        }

        //----------------------------GAME COMMANDS----------------------------------
        public void TakeCountry(Country c, Player p)
        {
            ActionQueue.Add(() =>
            {
                if (c.Player != null)
                {
                    c.Player.Countries.Remove(c);
                }
                c.Player = p;
                p.Countries.Add(c);
            });
        }

        public void DistributeArmy(Country c)
        {
            ActionQueue.Add(() =>
            {
                c.Army++;
            });
        }

        public void Attack(Country attacker, Country defender)
        {
            if (!attacker.Selected)
            {
                ActionQueue.Add(() =>
                {
                    attacker.Selected = true;
                });
            }
            if (!defender.Selected)
            {
                ActionQueue.Add(() =>
                {
                    defender.Selected = true;
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
                }
                else if (attacker.Army == 0) {
                    attacker.Selected = false;
                    defender.Selected = false;
                }
                else if (attacker.Army > 0 && defender.Army > 0)
                {
                    if (Random.Next(2) == 0) attacker.Army--;
                    else defender.Army--;
                    Attack(attacker, defender);
                }
            });
        }

        private void MoveArmy(Country source, Country target, int amount)
        {
            ActionQueue.Add(() =>
            {
                source.Army -= amount;
                target.Army += amount;
            });
        }

        //-----------------------------END GAME COMMANDS-------------------------------------

        public void NextTurn()
        {
            if (State != GameState.Playing_Idle) return;
            SetState(GameState.Playing_DistrubutionPhase);

            ActionQueue.Add(() => Players[currentPlayer].StartTurn(this));
        }

        public void SetMap(BitmapImage image)
        {
            Map.SetMap(image);
            Map.RefreshMap();
            CountryMap = new int[Map.Width, Map.Height];
            CompletedPoints = new bool[Map.Width, Map.Height];
            DistanceToNearestBorder = new float[Map.Width, Map.Height];
            for (int y = 0; y < Map.Height; y++)
            {
                for (int x = 0; x < Map.Width; x++)
                {
                    DistanceToNearestBorder[x, y] = int.MaxValue;
                    if (Map.GetPixel(x, y).Equals(White)) CountryMap[x, y] = UNASSIGNED_COUNTRY;
                    else if (Map.GetPixel(x, y).Equals(Black))
                    {
                        CountryMap[x, y] = BORDER;
                        CompletedPoints[x, y] = true;
                    }
                    else
                    {
                        CountryMap[x, y] = OCEAN;
                        CompletedPoints[x, y] = true;
                    }
                }
            }
        }

        public Image GetMapImage()
        {
            return Map.GetMapImage();
        }

        private void SetState(GameState state)
        {
            Console.WriteLine("Changed from state {0} to {1}.", State.ToString(), state.ToString());
            State = state;
        }

        //------------------------------------------------------INIT--------------------------------------------------
        private void Initialize()
        {
            ActionQueue.Add(() => FloodFill(true, 0, 0, STARTVALUE));
            SetState(GameState.Initializing_FindCountries);
        }

        public void FloodFill(bool start, int x, int y, int id)
        {
            bool stop = false;
            if (CountryMap[x, y] == id) stop = true;
            else if (CountryMap[x, y] != UNASSIGNED_COUNTRY)
            {
                stop = true;
                if(!start) FindNeighboursQueue.Add(() => FindNeighbour(x, y, id, 0));
            }
            if(!stop) {
                if (start)
                {
                    id = Countries.Count();
                    Countries.Add(new Country(id));
                }
                CountryMap[x, y] = id;
                Countries[id].AreaPixels.Add(new System.Windows.Point(x, y));
                if (!CompletedPoints[x - 1 < 0 ? 0 : x - 1, y]) {
                    CompletedPoints[x - 1 < 0 ? 0 : x - 1, y] = true;
                    ActionQueue.Insert(0, () => FloodFill(false, x - 1 < 0 ? 0 : x - 1, y, id));
                }
                if (!CompletedPoints[x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y]) {
                    CompletedPoints[x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y] = true;
                    ActionQueue.Insert(0, () => FloodFill(false, x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y, id));
                }
                if (!CompletedPoints[x, y - 1 < 0 ? 0 : y - 1])
                {
                    CompletedPoints[x, y - 1 < 0 ? 0 : y - 1] = true;
                    ActionQueue.Insert(0, () => FloodFill(false, x, y - 1 < 0 ? 0 : y - 1, id));
                }
                if (!CompletedPoints[x, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1])
                {
                    CompletedPoints[x, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1] = true;
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
                    //TODO: Ocean borders
                }
                else
                {
                    Countries[id].AddNeighbour(Countries[CountryMap[x, y]]);
                    Countries[CountryMap[x, y]].AddNeighbour(Countries[id]);
                }
            }
        }

        private void FindDistancesToNearestBorder(bool fast)
        {
            if (fast)
            {
                {
                    for (int y = 0; y < Map.Height - 1; y++)
                    {
                        for (int x = 0; x < Map.Width - 1; x++)
                        {
                            if (CountryMap[x, y] >= 0)
                            {
                                int realX = x;
                                int realY = y;
                                ActionQueue.Add(() =>
                                {
                                    int range = 15;
                                    bool borderFound = false;
                                    int nearX = 0; int nearY = 0;
                                    float minDistance = int.MaxValue;
                                    while (!borderFound)
                                    {
                                        for (int dy = realY - range; dy < realY + range; dy++)
                                        {
                                            for (int dx = realX - range; dx < realX + range; dx++)
                                            {
                                                if (dx >= 0 && dx < Map.Width && dy >= 0 && dy < Map.Height)
                                                {
                                                    Console.WriteLine("Border for {1}/{2} with {3}/{4} and range {5} with distance of {0}", "rr", realX, realY, dx, dy, range);
                                                    if (CountryMap[dx, dy] == BORDER)
                                                    {
                                                        borderFound = true;
                                                        float distance = (float)Math.Sqrt(Math.Pow(dx - realX, 2) + Math.Pow(dy - realY, 2));
                                                        if (distance < minDistance)
                                                        {
                                                            minDistance = distance;
                                                            nearX = dx;
                                                            nearY = dy;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        range += 15;
                                    }
                                    DistanceToNearestBorder[x, y] = minDistance;
                                });
                            }
                        }
                    }
                }
            }
            else
            {
                for (int y = 0; y < Map.Height; y++)
                {
                    for (int x = 0; x < Map.Width; x++)
                    {
                        if (x == 0 || x == Map.Width - 1 || y == 0 || y == Map.Height - 1 || CountryMap[x, y] == BORDER) Spread(x, y, 0);
                    }
                }
            }
        }

        private void Spread(int x, int y, float distance)
        {
            if (distance + 1 < DistanceToNearestBorder[x - 1 < 0 ? 0 : x - 1, y])
            {
                ActionQueue.Add(() => Spread(x - 1 < 0 ? 0 : x - 1, y, distance + 1f));
                DistanceToNearestBorder[x - 1 < 0 ? 0 : x - 1, y] = distance + 1f;
            }
            if (distance + 1 < DistanceToNearestBorder[x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y])
            {
                ActionQueue.Add(() => Spread(x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y, distance + 1f));
                DistanceToNearestBorder[x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y] = distance + 1f;
            }
            if (distance + 1 < DistanceToNearestBorder[x, y - 1 < 0 ? 0 : y - 1])
            {
                ActionQueue.Add(() => Spread(x, y - 1 < 0 ? 0 : y - 1, distance + 1f));
                DistanceToNearestBorder[x, y - 1 < 0 ? 0 : y - 1] = distance + 1f;
            }
            if (distance + 1 < DistanceToNearestBorder[x, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1])
            {
                ActionQueue.Add(() => Spread(x, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1, distance + 1f));
                DistanceToNearestBorder[x, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1] = distance + 1f;
            }

            if (distance + 1.414f < DistanceToNearestBorder[x - 1 < 0 ? 0 : x - 1, y - 1 < 0 ? 0 : y - 1])
            {
                ActionQueue.Add(() => Spread(x - 1 < 0 ? 0 : x - 1, y - 1 < 0 ? 0 : y - 1, distance + 1.414f));
                DistanceToNearestBorder[x - 1 < 0 ? 0 : x - 1, y - 1 < 0 ? 0 : y - 1] = distance + 1.414f;
            }

            if (distance + 1.414f < DistanceToNearestBorder[x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y - 1 < 0 ? 0 : y - 1])
            {
                ActionQueue.Add(() => Spread(x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y - 1 < 0 ? 0 : y - 1, distance + 1.414f));
                DistanceToNearestBorder[x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y - 1 < 0 ? 0 : y - 1] = distance + 1.414f;
            }

            if (distance + 1.414f < DistanceToNearestBorder[x - 1 < 0 ? 0 : x - 1, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1])
            {
                ActionQueue.Add(() => Spread(x - 1 < 0 ? 0 : x - 1, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1, distance + 1.414f));
                DistanceToNearestBorder[x - 1 < 0 ? 0 : x - 1, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1] = distance + 1.414f;
            }

            if (distance + 1.414f < DistanceToNearestBorder[x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1])
            {
                ActionQueue.Add(() => Spread(x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1, distance + 1.414f));
                DistanceToNearestBorder[x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1] = distance + 1.414f;
            }


        }

        private void SetSelectedBorders()
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
                float furthestDistance = -1;
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
            if ((State != GameState.ReadyToPlay && State != GameState.Playing_Idle) || numPlayers * numCountries > Countries.Count || numArmy > 50) return;
            ResetGame();
            for (int i = 0; i < numPlayers; i++)
            {
                Players.Add(new Player(Players.Count, RandomColor(Players.Select(p => p.PrimaryColor).ToArray())));
            }

            List<int> countryIds = new List<int>();
            List<int> playerCountryIds = new List<int>();
            for (int j = 0; j < Countries.Count; j++) countryIds.Add(j);
            for (int i = 0; i < numPlayers; i++)
            {
                playerCountryIds.Clear();
                for (int j = 0; j < numCountries; j++)
                {
                    int id = countryIds[Random.Next(countryIds.Count)];
                    countryIds.Remove(id);
                    playerCountryIds.Add(id);
                    TakeCountry(Countries[id], Players[i]);
                    DistributeArmy(Countries[id]);
                }

                for(int j = 0; j < numArmy - numCountries; j++)
                {
                    DistributeArmy(Countries[playerCountryIds[Random.Next(playerCountryIds.Count)]]);
                } 
            }
            SetState(GameState.Playing_Idle);
            ActionQueue.Add(() => UIManager.SetupPlayerOrderGrid(Players, currentPlayer));
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
