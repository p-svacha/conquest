using Conquest.MapClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Conquest.UI
{
    class UIManager
    {
        private Label CoordinatesLabel;
        private Grid InfoPanel;

        public UIManager(Grid infoPanel, Label coordinatesLabel)
        {
            CoordinatesLabel = coordinatesLabel;
            InfoPanel = infoPanel;
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
    }


}
