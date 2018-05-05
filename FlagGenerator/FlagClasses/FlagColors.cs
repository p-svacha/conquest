using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FlagGeneration.FlagClasses
{
    class FlagColors
    {
        private static Random Random = new Random();

        private static Dictionary<Color, int> Colors;

        public static Color Red = Color.FromArgb(255, 255, 0, 0);
        public static Color White = Color.FromArgb(255, 255, 255, 255);
        public static Color Blue = Color.FromArgb(255, 0, 0, 255);
        public static Color Yellow = Color.FromArgb(255, 255, 255, 0);
        public static Color Green = Color.FromArgb(160, 0, 255, 0);
        public static Color Black = Color.FromArgb(255, 0, 0, 0);

        public static Color RandomColor()
        {
            if(Colors == null)
            {
                Colors = new Dictionary<Color, int>();
                Colors.Add(Red, 148);
                Colors.Add(White, 140);
                Colors.Add(Blue, 102);
                Colors.Add(Yellow, 89);
                Colors.Add(Green, 87);
                Colors.Add(Black, 59);
            }

            int sum = Colors.Sum(x => x.Value);
            int r = Random.Next(sum);
            int tempSum = 0;
            
            foreach(KeyValuePair<Color, int> kvp in Colors)
            {
                tempSum += kvp.Value;
                if (r < tempSum) return kvp.Key;
            }
            throw new Exception("Color chose failure");
        }
    }
}
