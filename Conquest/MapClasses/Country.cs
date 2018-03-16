using Conquest.PlayerClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Conquest.MapClasses
{
    class Country
    {
        public int Id;
        public string Name;
        public int Army;
        public Point Center;
        public List<Point> AreaPixels;
        public List<Point> BorderPixels;
        public List<Country> Neighbours;
        public Continent Continent;
        public Player Player;
        public bool Selected;

        public Country(int id)
        {
            Neighbours = new List<Country>();
            AreaPixels = new List<Point>();
            BorderPixels = new List<Point>();
            this.Id = id;
        }

        public void AddNeighbour(Country c)
        {
            if (!Neighbours.Contains(c)) Neighbours.Add(c);
        }
    }
}
