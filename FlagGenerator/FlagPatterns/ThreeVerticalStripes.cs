using FlagGeneration.FlagClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FlagGeneration.FlagPatterns
{
    class ThreeVerticalStripes : MainPattern
    {
        public override void Apply(WriteableBitmap wb)
        {
            Color c1 = FlagColors.RandomColor();
            Color c2 = FlagColors.RandomColor();
            while (c2 == c1) c2 = FlagColors.RandomColor();
            Color c3 = FlagColors.RandomColor();
            if (Random.Next(4) == 0) c3 = c1;
            else
            {
                while(c3 == c1 || c3 == c2) c3 = FlagColors.RandomColor();
            }

            wb.FillRectangle(0, 0, (int)(wb.PixelWidth / 3), (int)(wb.PixelHeight), c1);
            wb.FillRectangle((int)(wb.PixelWidth / 3), 0, (int)(wb.PixelWidth / 3 * 2), (int)(wb.PixelHeight), c2);
            wb.FillRectangle((int)(wb.PixelWidth / 3 * 2), 0, (int)(wb.PixelWidth), (int)(wb.PixelHeight), c3);
        }
    }
}
