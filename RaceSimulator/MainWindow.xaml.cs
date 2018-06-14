using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Excel = Microsoft.Office.Interop.Excel;

namespace RaceSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Driver> Drivers = new List<Driver>();

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
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
                int currentSeason = xlWorkbook.Sheets.Count - 1;
                driverRatingsWorksheet = xlWorkbook.Sheets[1];
                currentSeasonWorksheet = xlWorkbook.Sheets[currentSeason + 1];
                driverRatingsRange = driverRatingsWorksheet.UsedRange;
                currentSeasonRange = currentSeasonWorksheet.UsedRange;

                //Ratings
                int rowCount = driverRatingsRange.Rows.Count;
                int colCount = driverRatingsRange.Columns.Count;

                for (int i = 1; i <= rowCount; i++)
                {
                    string name = driverRatingsRange.Cells[i, 1].Value2.ToString();
                    string country = driverRatingsRange.Cells[i, 2].Value2.ToString();
                    int rating = (int)(driverRatingsRange.Cells[i, 3].Value2);
                    Drivers.Add(new Driver(name, country, rating));
                }

                for (int i = 0; i < Drivers.Count; i++)
                {
                    DriverRatingsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    Grid driverRatingPanel = Drivers[i].DriverRatingPanel();
                    driverRatingPanel.SetValue(Grid.RowProperty, i);
                    DriverRatingsGrid.Children.Add(driverRatingPanel);
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
    }
}
