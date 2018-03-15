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
        public List<Point> Pixels;
        public List<Country> Neighbours;
        public Continent Continent;
        public Player Player;

        public Country(int id)
        {
            Neighbours = new List<Country>();
            Pixels = new List<Point>();
            this.Id = id;
        }

        public void AddNeighbour(Country c)
        {
            if (!Neighbours.Contains(c)) Neighbours.Add(c);
        }

        public void SetCenter()
        {
            int xSum = 0;
            int ySum = 0;
            foreach(Point p in Pixels)
            {
                xSum += (int)p.X;
                ySum += (int)p.Y;
            }
            Center = new Point(xSum / Pixels.Count, ySum / Pixels.Count);
        }
    }
}
