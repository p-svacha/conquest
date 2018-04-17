using Conquest.IO;
using Conquest.MapClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Conquest.Windows
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MapSelection : Window
    {
        public static string MAP_PATH = "../../Resources/GeneratedMaps";
        private const int MAPS_PER_ROW = 4;

        public MapSelection()
        {
            InitializeComponent();

            List<string> filesNames = new List<string>();
            string[] allFiles = Directory.GetFiles(MAP_PATH);
            
            foreach(string s in allFiles)
            {
                if (!filesNames.Contains(System.IO.Path.GetFileNameWithoutExtension(s))) filesNames.Add(System.IO.Path.GetFileNameWithoutExtension(s));
            }

            int noMaps = filesNames.Count;
            List<Map> maps = new List<Map>();

            foreach(string s in filesNames)
            {
                maps.Add(MapLoader.LoadMap(MAP_PATH, s));
            }

            for(int i = 0; i < MAPS_PER_ROW; i++)
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int i = 0; i < maps.Count / MAPS_PER_ROW + 1; i++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition());
            }
            int counter = 0;
            foreach(Map m in maps)
            {
                Grid g = new Grid();
                g.Width = 200;
                g.Height = 220;
                g.SetValue(Grid.RowProperty, counter / MAPS_PER_ROW);
                g.SetValue(Grid.ColumnProperty, counter % MAPS_PER_ROW);
                g.ColumnDefinitions.Add(new ColumnDefinition());
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(200) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(20) });

                Image i = m.GetMapImage();
                g.Children.Add(i);
                Label countryLabel = new Label();
                countryLabel.Content = "Countries: " + m.Countries.Count;
                countryLabel.SetValue(Grid.RowProperty, 1);
                g.Children.Add(countryLabel);

                MainGrid.Children.Add(g);
            }
        }
    }
}
