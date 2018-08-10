using RaceSimulator.CountrySelection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using Excel = Microsoft.Office.Interop.Excel;

namespace RaceSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Driver> AllDrivers = new List<Driver>();
        private List<Driver> CustomUnselectedDrivers = new List<Driver>();
        private List<Driver> CustomSelectedDrivers = new List<Driver>();
        private CountrySelector countrySelector;
        private List<Championship> Championships = new List<Championship>();
        private int currentChampionship = -1;
        private Excel.Workbook xlWorkbook;
        private List<UIElement> CustomCSHeaderOriginalButtons = new List<UIElement>();

        public MainWindow()
        {
            InitializeComponent();
            countrySelector = new CountrySelector(Region1Selector, Region2Selector, CustomRaceRegion1Selector);
            LoadData();
            ReloadDriverSelection();
            btnNextDriver.IsEnabled = false;
            btnEndRace.IsEnabled = false;
            foreach (UIElement elem in CustomCSHeader.Children) CustomCSHeaderOriginalButtons.Add(elem);
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

                //LoadChampionships();
                countrySelector.Drivers = AllDrivers;
                ReloadDriverRatings();

                CustomRaceFormatSelector.ItemsSource = Format.Formats.Select(x => x.Name);
                CustomRaceFormatSelector.SelectedIndex = 0;
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

        private void ReloadDriverRatings(bool region1changed = false)
        {
            DriverRatingsGrid.Children.Clear();
            DriverRatingsGrid.RowDefinitions.Clear();

            AllDrivers = AllDrivers.OrderByDescending(x => x.Rating).ToList();
            List<Driver> listedDrivers = AllDrivers;
            if (Region1Selector.SelectedIndex != 0)
            {
                listedDrivers = AllDrivers.Where(x => x.Country.Region1 == (string)Region1Selector.SelectedItem).ToList();

                if (region1changed)
                {
                    Region2Selector.IsEnabled = true;
                    List<string> region2SelectionList = countrySelector.AllCountries.Where(x => x.Region1 == (string)Region1Selector.SelectedItem).Select(x => x.Region2).Distinct().ToList();
                    region2SelectionList.Insert(0, "");
                    Region2Selector.ItemsSource = region2SelectionList;
                    Region2Selector.SelectedIndex = 0;
                }

                if (Region2Selector.SelectedIndex != 0)
                {
                    listedDrivers = AllDrivers.Where(x => x.Country.Region1 == (string)Region1Selector.SelectedItem).Where(x => x.Country.Region2 == (string)Region2Selector.SelectedItem).ToList();
                }
            }
            else Region2Selector.IsEnabled = false;

            for (int i = 0; i < listedDrivers.Count; i++)
            {
                DriverRatingsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                DriverPanel dp = new DriverPanel(listedDrivers[i], false);
                dp.SetRank(i + 1);
                dp.SetRating(listedDrivers[i].Rating);
                dp.SetValue(Grid.RowProperty, i);
                DriverRatingsGrid.Children.Add(dp);
            }
        }

        private void ReloadDriverSelection(bool region1changed = false)
        {
            CustomRaceLeftGrid.Children.Clear();
            CustomRaceLeftGrid.RowDefinitions.Clear();
            CustomRaceRightGrid.Children.Clear();
            CustomRaceRightGrid.RowDefinitions.Clear();

            CustomSelectedDrivers = CustomSelectedDrivers.OrderByDescending(x => x.Rating).ToList();
            CustomUnselectedDrivers = AllDrivers.Except(CustomSelectedDrivers).ToList();
            if (CustomRaceRegion1Selector.SelectedIndex != 0)
            {
                if (region1changed)
                {
                    CustomRaceRegion2Selector.IsEnabled = true;
                    List<string> region2SelectionList = countrySelector.AllCountries.Where(x => x.Region1 == (string)CustomRaceRegion1Selector.SelectedItem).Select(x => x.Region2).Distinct().ToList();
                    region2SelectionList.Insert(0, "");
                    CustomRaceRegion2Selector.ItemsSource = region2SelectionList;
                    CustomRaceRegion2Selector.SelectedIndex = 0;
                }

                CustomUnselectedDrivers = CustomUnselectedDrivers.Where(x => x.Country.Region1 == (string)CustomRaceRegion1Selector.SelectedItem).Except(CustomSelectedDrivers).ToList();
                if (CustomRaceRegion2Selector.SelectedIndex != 0)
                {
                    CustomUnselectedDrivers = CustomUnselectedDrivers.Where(x => x.Country.Region2 == (string)CustomRaceRegion2Selector.SelectedItem).Except(CustomSelectedDrivers).ToList();
                }
            }
            else CustomRaceRegion2Selector.IsEnabled = false;
            if (CustomSearchDriver.Text != "") CustomUnselectedDrivers = CustomUnselectedDrivers.Where(x => x.Name.ToLower().Contains(CustomSearchDriver.Text.ToLower()) || x.Country.Region2.ToLower().Contains(CustomSearchDriver.Text.ToLower()) || x.Country.Region1.ToLower().Contains(CustomSearchDriver.Text.ToLower()) || x.Country.Name.ToLower().Contains(CustomSearchDriver.Text.ToLower())).ToList();

            for (int i = 0; i < CustomUnselectedDrivers.Count; i++)
            {
                Driver current = CustomUnselectedDrivers[i];
                CustomRaceLeftGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                DriverPanel dp = new DriverPanel(current, false);
                dp.SetRank(i + 1);
                dp.SetRating(current.Rating);
                dp.SetValue(Grid.RowProperty, i);
                dp.PreviewMouseLeftButtonDown += (sender, e) => { CustomUnselectedDrivers.Remove(current); CustomSelectedDrivers.Add(current); ReloadDriverSelection(); };
                CustomRaceLeftGrid.Children.Add(dp);
            }
            for(int i = CustomUnselectedDrivers.Count; i < 16; i++) CustomRaceLeftGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            for (int i = 0; i < CustomSelectedDrivers.Count; i++)
            {
                Driver current = CustomSelectedDrivers[i];
                CustomRaceRightGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                DriverPanel dp = new DriverPanel(current, false);
                dp.SetRank(i + 1);
                dp.SetRating(current.Rating);
                dp.SetValue(Grid.RowProperty, i);
                dp.PreviewMouseLeftButtonDown += (sender, e) => { CustomSelectedDrivers.Remove(current); CustomUnselectedDrivers.Add(current); ReloadDriverSelection(); };
                CustomRaceRightGrid.Children.Add(dp);
            }
            for (int i = CustomSelectedDrivers.Count; i < 16; i++) CustomRaceRightGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
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
            if (Championships[currentChampionship].State == ChampionshipState.Completed) btnStartRace.IsEnabled = false;
            ReloadChampionshipContainer();
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
            Championships[currentChampionship].EndRace(false);
            Championships[currentChampionship].SaveRatingsToExcel(false);
            btnEndRace.IsEnabled = false;
            btnStartRace.IsEnabled = true;
            ResetData();
            ReloadDriverSelection();
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

            ChampionshipOpen.Visibility = Visibility.Hidden;
            ChampionshipStarted.Visibility = Visibility.Hidden;
            ChampionshipCompleted.Visibility = Visibility.Hidden;
            btnStartRace.IsEnabled = true;
            switch (Championships[currentChampionship].State)
            {
                case ChampionshipState.Open:
                    ChampionshipOpen.Visibility = Visibility.Visible;
                    break;
                case ChampionshipState.Running:
                    ChampionshipStarted.Visibility = Visibility.Visible;
                    break;
                case ChampionshipState.Completed:
                    ChampionshipCompleted.Visibility = Visibility.Visible;
                    btnStartRace.IsEnabled = false;
                    break;
            }

            ChampionshipContainer.Children.Add(Championships[currentChampionship].StarterGridSV);
            ChampionshipContainer.Children.Add(Championships[currentChampionship].RacerGridSV);
        }

        private void btnRandomCountry_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine($"Random selected Country is {countrySelector.RandomCountry(true)}");
        }

        private void btnRandomAllCountry_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine($"Random selected Country is {countrySelector.RandomCountry(false)}");
        }

        private void btnAddAll_Click(object sender, RoutedEventArgs e)
        {
            CustomSelectedDrivers.AddRange(CustomUnselectedDrivers);
            ReloadDriverSelection();
        }

        private void btnStartCustomRace_Click(object sender, RoutedEventArgs e)
        {
            Format format = Format.Formats.First(x => x.Id == CustomRaceFormatSelector.SelectedIndex);
            if (CustomSelectedDrivers.Count >= format.MinDrivers && CustomSelectedDrivers.Count <= format.MaxDrivers)
            {
                Label CustomCSLabel = new Label { VerticalAlignment = VerticalAlignment.Center };
                CustomRaceLeftGrid.Children.Clear();
                CustomRaceLeftGrid.RowDefinitions.Clear();
                CustomRaceRightGrid.Children.Clear();
                CustomRaceRightGrid.RowDefinitions.Clear();
                Championship cs = new Championship(CustomSelectedDrivers, format, CustomRaceLeftGrid, CustomRaceRightGrid, CustomCSLabel);
                CustomCSHeader.Children.Clear();
                CustomCSHeader.ColumnDefinitions.Clear();
                CustomCSHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                CustomCSHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                CustomCSHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                CustomCSHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                CustomCSHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                CustomCSHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                CustomCSHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                Button CustomCSStartRace = new Button { Content = "Start Race" };
                Button CustomCSNextDriver = new Button { Content = "Next Driver" };
                Button CustomCSFinishRace = new Button { Content = "Finish Race" };
                Button CustomCSFinishCS = new Button { Content = "Finish Championship" };
                Button CustomCSEndRace = new Button { Content = "End Race" };
                Button CustomCSBackToDriverSelection = new Button { Content = "Back" };

                CustomCSLabel.SetValue(Grid.ColumnProperty, 0);

                CustomCSStartRace.SetValue(Grid.ColumnProperty, 1);
                CustomCSStartRace.Click += (sender2, e2) => { cs.StartRace(); CustomCSStartRace.IsEnabled = false; CustomCSNextDriver.IsEnabled = true; CustomCSFinishRace.IsEnabled = true; };

                CustomCSNextDriver.SetValue(Grid.ColumnProperty, 2);
                CustomCSNextDriver.Click += (sender2, e2) => { if (!cs.NextDriver()) { CustomCSNextDriver.IsEnabled = false; CustomCSFinishRace.IsEnabled = false; CustomCSEndRace.IsEnabled = true; }; };
                CustomCSNextDriver.IsEnabled = false;

                CustomCSEndRace.SetValue(Grid.ColumnProperty, 3);
                CustomCSEndRace.Click += (sender2, e2) => 
                {
                    cs.EndRace(true);
                    cs.SaveRatingsToExcel(true);
                    CustomCSEndRace.IsEnabled = false;
                    if (!cs.Format.IsFinished(cs)) CustomCSStartRace.IsEnabled = true;
                    ResetData();
                };
                CustomCSEndRace.IsEnabled = false;

                CustomCSFinishRace.SetValue(Grid.ColumnProperty, 4);
                CustomCSFinishRace.Click += (sender2, e2) =>
                {
                    while (cs.NextDriver()) ;
                    CustomCSFinishRace.IsEnabled = false;
                    CustomCSNextDriver.IsEnabled = false;
                    CustomCSEndRace.IsEnabled = true;
                };
                CustomCSFinishRace.IsEnabled = false;

                CustomCSFinishCS.SetValue(Grid.ColumnProperty, 5);
                CustomCSFinishCS.Click += (sender2, e2) =>
                {
                    while(!cs.Format.IsFinished(cs))
                    {
                        cs.StartRace();
                        while (cs.NextDriver()) ;
                        cs.EndRace(true);
                    }
                    cs.SaveRatingsToExcel(true);
                    ResetData();
                    CustomCSFinishCS.IsEnabled = false;
                    CustomCSStartRace.IsEnabled = false;
                    CustomCSFinishRace.IsEnabled = false;
                    CustomCSEndRace.IsEnabled = false;
                    CustomCSNextDriver.IsEnabled = false;
                };

                CustomCSBackToDriverSelection.SetValue(Grid.ColumnProperty, 6);
                CustomCSBackToDriverSelection.Click += (sender2, e2) =>
                {
                    CustomCSHeader.Children.Clear();
                    foreach(UIElement elem in CustomCSHeaderOriginalButtons)
                    {
                        CustomCSHeader.Children.Add(elem);
                    }
                    CustomSelectedDrivers.Clear();
                    ReloadDriverSelection();
                };

                CustomCSHeader.Children.Add(CustomCSLabel);
                CustomCSHeader.Children.Add(CustomCSStartRace);
                CustomCSHeader.Children.Add(CustomCSNextDriver);
                CustomCSHeader.Children.Add(CustomCSEndRace);
                CustomCSHeader.Children.Add(CustomCSFinishRace);
                CustomCSHeader.Children.Add(CustomCSFinishCS);
                CustomCSHeader.Children.Add(CustomCSBackToDriverSelection);
            }
            else Console.WriteLine("Too many or too few drivers selected.");
        }

        private void CustomSearchDriver_TextChanged(object sender, TextChangedEventArgs e)
        {
            ReloadDriverSelection();
        }

        private void CustomRaceRegion1Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReloadDriverSelection(true);
        }

        private void CustomRaceRegion2Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReloadDriverSelection();
        }

        private void Region1Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReloadDriverRatings(true);
        }

        private void Region2Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReloadDriverRatings();
        }
    }
}
