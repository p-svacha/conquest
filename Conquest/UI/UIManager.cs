using Conquest.MapClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Conquest.UI
{
    class UIManager
    {
        private Label CoordinatesLabel;
        private StackPanel InfoPanel;

        public UIManager(StackPanel infoPanel, Label coordinatesLabel)
        {
            CoordinatesLabel = coordinatesLabel;
            InfoPanel = infoPanel;
        }

        public void SetCountryInfo(Country c)
        {
            InfoPanel.Children.Clear();

            Label title = new Label();
            title.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            string s = c.Id + " (";
            foreach (Country n in c.Neighbours) s += n.Id + ", ";
            s = s.Substring(0, s.Length - 2);
            s += ")";
            if (c.Id == -1) title.Content = "no man's land";
            else title.Content = s;
            InfoPanel.Children.Add(title);
        }

        public void SetCoordinates(int x, int y)
        {
            CoordinatesLabel.Content = x + " / " + y;
        }

    }
}
