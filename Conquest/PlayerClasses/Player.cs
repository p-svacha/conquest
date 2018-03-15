using Conquest.MapClasses;
using Conquest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Conquest.PlayerClasses
{
    class Player
    {
        public Color PrimaryColor;
        public Color SecondaryColor;
        public List<Country> Countries;
        public List<Continent> Continents;

        public Player(Color primary)
        {
            PrimaryColor = primary;
            SecondaryColor = GameModel.RandomColor(new Color[] { PrimaryColor }, 300);

            Countries = new List<Country>();
            Continents = new List<Continent>();
        }
    }
}
