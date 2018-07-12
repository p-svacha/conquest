using RaceSimulator.CountrySelection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RaceSimulator
{
    class Driver
    {
        public string Name { get; }
        public Country Country { get; }
        public int Rating { get; set; }
        public int SeasonPoints { get; set; }
        public int RatingChange { get; set; }

        public int RaceTime { get; set; }
        public List<Championship> Championships = new List<Championship>();

        public Driver(string Name, Country Country, int Rating)
        {
            this.Name = Name;
            this.Country = Country;
            this.Rating = Rating;
        }
    }
}
