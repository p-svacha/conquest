using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FlagGeneration.FlagPatterns
{
    class MainPattern
    {
        public virtual void Apply(WriteableBitmap wb) { }

        protected static Random Random = new Random();
        private static Dictionary<MainPattern, int> Patterns;

        public static MainPattern RandomMainPattern()
        {
            if(Patterns == null)
            {
                Patterns = new Dictionary<MainPattern, int>();
                Patterns.Add(new ThreeHorizontalStripes(), 100);
                Patterns.Add(new ThreeVerticalStripes(), 100);
            }

            int sum = Patterns.Sum(x => x.Value);
            int r = Random.Next(sum);
            int tempSum = 0;

            foreach (KeyValuePair<MainPattern, int> kvp in Patterns)
            {
                tempSum += kvp.Value;
                if (r < tempSum) return kvp.Key;
            }
            throw new Exception("Pattern chose failure");
        }
    }
}
