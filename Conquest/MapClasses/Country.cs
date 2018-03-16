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

        public void SetCenter()
        {
            int xSum = 0;
            int ySum = 0;
            foreach(Point p in AreaPixels)
            {
                xSum += (int)p.X;
                ySum += (int)p.Y;
            }
            Center = new Point(xSum / AreaPixels.Count, ySum / AreaPixels.Count);
        }
    }
}
