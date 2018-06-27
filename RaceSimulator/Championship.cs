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
        public int Mode;
        public int ModeLimit;
        public int RacesDriven;
        public int State;
        public string Icon;
        public int numPromoted;
        public int numRelegated;
        public List<int> Scoring = new List<int>();

        public Excel._Worksheet ExcelWorksheet;
        public Excel.Range ExcelRange;

        public ScrollViewer StarterGridSV = new ScrollViewer();
        public ScrollViewer RacerGridSV = new ScrollViewer();
        public Grid StarterGrid = new Grid();
        public Grid RacerGrid = new Grid();

        private Random Random = new Random();
        private int currentDriver;

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
                Mode = (int)(ExcelRange.Cells[row, 1].Value2);
                ModeLimit = (int)(ExcelRange.Cells[row, 2].Value2);
                row++;
                string scoringString = ExcelRange.Cells[row, 1].Value2.ToString();
                string[] scorings = scoringString.Split(',');
                foreach(string s in scorings)
                {
                    Scoring.Add(int.Parse(s));
                }
                if (ExcelRange.Cells[row, 2].Value2 != null) Icon = ExcelRange.Cells[row, 2].Value2.ToString();
                row++;
                if (Name.StartsWith("World League"))
                {
                    string relString = scoringString = ExcelRange.Cells[row, 1].Value2.ToString();
                    string[] rels = relString.Split(',');
                    numPromoted = int.Parse(rels[0]);
                    numRelegated = int.Parse(rels[1]);
                    row++;
                }

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
                        if (i < numPromoted)
                        {
                            r = new Rectangle { Fill = new SolidColorBrush(Color.FromRgb(0xCC, 0xEE, 0xCC)) };
                        }
                        else if (i < Drivers.Count - numRelegated)
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
                {
                    State = 0;
                    foreach (Driver d in Drivers) d.ActiveChampionships.Add(this);
                }
                else if (Drivers[0].SeasonPoints < ModeLimit || Drivers[0].SeasonPoints == Drivers[1].SeasonPoints)
                {
                    State = 1;
                    foreach (Driver d in Drivers) d.ActiveChampionships.Add(this);
                }
                else State = 2;
            }
            finally
            {
                Marshal.ReleaseComObject(ExcelWorksheet);
                Marshal.ReleaseComObject(ExcelRange);
            }
        }

        public void StartRace()
        {
            foreach (Driver driver in Drivers)
            {
                driver.RaceTime = RandomNormal(driver.Rating, 150);
                if(Random.Next(100) < 20)
                {
                    int crashIntensity = Random.Next(3);
                    Console.WriteLine(driver.Name + " crashed! (Intensity " + (crashIntensity + 1) + ")");
                    driver.RaceTime /= (crashIntensity + 2);
                }
                RacerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }
            Drivers = Drivers.OrderBy(x => x.RaceTime).ToList();
        }

        //returns true when there are more drivers
        public bool NextDriver()
        {
            int bonus = 2; // a racer's rating will go up by this value per race IN AVERAGE 

            int rank = Drivers.Count - currentDriver;
            int expectedRatingRank = Drivers.OrderByDescending(x => x.Rating).ToList().IndexOf(Drivers[currentDriver]) + 1;
            int expectedRankRating = Drivers.OrderByDescending(x => x.Rating).ToList()[rank - 1].Rating;
            int ratingChangeFromDiffDromExpectedRating = (expectedRankRating - Drivers[currentDriver].Rating) / 12;
            //int ratingChangeFromDiffFromExpectedRatingRank = expectedRatingRank - rank;
            int expectedSeasonRank = Drivers.OrderByDescending(x => x.SeasonPoints).ThenByDescending(x => x.Rating).ToList().IndexOf(Drivers[currentDriver]) + 1;
            int ratingChangeFromDiffFromExpectedSeasonRank = expectedSeasonRank - rank;
            int ratingChangeFromMood = Random.Next(11) - 5;
            int ratingChangeFromRank = Drivers.Count / 2 - rank < 0 ? Drivers.Count / 2 - rank : Drivers.Count / 2 - rank + 1;
            ratingChangeFromRank *= 2;
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
            if (rank <= Scoring.Count) seasonPanel.SetRatingChange(Scoring[rank - 1]);

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

        public void EndRace()
        {
            Drivers = Drivers.OrderByDescending(x => x.RaceTime).ToList();

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
                currentSeasonWorksheet = xlWorkbook.Sheets[Id];
                driverRatingsRange = driverRatingsWorksheet.UsedRange;
                currentSeasonRange = currentSeasonWorksheet.UsedRange;

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
                    rowCount = currentSeasonRange.Rows.Count;
                    colCount = currentSeasonRange.Columns.Count;

                    for (int j = 1; j <= rowCount; j++)
                    {
                        if (currentSeasonRange.Cells[j, 1].Value2.ToString() == Drivers[i].Name)
                        {
                            if (i < Scoring.Count) currentSeasonRange.Cells[j, colCount + 1] = Scoring[i];
                            else currentSeasonRange.Cells[j, colCount + 1] = 0;
                        }
                    }
                }
            }
            finally
            {
                //Unload
                Marshal.ReleaseComObject(driverRatingsRange);
                Marshal.ReleaseComObject(driverRatingsWorksheet);
                Marshal.ReleaseComObject(currentSeasonRange);
                Marshal.ReleaseComObject(currentSeasonWorksheet);
                xlWorkbook.Close();
                Marshal.ReleaseComObject(xlWorkbook);
                xlApp.Quit();
                Marshal.ReleaseComObject(xlApp);
            }
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
