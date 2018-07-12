using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RaceSimulator
{
    class DriverPanel : Grid
    {
        private Driver Driver;
        private Label RankLabel;
        private Label NameLabel;
        private Image CountryImage;
        private Label RatingLabel;
        private Label RatingChangeLabel;

        public DriverPanel(Driver driver, bool showChampionships) {
            Driver = driver;
            Height = 40;
            ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(50, GridUnitType.Pixel) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(180, GridUnitType.Pixel) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(70, GridUnitType.Pixel) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(50, GridUnitType.Pixel) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(50, GridUnitType.Pixel) });
            if(showChampionships)
            {
                int colCounter = 5;
                if (driver.Championships.Where(c => c.State == ChampionshipState.Open || c.State == ChampionshipState.Running).Count() == 0)
                {
                    Championship cs = driver.Championships.Last();
                    ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(40, GridUnitType.Pixel) });
                    Image csImage = new Image { Source = new BitmapImage(new Uri("C:\\Microsoft\\conquest\\RaceSimulator\\res\\icons\\" + cs.Icon + ".png")), Height = 32, VerticalAlignment = VerticalAlignment.Center };
                    csImage.SetValue(Grid.ColumnProperty, colCounter++);
                    Children.Add(csImage);
                    int rank = cs.Drivers.OrderByDescending(d => d.SeasonPoints).ToList().IndexOf(driver);
                    if(rank < cs.Format.NumGreen)
                    {
                        ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(40, GridUnitType.Pixel) });
                        Image promImage = new Image { Source = new BitmapImage(new Uri("C:\\Microsoft\\conquest\\RaceSimulator\\res\\icons\\promotion.png")), Height = 20, VerticalAlignment = VerticalAlignment.Center };
                        promImage.SetValue(Grid.ColumnProperty, colCounter - 1);
                        Children.Add(promImage);
                    }
                    else if(rank >= cs.Drivers.Count - cs.Format.NumRed)
                    {
                        ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(40, GridUnitType.Pixel) });
                        Image relImage = new Image { Source = new BitmapImage(new Uri("C:\\Microsoft\\conquest\\RaceSimulator\\res\\icons\\relegation.png")), Height = 20, VerticalAlignment = VerticalAlignment.Center };
                        relImage.SetValue(Grid.ColumnProperty, colCounter - 1);
                        Children.Add(relImage);
                    }
                }
                else
                {
                    foreach (Championship cs in driver.Championships.Where(c => c.State == ChampionshipState.Open || c.State == ChampionshipState.Running))
                    {
                        ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(40, GridUnitType.Pixel) });
                        Image csImage = new Image { Source = new BitmapImage(new Uri("C:\\Microsoft\\conquest\\RaceSimulator\\res\\icons\\" + cs.Icon + ".png")), Height = 32 };
                        csImage.SetValue(Grid.ColumnProperty, colCounter++);
                        Children.Add(csImage);
                    }
                }
            }
            

            RankLabel = new Label { VerticalAlignment = VerticalAlignment.Center };
            RankLabel.SetValue(Grid.ColumnProperty, 0);
            NameLabel = new Label { Content = driver.Name, VerticalAlignment = VerticalAlignment.Center };
            NameLabel.SetValue(Grid.ColumnProperty, 1);
            CountryImage = new Image { Source = new BitmapImage(new Uri("C:\\Microsoft\\conquest\\RaceSimulator\\res\\countries\\" + driver.Country.Name + ".png")), Height = 35, VerticalAlignment = VerticalAlignment.Center };
            CountryImage.SetValue(Grid.ColumnProperty, 2);
            RatingLabel = new Label { VerticalAlignment = VerticalAlignment.Center };
            RatingLabel.SetValue(Grid.ColumnProperty, 3);
            RatingChangeLabel = new Label { VerticalAlignment = VerticalAlignment.Center};
            RatingChangeLabel.SetValue(Grid.ColumnProperty, 4);

            Children.Add(RankLabel);
            Children.Add(NameLabel);
            Children.Add(CountryImage);
            Children.Add(RatingLabel);
            Children.Add(RatingChangeLabel);
        }

        public void SetRank(int rank)
        {
            RankLabel.Content = rank;
        }

        public void SetRating(int rating)
        {
            RatingLabel.Content = rating;
        }

        public void SetRatingChange(int ratingChange)
        {
            if(ratingChange > 0)
            {
                RatingChangeLabel.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x88, 0x00));
                RatingChangeLabel.Content = "+" + ratingChange;
            }
            else
            {
                RatingChangeLabel.Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x00, 0x00));
                RatingChangeLabel.Content = ratingChange;
            }
        }
    }
}
