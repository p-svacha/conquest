using Conquest.MapClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Conquest.MapGeneration
{
    class OceanConnection
    {
        public int Distance;
        public Country SourceCountry;
        public Country TargetCountry;
        public Point SourcePoint;
        public Point TargetPoint;
        public HashSet<Country> TargetCluster;

        public OceanConnection(int distance, Country sourceCountry, Country targetCountry, Point sourcePoint, Point targetPoint, HashSet<Country> targetCluster)
        {
            Distance = distance;
            SourceCountry = sourceCountry;
            TargetCountry = targetCountry;
            SourcePoint = sourcePoint;
            TargetPoint = targetPoint;
            TargetCluster = targetCluster;
        }
    }
}
