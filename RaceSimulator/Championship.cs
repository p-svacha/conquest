using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Excel = Microsoft.Office.Interop.Excel;

namespace RaceSimulator
{
    class Championship
    {
        public List<Driver> Drivers = new List<Driver>();
        public Dictionary<Driver, int> StarterGridDictionary = new Dictionary<Driver, int>();
        public int Id;
        public string Name;
        public Format Format;
        public int RacesDriven;
        public ChampionshipState State;
        public string Icon;

        public Excel._Worksheet ExcelWorksheet;
        public Excel.Range ExcelRange;

        public ScrollViewer StarterGridSV = new ScrollViewer();
        public ScrollViewer RacerGridSV = new ScrollViewer();
        public Grid StarterGrid = new Grid();
        public Grid RacerGrid = new Grid();

        private Random Random = new Random();
        private int currentDriver;

        private Label customLabel;

        private int CRASH_PROBABILITY = 16;
        private int GODRUN_PROBABILITY = 8;

        public Championship(int id, Excel._Worksheet ws, List<Driver> allDrivers)
        {
            Id = id;
            ExcelWorksheet = ws;
            ExcelRange = ws.UsedRange;
            StarterGrid.SetValue(Control.BackgroundProperty, new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)));
            StarterGridSV.SetValue(Grid.ColumnProperty, 0);
            RacerGrid.SetValue(Control.BackgroundProperty, new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE)));
            RacerGridSV.SetValue(Grid.ColumnProperty, 1);
            StarterGridSV.Content = StarterGrid;
            RacerGridSV.Content = RacerGrid;
            LoadData(allDrivers);
        }

        public Championship(List<Driver> drivers, Format format, Grid starterGrid, Grid racerGrid, Label customLabel)
        {
            Drivers = drivers;
            Format = format;
            StarterGrid = starterGrid;
            RacerGrid = racerGrid;
            this.customLabel = customLabel;
            LoadStarterGrid();
        }

        private void LoadStarterGrid()
        {
            StarterGridDictionary.Clear();
            Drivers = Drivers.OrderByDescending(x => x.SeasonPoints).ThenByDescending(x => x.Rating).ToList();
            for (int i = 0; i < Drivers.Count; i++)
            {
                StarterGridDictionary.Add(Drivers[i], i);
                StarterGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                DriverPanel dp = new DriverPanel(Drivers[i], false);
                dp.SetRank(i + 1);
                dp.SetRating(Drivers[i].SeasonPoints);
                dp.SetValue(Grid.RowProperty, i);
                Rectangle r;
                if (i < Format.NumGreen)
                {
                    r = new Rectangle { Fill = new SolidColorBrush(Color.FromRgb(0xCC, 0xEE, 0xCC)) };
                }
                else if (i < Drivers.Count - Format.NumRed)
                {
                    r = new Rectangle { Fill = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)) };
                }
                else
                {
                    r = new Rectangle { Fill = new SolidColorBrush(Color.FromRgb(0xEE, 0xCC, 0xCC)) };
                }
                r.SetValue(Grid.RowProperty, i);
                StarterGrid.Children.Add(r);
                StarterGrid.Children.Add(dp);
            }
            for (int i = Drivers.Count; i < 16; i++) StarterGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        }

        public void LoadData(List<Driver> allDrivers)
        {
            try
            {
                int rowCount = ExcelRange.Rows.Count;
                int colCount = ExcelRange.Columns.Count;
                RacesDriven = colCount - 2;

                int row = 1;
                Name = ExcelRange.Cells[row, 1].Value2.ToString();
                row++;
                row++;
                if (ExcelRange.Cells[row, 2].Value2 != null) Icon = ExcelRange.Cells[row, 2].Value2.ToString();
                row++;

                for (int i = row; i <= rowCount; i++)
                {
                    Driver d = allDrivers.First(x => x.Name == ExcelRange.Cells[i, 1].Value2.ToString());
                    d.SeasonPoints = (int)(ExcelRange.Cells[i, 2].Value2);
                    Drivers.Add(d);
                }

                Drivers = Drivers.OrderByDescending(x => x.SeasonPoints).ThenByDescending(x => x.Rating).ToList();
                for (int i = 0; i < Drivers.Count; i++)
                {
                    StarterGridDictionary.Add(Drivers[i], i);
                    StarterGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    DriverPanel dp = new DriverPanel(Drivers[i], false);
                    dp.SetRank(i + 1);
                    dp.SetRating(Drivers[i].SeasonPoints);
                    dp.SetValue(Grid.RowProperty, i);
                    if (Name.StartsWith("World League"))
                    {
                        Rectangle r;
                        if (i < Format.NumGreen)
                        {
                            r = new Rectangle { Fill = new SolidColorBrush(Color.FromRgb(0xCC, 0xEE, 0xCC)) };
                        }
                        else if (i < Drivers.Count - Format.NumRed)
                        {
                            r = new Rectangle { Fill = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)) };
                        }
                        else
                        {
                            r = new Rectangle { Fill = new SolidColorBrush(Color.FromRgb(0xEE, 0xCC, 0xCC)) };
                        }
                        r.SetValue(Grid.RowProperty, i);
                        StarterGrid.Children.Add(r);
                    }
                    StarterGrid.Children.Add(dp);
                }

                if (Drivers[0].SeasonPoints == 0)
                    State = ChampionshipState.Open;
                else if (Format.IsFinished(this))
                    State = ChampionshipState.Completed;
                else
                    State = ChampionshipState.Running;
                foreach (Driver d in Drivers) d.Championships.Add(this);
            }
            finally
            {
                Marshal.ReleaseComObject(ExcelWorksheet);
                Marshal.ReleaseComObject(ExcelRange);
            }
        }

        public void StartRace()
        {
            int crashes = 0;
            int godRuns = 0;
            foreach (Driver driver in Drivers)
            {
                driver.RaceTime = RandomNormal(driver.Rating, 200);
                int rng = Random.Next(100);
                if(rng < CRASH_PROBABILITY)
                {
                    crashes++;
                    driver.RaceTime = 0;
                }
                else if(rng < CRASH_PROBABILITY + GODRUN_PROBABILITY)
                {
                    godRuns++;
                    driver.RaceTime = RandomNormal(10000, 1000); 
                }
                RacerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }
            Console.WriteLine(crashes + " crashes!");
            Console.WriteLine(godRuns + " god runs!");
            customLabel.Content = crashes + " crashes | " + godRuns + " god runs";
            for (int i = Drivers.Count; i < 16; i++) RacerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            Drivers = Drivers.OrderBy(x => x.RaceTime).ToList();
        }

        //returns true when there are more drivers
        public bool NextDriver()
        {
            int bonus = 2; // a racer's rating will go up by this value per race IN AVERAGE 

            int rank = Drivers.Count - currentDriver;

            int expectedRankRating = Drivers.OrderByDescending(x => x.Rating).ToList()[rank - 1].Rating;
            int ratingChangeFromDiffDromExpectedRating = (expectedRankRating - Drivers[currentDriver].Rating) / 12;

            float seasonScaleFactor = Format.MaxSeasonRatingChange / (Drivers.Count - 1);
            int expectedSeasonRank = Drivers.OrderByDescending(x => x.SeasonPoints).ThenByDescending(x => x.Rating).ToList().IndexOf(Drivers[currentDriver]) + 1;
            int ratingChangeFromDiffFromExpectedSeasonRank = expectedSeasonRank - rank;
            ratingChangeFromDiffFromExpectedSeasonRank = (int)(ratingChangeFromDiffFromExpectedSeasonRank * seasonScaleFactor);


            int ratingChangeFromMood = Random.Next(11) - 5;

            int ratingChangeFromRank;
            if (Drivers.Count % 2 != 0) ratingChangeFromRank = (Drivers.Count + 1) / 2 - rank;
            else ratingChangeFromRank = Drivers.Count / 2 - rank < 0 ? Drivers.Count / 2 - rank : Drivers.Count / 2 - rank + 1;
            float rankScaleFactor;
            if (Drivers.Count % 2 != 0) rankScaleFactor = Format.MaxRankRatingChange / ((Drivers.Count + 1) / 2 - 1);
            else rankScaleFactor = Format.MaxRankRatingChange / (Drivers.Count / 2);
            ratingChangeFromRank = (int)(ratingChangeFromRank * rankScaleFactor);

            Drivers[currentDriver].RatingChange = ratingChangeFromDiffDromExpectedRating + ratingChangeFromDiffFromExpectedSeasonRank + ratingChangeFromMood + ratingChangeFromRank + bonus;
            Console.WriteLine(Drivers[currentDriver].Name + "'s rating changed from " + Drivers[currentDriver].Rating + " to " + (Drivers[currentDriver].Rating + Drivers[currentDriver].RatingChange) + " [" + (Drivers[currentDriver].RatingChange > 0 ? "+" + Drivers[currentDriver].RatingChange : Drivers[currentDriver].RatingChange + "") + "] (Race: " + ratingChangeFromRank + ", Rating: " + ratingChangeFromDiffDromExpectedRating + ", Season: " + ratingChangeFromDiffFromExpectedSeasonRank + ", Mood: " + ratingChangeFromMood + ", Bonus: " + bonus + ").");

            DriverPanel dp = new DriverPanel(Drivers[currentDriver], false);
            dp.SetRank(rank);
            dp.SetRating(Drivers[currentDriver].RaceTime);
            dp.SetRatingChange(Drivers[currentDriver].RatingChange);
            dp.SetValue(Grid.RowProperty, rank - 1);
            RacerGrid.Children.Add(dp);

            DriverPanel seasonPanel = (DriverPanel)StarterGrid.Children[StarterGridDictionary[Drivers[currentDriver]] * 2 + 1];
            seasonPanel.SetValue(Control.BackgroundProperty, new SolidColorBrush(Color.FromArgb(0x33, 0x00, 0x00, 0x00)));
            if (rank <= Format.Scoring.Length && Drivers[currentDriver].RaceTime > 0) seasonPanel.SetRatingChange(Format.Scoring[rank - 1]);

            currentDriver++;

            if (RacerGrid.Children.Count == Drivers.Count)
            {
                Driver highestGain = Drivers.OrderByDescending(x => x.RatingChange).First();
                Driver highestLoss = Drivers.OrderBy(x => x.RatingChange).First();
                Console.WriteLine("----------------------------------------------");
                Console.WriteLine("Total rating change: " + Drivers.Sum(x => x.RatingChange));
                Console.WriteLine("Highest Rating Gain: " + highestGain.Name + " (" + highestGain.RatingChange + ")");
                Console.WriteLine("Highest Rating Loss: " + highestLoss.Name + " (" + highestLoss.RatingChange + ")");
                Console.WriteLine("----------------------------------------------");
                return false;
            }
            return true;
        }

        public void EndRace(bool customRace)
        {
            Drivers = Drivers.OrderByDescending(x => x.RaceTime).ToList();
            if (customRace) RacesDriven++;

            Excel.Application xlApp = null;
            Excel._Worksheet driverRatingsWorksheet = null;
            Excel._Worksheet currentSeasonWorksheet = null;
            Excel.Range driverRatingsRange = null;
            Excel.Range currentSeasonRange = null;
            Excel.Workbook xlWorkbook = null;
            try
            {
                xlApp = new Excel.Application();
                xlWorkbook = xlApp.Workbooks.Open("C:\\Microsoft\\conquest\\RaceSimulator\\drivers.xlsx");
                driverRatingsWorksheet = xlWorkbook.Sheets[2];
                driverRatingsRange = driverRatingsWorksheet.UsedRange;
                if (!customRace)
                {
                    currentSeasonWorksheet = xlWorkbook.Sheets[Id];
                    currentSeasonRange = currentSeasonWorksheet.UsedRange;
                }

                for (int i = 0; i < Drivers.Count; i++)
                {
                    //Rating
                    int rowCount = driverRatingsRange.Rows.Count;
                    int colCount = driverRatingsRange.Columns.Count;

                    for (int j = 1; j <= rowCount; j++)
                    {
                        if (driverRatingsRange.Cells[j, 1].Value2.ToString() == Drivers[i].Name)
                        {
                            driverRatingsRange.Cells[j, colCount + 1] = Drivers[i].RatingChange;
                        }
                    }

                    //Season points
                    if (customRace)
                    {
                        if (i < Format.Scoring.Length && Drivers[i].RaceTime > 0) Drivers[i].SeasonPoints += Format.Scoring[i];
                        Drivers[i].Rating += Drivers[i].RatingChange;
                    }
                    else
                    {
                        rowCount = currentSeasonRange.Rows.Count;
                        colCount = currentSeasonRange.Columns.Count;

                        for (int j = 1; j <= rowCount; j++)
                        {
                            if (currentSeasonRange.Cells[j, 1].Value2.ToString() == Drivers[i].Name)
                            {
                                if (i < Format.Scoring.Length && Drivers[i].RaceTime > 0) currentSeasonRange.Cells[j, colCount + 1] = Format.Scoring[i];
                                else currentSeasonRange.Cells[j, colCount + 1] = 0;
                            }
                        }
                    }
                }
            }
            finally
            {
                //Unload
                Marshal.ReleaseComObject(driverRatingsRange);
                Marshal.ReleaseComObject(driverRatingsWorksheet);
                if (!customRace)
                {
                    Marshal.ReleaseComObject(currentSeasonRange);
                    Marshal.ReleaseComObject(currentSeasonWorksheet);
                }
                xlWorkbook.Close();
                Marshal.ReleaseComObject(xlWorkbook);
                xlApp.Quit();
                Marshal.ReleaseComObject(xlApp);
            }
            if(customRace)
            {
                if (Format.Id <= 2) customLabel.Content = RacesDriven + " races completed.";
                StarterGrid.Children.Clear();
                StarterGrid.RowDefinitions.Clear();
                RacerGrid.Children.Clear();
                RacerGrid.RowDefinitions.Clear();
                LoadStarterGrid();
            }
            currentDriver = 0;
        }

        private int RandomNormal(int mean, int stdDev)
        {
            double u1 = 1.0 - Random.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - Random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal =
                         mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return (int)randNormal;
        }
    }
}
