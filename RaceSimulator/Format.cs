using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceSimulator
{
    class Format
    {
        public int Id;
        public string Name;
        public int[] Scoring;
        public int MaxRankRatingChange;
        public int MaxSeasonRatingChange;
        public int MinDrivers;
        public int MaxDrivers;
        public int NumGreen;
        public int NumRed;
        public Func<Championship, bool> IsFinished;

        public Format(int id, string name, int[] scoring, Func<Championship, bool> isFinished, int maxRankRatingChange, int maxSeasonRatingChange, int minDrivers = 0, int maxDrivers = 0, int numGreen = 0, int numRed = 0)
        {
            Id = id;
            Name = name;
            Scoring = scoring;
            IsFinished = isFinished;
            MaxRankRatingChange = maxRankRatingChange;
            MaxSeasonRatingChange = maxSeasonRatingChange;
            MinDrivers = minDrivers;
            MaxDrivers = maxDrivers;
            NumGreen = numGreen;
            NumRed = numRed;
        }
        private static List<Format> formats;
        public static List<Format> Formats
        {
            get
            {
                if(formats == null)
                {
                    formats = new List<Format>()
                    {
                        new Format(0, "Continental WC Qualifier", new int[] {25,18,15,12,10,8,6,4,2,1}, (cs) => {return cs.RacesDriven >= 12 && cs.Drivers.OrderByDescending(x => x.SeasonPoints).ToList()[3].SeasonPoints != cs.Drivers.OrderByDescending(x => x.SeasonPoints).ToList()[4].SeasonPoints; }, 20, 20, 4, 99, 4),
                        new Format(1, "WC Group Stage", new int[] {10,7,5,3,2,1}, (cs) => { return cs.RacesDriven >= 8 && cs.Drivers.OrderByDescending(x => x.SeasonPoints).ToList()[3].SeasonPoints != cs.Drivers.OrderByDescending(x => x.SeasonPoints).ToList()[4].SeasonPoints; }, 12, 8, 8, 8, 4, 4),
                        new Format(2, "WC K.O. Phase", new int[] {10,6,4,3}, (cs) => {return cs.Drivers.OrderByDescending(x => x.SeasonPoints).ToList()[1].SeasonPoints >= 50 && cs.Drivers.OrderByDescending(x => x.SeasonPoints).ToList()[2].SeasonPoints != cs.Drivers.OrderByDescending(x => x.SeasonPoints).ToList()[1].SeasonPoints; }, 8, 4, 4, 4, 2, 2),
                        new Format(3, "WC Finals", new int[] {10,6,4,3}, (cs) => {return cs.Drivers.OrderByDescending(x => x.SeasonPoints).ToList()[0].SeasonPoints >= 80 && cs.Drivers.OrderByDescending(x => x.SeasonPoints).ToList()[0].SeasonPoints != cs.Drivers.OrderByDescending(x => x.SeasonPoints).ToList()[1].SeasonPoints; }, 8, 4, 4, 4, 2, 2),
                    };
                }
                return formats;
            }
        }
    }
}
