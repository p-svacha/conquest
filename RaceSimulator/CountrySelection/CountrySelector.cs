using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Excel = Microsoft.Office.Interop.Excel;

namespace RaceSimulator.CountrySelection
{
    class CountrySelector
    {
        private Random Random = new Random();
        private ComboBox RegionSelector;
        public List<Driver> Drivers;
        public List<Country> AllCountries = new List<Country>();
        private List<Country> UnusedCountries
        {
            get
            {
                return AllCountries.Except(Drivers.Select(x => x.Country).Distinct().ToList()).ToList();
            }
        }

        public CountrySelector(ComboBox regionSelector)
        {
            RegionSelector = regionSelector;
            List<Country> countries = new List<Country>();
            Excel.Application xlApp = null;
            Excel._Worksheet countriesWorksheet = null;
            Excel.Range countriesRange = null;
            Excel.Workbook xlWorkbook = null;
            try
            {
                xlApp = new Excel.Application();
                xlWorkbook = xlApp.Workbooks.Open("C:\\Microsoft\\conquest\\RaceSimulator\\drivers.xlsx");
                countriesWorksheet = xlWorkbook.Sheets[1];
                countriesRange = countriesWorksheet.UsedRange;

                //Ratings
                int rowCount = countriesRange.Rows.Count;
                int colCount = countriesRange.Columns.Count;

                for (int i = 1; i <= rowCount; i++)
                {
                    string country = countriesRange.Cells[i, 2].Value2.ToString();
                    country = country.Substring(1, country.Length - 1);
                    string region1 = countriesRange.Cells[i, 3].Value2.ToString();
                    string region2 = countriesRange.Cells[i, 4].Value2.ToString();
                    int population = (int)(countriesRange.Cells[i, 5].Value2);
                    population /= 100;
                    Country c = new Country(country, region1, region2, population);
                    AllCountries.Add(c);
                }
                List<string> regionSelectionList = AllCountries.Select(x => x.Region1).Distinct().ToList();
                regionSelectionList.Insert(0, "");
                regionSelector.ItemsSource = regionSelectionList;
                regionSelector.SelectedIndex = 0;
            }
            finally
            {
                //Unload
                Marshal.ReleaseComObject(countriesWorksheet);
                Marshal.ReleaseComObject(countriesRange);
                xlWorkbook.Close();
                Marshal.ReleaseComObject(xlWorkbook);
                xlApp.Quit();
                Marshal.ReleaseComObject(xlApp);
            }
        }

        public string RandomUnusedCountry()
        {
            List<Country> candidates = UnusedCountries;
            if ((string)RegionSelector.SelectedItem != "") candidates = UnusedCountries.Where(x => x.Region1 == (string)RegionSelector.SelectedItem).ToList();
            int totalPop = 0;
            for (int i = 0; i < candidates.Count; i++)
            {
                totalPop += candidates[i].Population;
            }

            int rng = Random.Next(totalPop);
            int tempPop = 0;
            int c = 0;
            while (true)
            {
                tempPop += candidates[c].Population;
                if (tempPop > rng) return candidates[c].Name;
                c++;
            }
        }
    }
}
