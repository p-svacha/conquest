using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace RaceSimulator
{
    class Driver
    {
        public string Name { get; }
        public string Country { get; }
        public int Rating { get; set; }
        public int SeasonPoints { get; set; }

        public int RaceTime { get; set; }

        public Driver(string Name, string Country, int Rating)
        {
            this.Name = Name;
            this.Country = Country;
            this.Rating = Rating;
        }

        public Grid DriverRatingPanel()
        {
            Grid grid = new Grid { Height = 40 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(50, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(180, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(70, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(100, GridUnitType.Pixel) });

            Label rankLabel = new Label { Name = "rankLabel", VerticalAlignment = VerticalAlignment.Center };
            rankLabel.SetValue(Grid.ColumnProperty, 0);
            Label nameLabel = new Label { Content = Name, VerticalAlignment = VerticalAlignment.Center };
            nameLabel.SetValue(Grid.ColumnProperty, 1);
            Image countryImage = new Image { Source = new BitmapImage(new Uri("C:\\Microsoft\\conquest\\RaceSimulator\\res\\countries\\" + Country + ".png")), Height = 35, VerticalAlignment = VerticalAlignment.Center };
            countryImage.SetValue(Grid.ColumnProperty, 2);
            Label ratingLabel = new Label { Content = Rating, Name = "ratingLabel", VerticalAlignment = VerticalAlignment.Center };
            ratingLabel.SetValue(Grid.ColumnProperty, 3);

            grid.Children.Add(rankLabel);
            grid.Children.Add(nameLabel);
            grid.Children.Add(countryImage);
            grid.Children.Add(ratingLabel);

            return grid;
        }
    }
}
