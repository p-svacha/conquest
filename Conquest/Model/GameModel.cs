using Conquest.IO;
using Conquest.MapClasses;
using Conquest.MapGeneration;
using Conquest.PlayerClasses;
using Conquest.UI;
using Conquest.Windows;
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
            Null,
            GeneratingMap,
            ReadyToInitialize,
            Initializing_FindCountries,
            Initializing_FindNeighbours,
            Initializing_FindDistancesToNearestBorder,
            ReadyToPlay,
            Playing_Initialize,
            Playing_Idle,
            Playing_DistrubutionPhase,
            Playing_AttackPhase,
            Playing_MovePhase
        }

        private int CountryMinSize;
        private int CountriesPerOcean;
        private int islandConnections = 1; // do not change this
        private int MaxWaterConnectionDistance = 40;
        private int MinWaterConnectionCountryDistance; // countries will not be connected when within this range (3 = min. 2 countries in between)

        private const int FPS_DEFAULT = 400;

        private const int SELECTED_WIDTH = 4;
        
        private int initX = 0;
        private int initY = 0;

        private MainWindow MainWindow;
        private UIManager UIManager;
        private CheckBox AutoRun;
        private TextBox RunSpeed;
        public List<Player> Players;
        private Map Map;
        private static Random Random;

        private Dictionary<int, int> IdReplacements;
        private int currentPlayer;

        private GameState State;
        private List<Action> ActionQueue;
        private bool attackAgain;
        private List<Action> FindNeighboursQueue;
        private DateTime NextActionInvoke;

        public GameModel(MainWindow main, UIManager uiManager, CheckBox autoRun, TextBox runSpeed, bool generatingMap)
        {
            IdReplacements = new Dictionary<int, int>();
            State = GameState.Null;
            MainWindow = main;
            UIManager = uiManager;
            AutoRun = autoRun;
            RunSpeed = runSpeed;
            Random = new Random();
            Players = new List<Player>();
            ActionQueue = new List<Action>();
            FindNeighboursQueue = new List<Action>();
            Map = new Map();

            if(!generatingMap)
            {
                Map.SetMap(new BitmapImage(new Uri("../../Resources/Maps/test4.png", UriKind.Relative)));
                Map.Init();
                UIManager.Init(Map.Width);
                SetState(GameState.ReadyToInitialize);
            }
        }

        public void Update()
        {
            switch(State)
            {
                case GameState.GeneratingMap:
                    if(!InvokeAction())
                    {
                        Map.SetMap(MapGenerator.ConvertWriteableBitmapToBitmapImage(Map.GetWriteableBitmap()));
                        Map.Init();
                        SetState(GameState.ReadyToInitialize);
                    }
                    break;

                case GameState.ReadyToInitialize:
                    Initialize();
                    break;

                case GameState.Initializing_FindCountries:
                    if (!InvokeAction()) {
                        if (initX == Map.Width - 1 && initY == Map.Height - 1)
                        {
                            CreateWaters();
                            RemoveUnnecessaryBorders();
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
                            } while (!(initX == Map.Width - 1 && initY == Map.Height - 1) && Map.CompletedPoints[initX, initY]);
                            if (!(initX == Map.Width - 1 && initY == Map.Height - 1))
                                ActionQueue.Insert(0, () => FloodFill(true, initX, initY, MapPixelType.STARTVALUE));
                        }
                    }
                    break;

                case GameState.Initializing_FindNeighbours:
                    if(!InvokeAction())
                    {
                        DistanceToNearestBorderFinder finder = new DistanceToNearestBorderFinder(Map, ActionQueue);
                        ActionQueue.Add(() => finder.FindDistancesToNearestBorder(NearestBorderAlgorithm.BorderSpreadFourDirections));
                        FindNeighboursQueue.Clear();
                        SetState(GameState.Initializing_FindDistancesToNearestBorder);
                    }
                    break;

                case GameState.Initializing_FindDistancesToNearestBorder:
                    if(!InvokeAction())
                    {
                        SetSelectedBorders();
                        SetCenters();
                        ConnectIslands();
                        ConnectCloseCountriesOverWater(MaxWaterConnectionDistance);
                        Map.Name = "xxx";
                        MapSaver.SaveMap(Map, MapSelection.MAP_PATH);
                        SetState(GameState.ReadyToPlay);
                    }
                    break;

                case GameState.Playing_Initialize:
                    if(!InvokeAction(int.Parse(RunSpeed.Text)))
                    {
                        SetState(GameState.Playing_Idle);
                    }
                    break;

                case GameState.ReadyToPlay:
                    InvokeAction(int.Parse(RunSpeed.Text));
                    break;

                case GameState.Playing_Idle:
                    if (!InvokeAction(int.Parse(RunSpeed.Text)) && AutoRun.IsChecked == true) NextTurn();
                    CheckGameOver();
                    break;

                case GameState.Playing_DistrubutionPhase:
                    if (!InvokeAction(int.Parse(RunSpeed.Text)))
                    {
                        ActionQueue.Add(() => attackAgain = Players[currentPlayer].DoTurn(this));
                        SetState(GameState.Playing_AttackPhase);
                    }
                    break;

                case GameState.Playing_AttackPhase:
                    if (!InvokeAction(int.Parse(RunSpeed.Text)))
                    {
                        if (attackAgain) ActionQueue.Add(() => attackAgain = Players[currentPlayer].DoTurn(this));
                        else
                        {
                            ActionQueue.Add(() => Players[currentPlayer].EndTurn(this));
                            SetState(GameState.Playing_MovePhase);
                        }
                    }
                    break;

                case GameState.Playing_MovePhase:
                    if (!InvokeAction(int.Parse(RunSpeed.Text)))
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
        private bool InvokeAction(int waitAfterAction = 0, bool forceNoRefresh = false)
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
                //if(waitAfterAction > 0 && !forceNoRefresh) RefreshMap();
            }
            return true;
        }

        private void RefreshMap()
        {
            MainWindow.Dispatcher.Invoke(() =>
            {
                DateTime start = DateTime.Now;
                Map.RefreshMap();
                UIManager.RefreshGraphs(Players);
                foreach (Country c in Map.Countries)
                {
                    Map.DrawCountry(c);
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
                Map.DrawCountry(c);
                UIManager.RefreshGraphs(Players);
            });
        }

        public void DistributeArmy(Country c)
        {
            c.Army++;
            ActionQueue.Add(() =>
            {
                Map.DrawCountry(c);
                UIManager.RefreshGraphs(Players);
            });
        }

        public void Attack(Country attacker, Country defender)
        {
            if (!attacker.Selected)
            {
                ActionQueue.Add(() =>
                {
                    attacker.Selected = true;
                    Map.DrawCountry(attacker);
                    UIManager.RefreshGraphs(Players);
                });
            }
            if (!defender.Selected)
            {
                ActionQueue.Add(() =>
                {
                    defender.Selected = true;
                    Map.DrawCountry(defender);
                    UIManager.RefreshGraphs(Players);
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
                Map.DrawCountry(attacker);
                Map.DrawCountry(defender);
                UIManager.RefreshGraphs(Players);
            });
        }

        public void MoveArmy(Country source, Country target, int amount)
        {
            ActionQueue.Add(() =>
            {
                if (target.Army + amount > target.MaxArmy) amount = target.MaxArmy - target.Army;
                source.Army -= amount;
                target.Army += amount;
                Map.DrawCountry(source);
                Map.DrawCountry(target);
                UIManager.RefreshGraphs(Players);
            });
        }

        //-----------------------------END GAME COMMANDS-------------------------------------

        public void NextTurn()
        {
            if (State != GameState.Playing_Idle) return;
            SetState(GameState.Playing_DistrubutionPhase);

            ActionQueue.Add(() => Players[currentPlayer].StartTurn(this));
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
            ActionQueue.Add(() => FloodFill(true, 0, 0, MapPixelType.STARTVALUE));
            SetState(GameState.Initializing_FindCountries);
        }

        public void FloodFill(bool start, int x, int y, int id)
        {
            bool stop = false;
            if (Map.CountryMap[x, y] == id) stop = true;
            else if (Map.CountryMap[x, y] != MapPixelType.UNASSIGNED_COUNTRY)
            {
                stop = true;
                if(!start) FindNeighboursQueue.Add(() => FindNeighbour(x, y, id, 0));
            }
            if(!stop) {
                if (start)
                {
                    id = Map.Countries.Count();
                    Map.Countries.Add(new Country(id, RandomColor()));
                }
                Map.CountryMap[x, y] = id;
                Map.SetPixel(x, y, Map.Countries[id].Color);
                Map.Countries[id].AreaPixels.Add(new System.Windows.Point(x, y));
                if (!Map.CompletedPoints[x - 1 < 0 ? 0 : x - 1, y]) {
                    Map.CompletedPoints[x - 1 < 0 ? 0 : x - 1, y] = true;
                    ActionQueue.Insert(0, () => FloodFill(false, x - 1 < 0 ? 0 : x - 1, y, id));
                }
                if (!Map.CompletedPoints[x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y]) {
                    Map.CompletedPoints[x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y] = true;
                    ActionQueue.Insert(0, () => FloodFill(false, x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y, id));
                }
                if (!Map.CompletedPoints[x, y - 1 < 0 ? 0 : y - 1])
                {
                    Map.CompletedPoints[x, y - 1 < 0 ? 0 : y - 1] = true;
                    ActionQueue.Insert(0, () => FloodFill(false, x, y - 1 < 0 ? 0 : y - 1, id));
                }
                if (!Map.CompletedPoints[x, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1])
                {
                    Map.CompletedPoints[x, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1] = true;
                    ActionQueue.Insert(0, () => FloodFill(false, x, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1, id));
                }
            }
        }

        private void CreateWaters()
        {
            // Replace small countries with lakes
            foreach (Country c in Map.Countries)
            {
                if (c.AreaPixels.Count < CountryMinSize)
                {
                    Map.DrawCountry(c, true);
                }
                else Map.DrawCountry(c);
            }
            Map.Countries = Map.Countries.Where(c => c.AreaPixels.Count > CountryMinSize).ToList();

            // Replace huge countries with ocean
            int oceanAmount = Map.Countries.Count / CountriesPerOcean;
            List<Country> newOceans = Map.Countries.OrderByDescending(x => x.AreaPixels.Count).Take(oceanAmount).ToList();
            Map.Countries = Map.Countries.Except(newOceans).ToList();
            foreach (Country c in newOceans)
            {
                Map.DrawCountry(c, true);
            }

            // Apply changes
            for (int i = 0; i < Map.Countries.Count; i++)
            {
                IdReplacements.Add(Map.Countries[i].Id, i);
                Map.Countries[i].Id = i;
            }
            for(int y = 0; y < Map.Height; y++)
            {
                for(int x = 0; x < Map.Width; x++)
                {
                    if (IdReplacements.ContainsKey(Map.CountryMap[x, y])) Map.CountryMap[x, y] = IdReplacements[Map.CountryMap[x, y]];
                    else if (Map.CountryMap[x, y] >= 0) Map.CountryMap[x, y] = MapPixelType.OCEAN;
                }
            }
            Map.SetMap(MapGenerator.ConvertWriteableBitmapToBitmapImage(Map.GetWriteableBitmap()), false);
            RefreshMap();
        }

        private void RemoveUnnecessaryBorders()
        {
            for(int y = 0; y < Map.Height; y++)
            {
                for(int x = 0; x < Map.Width; x++)
                {
                    if (Map.CountryMap[x, y] == MapPixelType.BORDER)
                    {
                        List<int> borderingCountries = new List<int>();
                        if (x > 0 && !borderingCountries.Contains(Map.CountryMap[x - 1, y]) && Map.CountryMap[x - 1, y] != MapPixelType.BORDER) borderingCountries.Add(Map.CountryMap[x - 1, y]);
                        if (x < Map.Width - 1 && !borderingCountries.Contains(Map.CountryMap[x + 1, y]) && Map.CountryMap[x + 1, y] != MapPixelType.BORDER) borderingCountries.Add(Map.CountryMap[x + 1, y]);
                        if (y > 0 && !borderingCountries.Contains(Map.CountryMap[x, y - 1]) && Map.CountryMap[x, y - 1] != MapPixelType.BORDER) borderingCountries.Add(Map.CountryMap[x, y - 1]);
                        if (y < Map.Height - 1 && !borderingCountries.Contains(Map.CountryMap[x, y + 1]) && Map.CountryMap[x, y + 1] != MapPixelType.BORDER) borderingCountries.Add(Map.CountryMap[x, y + 1]);
                        if (borderingCountries.Count == 1)
                        {
                            Map.CountryMap[x, y] = borderingCountries[0];
                            if (Map.CountryMap[x, y] >= 0)
                            {
                                Map.Countries[Map.CountryMap[x, y]].AreaPixels.Add(new Point(x, y));
                                Map.SetPixel(x, y, Map.White);
                            }
                            else Map.SetPixel(x, y, BackgroundColor(x, y, Map.Width, Map.Height));
                        }
                    }
                }
            }
            Map.SetMap(MapGenerator.ConvertWriteableBitmapToBitmapImage(Map.GetWriteableBitmap()), false);
            RefreshMap();
        }

        private void FindNeighbour(int x, int y, int id, int step)
        {
            if (step == 0)
            {
                if (!IdReplacements.ContainsKey(id)) return;
                id = IdReplacements[id];
            }
            bool stop = false;
            if (step > 2 || Map.CountryMap[x, y] == id) stop = true;
            if (!stop)
            {
                if (Map.CountryMap[x, y] == MapPixelType.BORDER)
                {
                    ActionQueue.Add(() => FindNeighbour(x - 1 < 0 ? 0 : x - 1, y, id, step + 1));
                    ActionQueue.Add(() => FindNeighbour(x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y, id, step + 1));
                    ActionQueue.Add(() => FindNeighbour(x, y - 1 < 0 ? 0 : y - 1, id, step + 1));
                    ActionQueue.Add(() => FindNeighbour(x, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1, id, step + 1));
                }
                else if(Map.CountryMap[x, y] == MapPixelType.OCEAN)
                {
                    //TODO: Ocean borders
                }
                else
                {
                    Map.Countries[id].AddNeighbour(Map.Countries[Map.CountryMap[x, y]]);
                    Map.Countries[Map.CountryMap[x, y]].AddNeighbour(Map.Countries[id]);
                }
            }
        }

        private void ConnectIslands()
        {
            List<HashSet<Country>> Clusters = new List<HashSet<Country>>();
            foreach(Country c in Map.Countries)
            {
                // Create new cluster if country is not in one
                if(Clusters.Where(x => x.Contains(c)).Count() == 0)
                {
                    HashSet<Country> newCluster = new HashSet<Country>();
                    newCluster.Add(c);
                    foreach (Country n in c.Neighbours) newCluster.Add(n);
                    Clusters.Add(newCluster);
                }
                // Add to cluster if country already is in one
                else
                {
                    HashSet<Country> cluster = Clusters.Where(x => x.Contains(c)).First();
                    foreach (Country n in c.Neighbours) cluster.Add(n);
                }
            }

            // Merge clusters
            List<Country> inMultipleClusters = Map.Countries.Where(x => (Clusters.Where(l => l.Contains(x)).Count() > 1)).ToList();
            foreach(Country c in inMultipleClusters)
            {
                List<HashSet<Country>> ClustersToMerge = Clusters.Where(l => l.Contains(c)).ToList();
                if (ClustersToMerge.Count > 1)
                {
                    HashSet<Country> ClusterToKeep = ClustersToMerge[0];
                    List<HashSet<Country>> ClustersToRemove = ClustersToMerge.Skip(1).ToList();
                    foreach (HashSet<Country> toRemove in ClustersToRemove)
                    {
                        foreach (Country c2 in toRemove) ClusterToKeep.Add(c2);
                        Clusters.Remove(toRemove);
                    }
                }
            }

            // Connect clusters
            while(Clusters.Count > 1)
            {
                HashSet<Country> clusterToConnect = Clusters[0];
                List<OceanConnection> connections = new List<OceanConnection>();
                // Find nearest neighbours
                foreach (Country c1 in clusterToConnect) {
                    foreach (HashSet<Country> others in Clusters.Skip(1).ToList())
                    {
                        foreach (Country c2 in others)
                        {
                            foreach(Point p1 in c1.AreaPixels)
                            {
                                foreach(Point p2 in c2.AreaPixels)
                                {
                                    int distance = (int)(Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) + ((p2.Y - p1.Y) * (p2.Y - p1.Y))));
                                    if (connections.Count < islandConnections || distance < connections[0].Distance)
                                    {
                                        // Found new short connection
                                        OceanConnection newConnection = new OceanConnection(distance, c1, c2, p1, p2, others);

                                        // Check if not crossing land
                                        bool valid = true;
                                        int steps = newConnection.Distance / 4;
                                        for (int i = 0; i < steps; i++)
                                        {
                                            int targetX = (int)(newConnection.SourcePoint.X + ((newConnection.TargetPoint.X - newConnection.SourcePoint.X) / steps * i));
                                            int targetY = (int)(newConnection.SourcePoint.Y + ((newConnection.TargetPoint.Y - newConnection.SourcePoint.Y) / steps * i));
                                            if (!(Map.CountryMap[targetX, targetY] == MapPixelType.OCEAN || Map.CountryMap[targetX, targetY] == MapPixelType.BORDER || Map.CountryMap[targetX, targetY] == newConnection.SourceCountry.Id || Map.CountryMap[targetX, targetY] == newConnection.TargetCountry.Id))
                                            {
                                                valid = false;
                                            }
                                        }

                                        if (valid)
                                        {
                                            // Check if connection between those country exists
                                            if (connections.Where(x => c1 == x.SourceCountry && c2 == x.TargetCountry).Count() > 0)
                                            {
                                                OceanConnection oldCon = connections.Where(x => c1 == x.SourceCountry && c2 == x.TargetCountry).FirstOrDefault();
                                                if (distance < oldCon.Distance)
                                                {
                                                    connections.Remove(oldCon);
                                                    connections.Add(newConnection);
                                                }
                                            }
                                            else connections.Add(newConnection);
                                            connections = connections.OrderBy(x => x.Distance).Take(islandConnections).OrderByDescending(x => x.Distance).ToList();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Add neighbours
                foreach(OceanConnection con in connections)
                {
                    con.SourceCountry.AddNeighbour(con.TargetCountry);
                    con.TargetCountry.AddNeighbour(con.SourceCountry);
                    foreach(Country c in con.TargetCluster)
                    {
                        clusterToConnect.Add(c);
                    }
                    Clusters.Remove(con.TargetCluster);

                    // Draw connection to map
                    double startX = con.SourcePoint.X;
                    double startY = con.SourcePoint.Y;
                    double endX = con.TargetPoint.X;
                    double endY = con.TargetPoint.Y;

                    int circlesDrawn = 0;
                    int steps = con.Distance / 4;
                    for (int i = 0; i < steps; i++)
                    {
                        int targetX = (int)(startX + ((endX - startX) / steps * i));
                        int targetY = (int)(startY + ((endY - startY) / steps * i));
                        if (Map.CountryMap[targetX, targetY] == MapPixelType.OCEAN)
                        {
                            Map.GetWriteableBitmap().FillEllipseCentered(targetX, targetY, 1, 1, Color.FromArgb(255, 255, 0, 0));
                            circlesDrawn++;
                        }
                    }
                    if (circlesDrawn == 0) Map.GetWriteableBitmap().FillEllipseCentered((int)(startX + (endX - startX) / 2), (int)(startY + (endY - startY) / 2), 1, 1, Color.FromArgb(255, 255, 0, 0));
                }

                
                Map.SetMap(MapGenerator.ConvertWriteableBitmapToBitmapImage(Map.GetWriteableBitmap()), false);
                RefreshMap();
            }

        }

        private void ConnectCloseCountriesOverWater(int maxDistance)
        {
            List<OceanConnection> oceanConnections = new List<OceanConnection>();
            foreach(Country source in Map.Countries)
            {
                foreach(Country target in Map.Countries)
                {
                    foreach(Point sourcePoint in source.AreaPixels)
                    {
                        foreach(Point targetPoint in target.AreaPixels)
                        {
                            if(source != target && !source.Neighbours.Contains(target) && !target.Neighbours.Contains(source) && !WithinWaterConnectionCountryRange(source, target))
                            {
                                int distance = (int)(Math.Sqrt((targetPoint.X - sourcePoint.X) * (targetPoint.X - sourcePoint.X) + ((targetPoint.Y - sourcePoint.Y) * (targetPoint.Y - sourcePoint.Y))));
                                if (distance <= maxDistance)
                                {
                                    // Found new short connection
                                    OceanConnection newConnection = new OceanConnection(distance, source, target, sourcePoint, targetPoint);

                                    // Check if not crossing land
                                    bool valid = true;
                                    int steps = newConnection.Distance;
                                    for (int i = 0; i < steps; i++)
                                    {
                                        int targetX = (int)(newConnection.SourcePoint.X + ((newConnection.TargetPoint.X - newConnection.SourcePoint.X) / steps * i));
                                        int targetY = (int)(newConnection.SourcePoint.Y + ((newConnection.TargetPoint.Y - newConnection.SourcePoint.Y) / steps * i));
                                        if (!(Map.CountryMap[targetX, targetY] == MapPixelType.OCEAN || Map.CountryMap[targetX, targetY] == MapPixelType.BORDER || Map.CountryMap[targetX, targetY] == newConnection.SourceCountry.Id || Map.CountryMap[targetX, targetY] == newConnection.TargetCountry.Id))
                                        {
                                            valid = false;
                                        }
                                    }

                                    if (valid)
                                    {
                                        // Check if connection between those country exists
                                        if (oceanConnections.Where(x => source == x.SourceCountry && target == x.TargetCountry).Count() > 0)
                                        {
                                            OceanConnection oldCon = oceanConnections.Where(x => source == x.SourceCountry && target == x.TargetCountry).FirstOrDefault();
                                            if (distance < oldCon.Distance)
                                            {
                                                oceanConnections.Remove(oldCon);
                                                oceanConnections.Add(newConnection);
                                                newConnection.SourceCountry.AddNeighbour(newConnection.TargetCountry);
                                                newConnection.TargetCountry.AddNeighbour(newConnection.SourceCountry);
                                            }
                                        }
                                        else
                                        {
                                            oceanConnections.Add(newConnection);
                                            newConnection.SourceCountry.AddNeighbour(newConnection.TargetCountry);
                                            newConnection.TargetCountry.AddNeighbour(newConnection.SourceCountry);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach(OceanConnection con in oceanConnections)
            {
                // Draw connection to map
                double startX = con.SourcePoint.X;
                double startY = con.SourcePoint.Y;
                double endX = con.TargetPoint.X;
                double endY = con.TargetPoint.Y;

                int circlesDrawn = 0;
                int steps = con.Distance / 4;
                for (int i = 0; i < steps; i++)
                {
                    int targetX = (int)(startX + ((endX - startX) / steps * i));
                    int targetY = (int)(startY + ((endY - startY) / steps * i));
                    if (Map.CountryMap[targetX, targetY] == MapPixelType.OCEAN)
                    {
                        Map.GetWriteableBitmap().FillEllipseCentered(targetX, targetY, 1, 1, Color.FromArgb(255, 255, 0, 0));
                        circlesDrawn++;
                    }
                }
                if (circlesDrawn == 0) Map.GetWriteableBitmap().FillEllipseCentered((int)(startX + (endX - startX) / 2), (int)(startY + (endY - startY) / 2), 1, 1, Color.FromArgb(255, 255, 0, 0));
            }

            Map.SetMap(MapGenerator.ConvertWriteableBitmapToBitmapImage(Map.GetWriteableBitmap()), false);
            RefreshMap();
        }

        private bool WithinWaterConnectionCountryRange(Country c1, Country c2)
        {
            HashSet<Country> Cluster = new HashSet<Country>();
            Cluster.Add(c1);
            for(int i = 1; i < MinWaterConnectionCountryDistance; i++)
            {
                List<Country> toAdd = new List<Country>();
                foreach(Country c in Cluster)
                {
                    foreach(Country n in c.Neighbours)
                    {
                        if (n == c2) return true;
                        toAdd.Add(n);
                    }
                }
                foreach (Country a in toAdd) Cluster.Add(a);
            }
            return false;
        }

        private void SetSelectedBorders()
        {
            foreach(Country c in Map.Countries)
            {
                foreach(Point p in c.AreaPixels)
                {
                    if (Map.DistanceToNearestBorder[(int)p.X,(int)p.Y] <= SELECTED_WIDTH) c.BorderPixels.Add(new Point(p.X, p.Y));
                }
            }
        }

        private void SetCenters()
        {
            foreach(Country c in Map.Countries)
            {
                Point tempCenter = new Point(-1, -1);
                float furthestDistance = -1;
                foreach(Point p in c.AreaPixels)
                {
                    if (Map.DistanceToNearestBorder[(int)p.X, (int)p.Y] > furthestDistance)
                    {
                        tempCenter = new Point(p.X, p.Y);
                        furthestDistance = Map.DistanceToNearestBorder[(int)p.X, (int)p.Y];
                    }
                }
                c.Center = new Point(tempCenter.X, tempCenter.Y);
                c.MaxArmy = (int)(Math.Sqrt((furthestDistance / 2) * (furthestDistance / 2) + (furthestDistance / 2) * (furthestDistance / 2)));
            }
        }
        //-----------------------------------------------------------------END INIT------------------------------------------------------------

        public void MouseMove(Object sender, MouseEventArgs e)
        {
            if (State == GameState.Null) return;
            int x = (int)e.GetPosition(Map.GetMapImage()).X;
            int y = (int)e.GetPosition(Map.GetMapImage()).Y;
            if (CoordinatesOnMap(x, y)) {
                UIManager.SetCoordinates(x, y);
                UIManager.SetNearestBorder(Map.DistanceToNearestBorder[x, y]);
                if (Map.CountryMap[x, y] >= 0) UIManager.SetCountryInfo(Map.Countries[Map.CountryMap[x, y]]);
                else UIManager.SetCountryInfo(new Country(-1, Map.Black));
            }
        }

        public void MouseDown(Object sender, MouseButtonEventArgs e)
        {
            int x = (int)e.GetPosition(Map.GetMapImage()).X;
            int y = (int)e.GetPosition(Map.GetMapImage()).Y;
        }

        private void ResetGame()
        {
            IdReplacements.Clear();
            currentPlayer = 0;
            foreach (Country c in Map.Countries)
            {
                c.Selected = false;
                c.Player = null;
                c.Army = 0;
            }
            Players.Clear();
            ActionQueue.Clear();
            FindNeighboursQueue.Clear();
            RefreshMap();
        }

        public void StartGame(int numPlayers, int numCountries, int numArmy)
        {
            if ((State != GameState.ReadyToPlay) || numPlayers * numCountries > Map.Countries.Count || numArmy > 50) return;
            ResetGame();
            Random r = new Random();
            for (int i = 0; i < numPlayers; i++)
            {
                int ai = r.Next(5); 
                Players.Add(new Player(Players.Count, RandomColor(Players.Select(p => p.PrimaryColor).ToArray()), ai));
            }

            List<int> countryIds = new List<int>();
            List<int> playerCountryIds = new List<int>();
            for (int j = 0; j < Map.Countries.Count; j++) countryIds.Add(j);
            for (int i = 0; i < numPlayers; i++)
            {
                playerCountryIds.Clear();
                for (int j = 0; j < numCountries; j++)
                {
                    int id = countryIds[Random.Next(countryIds.Count)];
                    countryIds.Remove(id);
                    playerCountryIds.Add(id);
                    TakeCountry(Map.Countries[id], Players[i]);
                    DistributeArmy(Map.Countries[id]);
                }

                for(int j = 0; j < numArmy - numCountries; j++)
                {
                    DistributeArmy(Map.Countries[playerCountryIds[Random.Next(playerCountryIds.Count)]]);
                } 
            }
            SetState(GameState.Playing_Initialize);
            ActionQueue.Add(() => UIManager.SetupPlayerOrderGrid(Players, currentPlayer));
        }

        public void StopGame()
        {
            if (State != GameState.Playing_Idle) return;
            ResetGame();
            SetState(GameState.ReadyToPlay);
        }

        public void GenerateMap(int width, int height, int minCountrySize, int countriesPerOcean, int wcmcd, int wcmald, float countryAmountScale)
        {
            if (State != GameState.Null && State != GameState.ReadyToPlay && State != GameState.GeneratingMap && State != GameState.Initializing_FindCountries && State != GameState.Initializing_FindNeighbours && State != GameState.Initializing_FindDistancesToNearestBorder) return;
            Map = new Map();
            MinWaterConnectionCountryDistance = wcmcd;
            MaxWaterConnectionDistance = wcmald;
            initX = 0;
            initY = 0;
            Players.Clear();
            ActionQueue.Clear();
            FindNeighboursQueue.Clear();
            IdReplacements.Clear();
            currentPlayer = 0;
            Map.Countries.Clear();
            CountryMinSize = minCountrySize;
            CountriesPerOcean = countriesPerOcean;
            MapGenerator gen = new MapGenerator(width, height, Map, ActionQueue, countryAmountScale);
            ActionQueue.Add(() => gen.GenerateMap());
            UIManager.Init(Map.Width);
            SetState(GameState.GeneratingMap);
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

        public static Color RandomOceanColor()
        {
            Color toReturn;
            byte[] colorData = new byte[3];
            Random.NextBytes(colorData);
            toReturn = Color.FromArgb(255, (byte)(colorData[0] / 8), (byte)((colorData[2]+200)/2), (byte)(colorData[1] + 128 > 255 ? 255 : colorData[1] + 128));
            return toReturn;
        }

        public static Color BackgroundColor(int x, int y, int width, int height)
        {
            float cornerDistance = (float)(Math.Sqrt(width / 2 * width / 2 + height / 2 * height / 2));
            float pointDistance = (float)(Math.Sqrt((x - width / 2) * (x - width / 2) + (y - height / 2) * (y - height / 2)));

            float factor = pointDistance / cornerDistance;
            byte value = (byte)(255 - 150 * factor);
            return Color.FromArgb(255, value, value, value);
        }
    }
}
