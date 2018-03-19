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

        public UIManager(Grid infoPanel, Label coordinatesLabel, Label nearestBorderLabel, Grid playerOrder)
        {
            CoordinatesLabel = coordinatesLabel;
            NearestBorderLabel = nearestBorderLabel;
            InfoPanel = infoPanel;
            PlayerOrder = playerOrder;
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

        public void SetupPlayerOrderGrid(List<Player> players, int currentPlayer)
        {
            PlayerOrder.Children.Clear();
            PlayerOrder.ColumnDefinitions.Clear();
            PlayerOrder.RowDefinitions.Clear();
            PlayerOrder.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < players.Count; i++)
            {
                PlayerOrder.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });
                TextBlock txt = new TextBlock()
                {
                    Text = "."
                };
                txt.SetValue(Grid.ColumnProperty, 0);
                txt.SetValue(Grid.RowProperty, 0);
                Rectangle rect = new Rectangle()
                {
                    Fill = new SolidColorBrush(players[i].PrimaryColor),
                    IsHitTestVisible = false,
                };
                rect.SetValue(Grid.ColumnProperty, i);
                rect.SetValue(Grid.RowProperty, 0);
                PlayerOrder.Children.Add(rect);

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

                if (currentPlayer == i)
                {
                    Rectangle selectRect = new Rectangle()
                    {
                        Stroke = new SolidColorBrush(players[i].SecondaryColor),
                        IsHitTestVisible = false,
                        Height = PlayerOrder.Height,
                        StrokeThickness = 4
                    };
                    selectRect.SetValue(Grid.ColumnProperty, i);
                    selectRect.SetValue(Grid.RowProperty, 0);
                    PlayerOrder.Children.Add(selectRect);
                }
                if(!players[i].Alive)
                {
                    Rectangle deadRect = new Rectangle()
                    {
                        Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
                        IsHitTestVisible = false,
                        Height = PlayerOrder.Height,
                        StrokeThickness = 6
                    };
                    deadRect.SetValue(Grid.ColumnProperty, i);
                    deadRect.SetValue(Grid.RowProperty, 0);
                    PlayerOrder.Children.Add(deadRect);
                }
            }
        }
    }


}
