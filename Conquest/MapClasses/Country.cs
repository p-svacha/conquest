using Conquest.PlayerClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Conquest.MapClasses
{
    class Country
    {
        public int Id;
        public Color Color;
        public string Name;
        public int Army;
        public int MaxArmy;
        public Point Center;
        public List<Point> AreaPixels;
        public List<Point> BorderPixels;
        public List<Country> Neighbours;
        public Continent Continent;
        public Player Player;
        public bool Selected;

        public Country(int id, Color color)
        {
            Neighbours = new List<Country>();
            AreaPixels = new List<Point>();
            BorderPixels = new List<Point>();
            this.Id = id;
            this.Color = color;
        }

        public int Size
        {
            get { return AreaPixels.Count; }
        }

        public void AddNeighbour(Country c)
        {
            if (!Neighbours.Contains(c)) Neighbours.Add(c);
        }
    }
}
