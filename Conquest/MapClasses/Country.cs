using Conquest.PlayerClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conquest.MapClasses
{
    class Country
    {
        public int Id;
        public string Name;
        public Continent Continent;
        public Player Player;

        public Country(int id)
        {
            this.Id = id;
        }
    }
}
