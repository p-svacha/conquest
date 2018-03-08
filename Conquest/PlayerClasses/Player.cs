using Conquest.MapClasses;
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
        public Color Color;
        public List<Country> Countries;
        public List<Continent> Continents;

        public Player(Color c)
        {
            Color = c;
            Countries = new List<Country>();
            Continents = new List<Continent>();
        }
    }
}
