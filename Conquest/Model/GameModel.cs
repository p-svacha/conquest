using Conquest.MapClasses;
using Conquest.PlayerClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Conquest.Model
{
    class GameModel
    {

        private List<Player> Players;
        private List<Country> Countries;
        private List<Continent> Continents;
        private Map Map;

        public GameModel()
        {
            Players = new List<Player>();
            Countries = new List<Country>();
            Continents = new List<Continent>();
            Map = new Map();
        }

        public void SetMap(BitmapImage image)
        {
            Map.SetMap(image);
        }

        public Image GetMapImage()
        {
            return Map.GetMapImage();
        }

        public Color GetMapPixel(int x, int y)
        {
            return Map.GetPixel(x, y);
        }

        public void SetMapPixel(int x, int y, Color c)
        {
            Map.SetPixel(x, y, c);
        }

        public void FloodFill(int x, int y, Color c)
        {
            if (Map.GetPixel(x, y).Equals(c)) return;
        }
    }
}
