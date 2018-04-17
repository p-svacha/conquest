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
    class MapSaver
    {
        public static void SaveMap(Map map, string path)
        {
            SaveMapFile(map, path);
            SaveMapImage(map, path);
        }

        private static void SaveMapFile(Map map, string path)
        {
            string fullPath = Path.Combine(path, map.Name.ToLower() + ".map");
            List<string> lines = new List<string>();

            // 0: Title
            lines.Add(map.Name);

            // 1: [Width,Height]
            lines.Add(map.Width + "," + map.Height);

            // 2: CountryMap [id,]
            string cm = "";
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    cm += map.CountryMap[x, y] + ",";
                }
            }
            lines.Add(cm.Substring(0, cm.Length - 1));

            // 3: #Countries
            lines.Add(map.Countries.Count + "");


            foreach (Country c in map.Countries)
            {
                // 4+3x: Country Details (0Name,1MaxArmy,2CenterX,3CenterY,4Size,5#Neighbours,7#BorderPixels)
                lines.Add(c.Name + "," + c.MaxArmy + "," + c.Center.X + "," + c.Center.Y + "," + c.Size + "," + c.Neighbours.Count + "," + c.BorderPixels.Count);

                // 5+3x: Country Neighbours [id,] 
                string cn = "";
                foreach (Country n in c.Neighbours) cn += n.Id + ",";
                lines.Add(cn.Substring(0, cn.Length - 1));

                // 6+3x: Country AreaPixels [x/y,]
                string cap = "";
                foreach (Point p in c.AreaPixels) cap += p.X + "/" + p.Y + ",";
                lines.Add(cap.Substring(0, cap.Length - 1));

                // 7+3x: Country BorderPixels [x/y,]
                string cbp = "";
                foreach (Point p in c.BorderPixels) cbp += p.X + "/" + p.Y + ",";
                lines.Add(cbp.Substring(0, cbp.Length - 1));
            }

            File.WriteAllLines(fullPath, lines.ToArray());
        }

        private static void SaveMapImage(Map map, string path)
        {
            string fullPath = Path.Combine(path, map.Name.ToLower() + ".png");
            using (FileStream stream5 = new FileStream(fullPath, FileMode.Create))
            {
                PngBitmapEncoder encoder5 = new PngBitmapEncoder();
                encoder5.Frames.Add(BitmapFrame.Create(map.GetWriteableBitmap()));
                encoder5.Save(stream5);
            }
        }
    }
}
