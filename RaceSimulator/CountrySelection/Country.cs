using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceSimulator.CountrySelection
{
    class Country
    {
        public string Name { get; set; }
        public string Region1 { get; set; }
        public string Region2 { get; set; }
        public int Population { get; set; }

        public Country(string name, string region1, string region2, int population)
        {
            this.Name = name;
            this.Region1 = region1;
            this.Region2 = region2;
            this.Population = population;
        }
    }
}
