using Conquest.MapClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Conquest.IO
{
    class MapLoader
    {

        public static Map LoadMap(string path, string mapName)
        {
            Map map = new Map();

            map.SetMap(new BitmapImage(new Uri(Path.Combine(path, mapName + ".png"), UriKind.Relative)));

            string[] lines = File.ReadAllLines(Path.Combine(path, mapName + ".map"));

            map.Name = lines[0];
            string[] dim = lines[1].Split(',');
            int width = int.Parse(dim[0]);
            int height = int.Parse(dim[1]);

            int counter = 0;
            string[] cm = lines[2].Split(',');
            for(int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    map.CountryMap[x, y] = int.Parse(cm[counter++]);
                }
            }

            for(int i = 0; i < int.Parse(lines[3]); i++)
            {
                Country c = new Country(i, Map.White);
                string[] details = lines[4 + 4 * i].Split(',');
                c.Name = details[0];
                c.MaxArmy = int.Parse(details[1]);
                c.Center = new Point(int.Parse(details[2]), int.Parse(details[3]));

                string[] area = lines[6 + 4 * i].Split(',');
                foreach(string ar in area)
                {
                    string[] a = ar.Split('/');
                    c.AreaPixels.Add(new Point(int.Parse(a[0]), int.Parse(a[1])));
                }

                string[] border = lines[7 + 4 * i].Split(',');
                foreach (string ar in border)
                {
                    string[] a = ar.Split('/');
                    c.AreaPixels.Add(new Point(int.Parse(a[0]), int.Parse(a[1])));
                }
                map.Countries.Add(c);
            }

            for (int i = 0; i < int.Parse(lines[3]); i++)
            {
                string[] neighbours = lines[5 + 4 * i].Split(',');
                foreach (string n in neighbours) map.Countries[i].Neighbours.Add(map.Countries[int.Parse(n)]);
            }

            return map;
        }
    }
}
