using RaceSimulator.CountrySelection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Excel = Microsoft.Office.Interop.Excel;

namespace RaceSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Driver> AllDrivers = new List<Driver>();
        private CountrySelector countrySelector;
        private List<Championship> Championships = new List<Championship>();
        private int currentChampionship = -1;
        private Excel.Workbook xlWorkbook;

        public MainWindow()
        {
            InitializeComponent();
            countrySelector = new CountrySelector(RegionSelector);
            LoadData();
            btnNextDriver.IsEnabled = false;
            btnEndRace.IsEnabled = false;
        }

        private void ResetData()
        {
            Championships.Clear();
            DriverRatingsGrid.Children.Clear();
            DriverRatingsGrid.RowDefinitions.Clear();
            AllDrivers.Clear();
            LoadData();
        }

        private void LoadData()
        {
            Excel.Application xlApp = null;
            Excel._Worksheet driverRatingsWorksheet = null;
            Excel.Range driverRatingsRange = null;
            try
            {
                xlApp = new Excel.Application();
                xlWorkbook = xlApp.Workbooks.Open("C:\\Microsoft\\conquest\\RaceSimulator\\drivers.xlsx");
                driverRatingsWorksheet = xlWorkbook.Sheets[2];
                driverRatingsRange = driverRatingsWorksheet.UsedRange;

                //Ratings
                int rowCount = driverRatingsRange.Rows.Count;

                for (int i = 1; i <= rowCount; i++)
                {
                    string name = driverRatingsRange.Cells[i, 1].Value2.ToString();
                    string countryString = driverRatingsRange.Cells[i, 2].Value2.ToString();
                    Country country = countrySelector.AllCountries.First(x => x.Name == countryString);
                    int rating = (int)(driverRatingsRange.Cells[i, 3].Value2);
                    AllDrivers.Add(new Driver(name, country, rating));
                }

                LoadChampionships();
                countrySelector.Drivers = AllDrivers;
                ReloadDriverRatings();
            }
            finally
            {
                //Unload
                Marshal.ReleaseComObject(driverRatingsRange);
                Marshal.ReleaseComObject(driverRatingsWorksheet);
                xlWorkbook.Close();
                Marshal.ReleaseComObject(xlWorkbook);
                xlApp.Quit();
                Marshal.ReleaseComObject(xlApp);
            }
        }

        private void ReloadDriverRatings()
        {
            DriverRatingsGrid.Children.Clear();
            DriverRatingsGrid.RowDefinitions.Clear();

            AllDrivers = AllDrivers.OrderByDescending(x => x.Rating).ToList();
            List<Driver> listedDrivers = AllDrivers;
            if (RegionSelector.SelectedIndex != 0) listedDrivers = AllDrivers.Where(x => x.Country.Region1 == (string)RegionSelector.SelectedItem).ToList();

                for (int i = 0; i < listedDrivers.Count; i++)
                {
                    DriverRatingsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    DriverPanel dp = new DriverPanel(listedDrivers[i], true);
                    dp.SetRank(i + 1);
                    dp.SetRating(listedDrivers[i].Rating);
                    dp.SetValue(Grid.RowProperty, i);
                    DriverRatingsGrid.Children.Add(dp);
                }
        }

        private void LoadChampionships()
        {
            for (int i = 3; i <= xlWorkbook.Sheets.Count; i++)
            {
                Championship c = new Championship(i, xlWorkbook.Sheets[i], AllDrivers);
                Championships.Add(c);
            }
            if (currentChampionship == -1)
            {
                currentChampionship = Championships.Count - 1;
                btnNextChampionship.IsEnabled = false;
            }
            if (Championships[currentChampionship].State > 1) btnStartRace.IsEnabled = false;
            ReloadChampionshipContainer();
        }

        
        private void btnRandomCountry_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine($"Random selected Country is {countrySelector.RandomUnusedCountry()}");
        }

        private void btnStartRace_Click(object sender, RoutedEventArgs e)
        {
            Championships[currentChampionship].StartRace();
            btnStartRace.IsEnabled = false;
            btnNextDriver.IsEnabled = true;
        }

        private void btnNextDriver_Click(object sender, RoutedEventArgs e)
        {
            if(!Championships[currentChampionship].NextDriver())
            {
                btnNextDriver.IsEnabled = false;
                btnEndRace.IsEnabled = true;
            }
        }

        private void btnEndRace_Click(object sender, RoutedEventArgs e)
        {
            Championships[currentChampionship].EndRace();
            btnEndRace.IsEnabled = false;
            btnStartRace.IsEnabled = true;
            ResetData();
        }

        private void btnPrevChampionship_Click(object sender, RoutedEventArgs e)
        {
            currentChampionship--;
            btnNextChampionship.IsEnabled = true;
            if (currentChampionship == 0) btnPrevChampionship.IsEnabled = false;
            ReloadChampionshipContainer();
        }

        private void btnNextChampionship_Click(object sender, RoutedEventArgs e)
        {
            currentChampionship++;
            btnPrevChampionship.IsEnabled = true;
            if (currentChampionship == Championships.Count - 1) btnNextChampionship.IsEnabled = false;
            ReloadChampionshipContainer();
        }

        private void ReloadChampionshipContainer()
        {
            ChampionshipContainer.Children.Clear();
            ChampionshipTitle.Content = Championships[currentChampionship].Name;
            RacesDriven.Content = "Standings after " + Championships[currentChampionship].RacesDriven + " races";
            switch(Championships[currentChampionship].Mode)
            {
                case 0:
                    ChampionshipMode.Content = "First to " + Championships[currentChampionship].ModeLimit;
                    break;
            }

            ChampionshipOpen.Visibility = Visibility.Hidden;
            ChampionshipStarted.Visibility = Visibility.Hidden;
            ChampionshipCompleted.Visibility = Visibility.Hidden;
            btnStartRace.IsEnabled = true;
            switch (Championships[currentChampionship].State)
            {
                case 0:
                    ChampionshipOpen.Visibility = Visibility.Visible;
                    break;
                case 1:
                    ChampionshipStarted.Visibility = Visibility.Visible;
                    break;
                case 2:
                    ChampionshipCompleted.Visibility = Visibility.Visible;
                    btnStartRace.IsEnabled = false;
                    break;
            }

            ChampionshipContainer.Children.Add(Championships[currentChampionship].StarterGridSV);
            ChampionshipContainer.Children.Add(Championships[currentChampionship].RacerGridSV);
        }

        private void RegionSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReloadDriverRatings();
        }
    }
}
