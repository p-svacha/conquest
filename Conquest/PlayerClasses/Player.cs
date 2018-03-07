using Conquest.MapClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conquest.PlayerClasses
{
    class Player
    {
        public List<Country> Countries;
        public List<Continent> Continents;

        public Player()
        {
            Countries = new List<Country>();
            Continents = new List<Continent>();
        }
    }
}
