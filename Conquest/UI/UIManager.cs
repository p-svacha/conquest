using Conquest.MapClasses;
using Conquest.PlayerClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Conquest.UI
{
    class UIManager
    {
        private Label CoordinatesLabel;
        private Label NearestBorderLabel;
        private Grid PlayerOrder;
        private Grid InfoPanel;
        private Grid GraphCountries;
        private Grid GraphArmy;
        private Grid GraphDensity;

        public UIManager(Grid infoPanel, Label coordinatesLabel, Label nearestBorderLabel, Grid playerOrder, Grid graphCountries, Grid graphArmy, Grid graphDensity)
        {
            CoordinatesLabel = coordinatesLabel;
            NearestBorderLabel = nearestBorderLabel;
            InfoPanel = infoPanel;
            PlayerOrder = playerOrder;
            GraphCountries = graphCountries;
            GraphArmy = graphArmy;
            GraphDensity = graphDensity;
        }

        public void SetCountryInfo(Country c)
        {
            SetupCountryInfoGrid();
            if (c.Id != -1) {
                Label valTitle = InfoLabel(c.Id + "", 0, 0, 2);
                valTitle.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                valTitle.FontWeight = FontWeights.Bold;

                //neighbours
                Label lblNeighbours = InfoLabel("Neighbours", 1, 0);
                string neighbourContent = "";
                if (c.Neighbours.Count > 0)
                {
                    foreach (Country n in c.Neighbours) neighbourContent += n.Id + ", ";
                    neighbourContent = neighbourContent.Substring(0, neighbourContent.Length - 2);
                }
                Label valNeighbours = InfoLabel(neighbourContent, 1, 1);

                //army
                Label lblArmy = InfoLabel("Army", 2, 0);
                Label valArmy = InfoLabel(c.Army + "", 2, 1);
            }
        }

        public void SetCoordinates(int x, int y)
        {
            CoordinatesLabel.Content = x + " / " + y;
        }

        public void SetNearestBorder(int d)
        {
            NearestBorderLabel.Content = d;
        }

        private Label InfoLabel(string content, int row, int column, int columnSpan = 1)
        {
            Label l = new Label()
            {
                Content = content
            };
            l.SetValue(Grid.RowProperty, row);
            l.SetValue(Grid.ColumnProperty, column);
            l.SetValue(Grid.ColumnSpanProperty, columnSpan);

            InfoPanel.Children.Add(l);
            return l;
        }

        private void SetupCountryInfoGrid()
        {
            InfoPanel.Children.Clear();
            InfoPanel.ColumnDefinitions.Clear();
            InfoPanel.RowDefinitions.Clear();
            InfoPanel.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(1, GridUnitType.Star)
            });
            InfoPanel.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(1, GridUnitType.Star)
            });
            InfoPanel.RowDefinitions.Add(new RowDefinition());
            InfoPanel.RowDefinitions.Add(new RowDefinition());
            InfoPanel.RowDefinitions.Add(new RowDefinition());
        }

        public void RefreshGraphs(List<Player> players)
        {
            GraphCountries.Children.Clear();
            GraphCountries.ColumnDefinitions.Clear();
            GraphArmy.Children.Clear();
            GraphArmy.ColumnDefinitions.Clear();
            GraphDensity.Children.Clear();
            GraphDensity.ColumnDefinitions.Clear();
            for (int i = 0; i < players.Where(p => p.Alive).Count(); i++)
            {
                List<Player> orderedByCountries = players.Where(p => p.Alive).OrderByDescending(p => p.Countries.Count).ToList();
                GraphCountries.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(orderedByCountries[i].Countries.Count, GridUnitType.Star)
                });

                Rectangle rectCountries = new Rectangle()
                {
                    Fill = new SolidColorBrush(orderedByCountries[i].PrimaryColor),
                    IsHitTestVisible = false,
                };
                Label txtCountries = new Label()
                {
                    Content = orderedByCountries[i].Countries.Count,
                    Foreground = new SolidColorBrush(orderedByCountries[i].SecondaryColor)
                };
                rectCountries.SetValue(Grid.ColumnProperty, i);
                txtCountries.SetValue(Grid.ColumnProperty, i);
                GraphCountries.Children.Add(rectCountries);
                GraphCountries.Children.Add(txtCountries);

                List<Player> orderedByArmy = players.Where(p => p.Alive).OrderByDescending(p => p.Countries.Sum(c => c.Army)).ToList();
                GraphArmy.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(orderedByArmy[i].Countries.Sum(c => c.Army), GridUnitType.Star)
                });
                Rectangle rectArmy = new Rectangle()
                {
                    Fill = new SolidColorBrush(orderedByArmy[i].PrimaryColor),
                    IsHitTestVisible = false,
                };
                Label txtArmy = new Label()
                {
                    Content = orderedByArmy[i].Countries.Sum(c => c.Army),
                    Foreground = new SolidColorBrush(orderedByArmy.ToList()[i].SecondaryColor)
                };
                rectArmy.SetValue(Grid.ColumnProperty, i);
                txtArmy.SetValue(Grid.ColumnProperty, i);
                GraphArmy.Children.Add(rectArmy);
                GraphArmy.Children.Add(txtArmy);
            }
        }

        public void SetupPlayerOrderGrid(List<Player> players, int currentPlayer)
        {
            PlayerOrder.Children.Clear();
            PlayerOrder.ColumnDefinitions.Clear();
            PlayerOrder.RowDefinitions.Clear();
            PlayerOrder.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < players.Where(p => p.Alive).Count(); i++)
            {
                PlayerOrder.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });
                Rectangle rect = new Rectangle()
                {
                    Fill = new SolidColorBrush(players.Where(p => p.Alive).ToList()[i].PrimaryColor),
                    IsHitTestVisible = false,
                };
                rect.SetValue(Grid.ColumnProperty, i);
                rect.SetValue(Grid.RowProperty, 0);
                PlayerOrder.Children.Add(rect);

                if (currentPlayer == players.Where(p => p.Alive).ToList()[i].Id)
                {
                    Rectangle selectRect = new Rectangle()
                    {
                        Stroke = new SolidColorBrush(players.Where(p => p.Alive).ToList()[i].SecondaryColor),
                        IsHitTestVisible = false,
                        Height = PlayerOrder.Height,
                        StrokeThickness = 4
                    };
                    selectRect.SetValue(Grid.ColumnProperty, i);
                    selectRect.SetValue(Grid.RowProperty, 0);
                    PlayerOrder.Children.Add(selectRect);
                }

                Rectangle borderRect = new Rectangle()
                {
                    Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
                    StrokeThickness = 1,
                    Height = PlayerOrder.Height,
                    IsHitTestVisible = false
                };
                borderRect.SetValue(Grid.ColumnProperty, i);
                borderRect.SetValue(Grid.RowProperty, 0);
                PlayerOrder.Children.Add(borderRect);
            }
        }
    }


}
