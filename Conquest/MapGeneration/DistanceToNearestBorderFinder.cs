using Conquest.MapClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conquest.MapGeneration
{
    class DistanceToNearestBorderFinder
    {
        private Map Map;
        private List<Action> ActionQueue;

        public DistanceToNearestBorderFinder(Map map, List<Action> actionQueue)
        {
            this.Map = map;
            this.ActionQueue = actionQueue;
        }

        public void FindDistancesToNearestBorder(NearestBorderAlgorithm algorithm)
        {
            switch(algorithm) {
                case NearestBorderAlgorithm.BorderSpreadFourDirections:
                    for (int y = 0; y < Map.Height; y++)
                    {
                        for (int x = 0; x < Map.Width; x++)
                        {
                            if (x == 0 || x == Map.Width - 1 || y == 0 || y == Map.Height - 1 || Map.CountryMap[x, y] == MapPixelType.BORDER) Spread(x, y, 0, false);
                        }
                    }
                    break;

                case NearestBorderAlgorithm.BorderSpreadEightDirections:
                    for (int y = 0; y < Map.Height; y++)
                    {
                        for (int x = 0; x < Map.Width; x++)
                        {
                            if (x == 0 || x == Map.Width - 1 || y == 0 || y == Map.Height - 1 || Map.CountryMap[x, y] == MapPixelType.BORDER) Spread(x, y, 0, true);
                        }
                    }
                    break;
            }
        }

        private void Spread(int x, int y, float distance, bool eightDirections)
        {
            if (distance + 1 < Map.DistanceToNearestBorder[x - 1 < 0 ? 0 : x - 1, y])
            {
                ActionQueue.Add(() => Spread(x - 1 < 0 ? 0 : x - 1, y, distance + 1f, eightDirections));
                Map.DistanceToNearestBorder[x - 1 < 0 ? 0 : x - 1, y] = distance + 1f;
            }
            if (distance + 1 < Map.DistanceToNearestBorder[x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y])
            {
                ActionQueue.Add(() => Spread(x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y, distance + 1f, eightDirections));
                Map.DistanceToNearestBorder[x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y] = distance + 1f;
            }
            if (distance + 1 < Map.DistanceToNearestBorder[x, y - 1 < 0 ? 0 : y - 1])
            {
                ActionQueue.Add(() => Spread(x, y - 1 < 0 ? 0 : y - 1, distance + 1f, eightDirections));
                Map.DistanceToNearestBorder[x, y - 1 < 0 ? 0 : y - 1] = distance + 1f;
            }
            if (distance + 1 < Map.DistanceToNearestBorder[x, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1])
            {
                ActionQueue.Add(() => Spread(x, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1, distance + 1f, eightDirections));
                Map.DistanceToNearestBorder[x, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1] = distance + 1f;
            }

            if (eightDirections)
            {
                if (distance + 1.414f < Map.DistanceToNearestBorder[x - 1 < 0 ? 0 : x - 1, y - 1 < 0 ? 0 : y - 1])
                {
                    ActionQueue.Add(() => Spread(x - 1 < 0 ? 0 : x - 1, y - 1 < 0 ? 0 : y - 1, distance + 1.414f, eightDirections));
                    Map.DistanceToNearestBorder[x - 1 < 0 ? 0 : x - 1, y - 1 < 0 ? 0 : y - 1] = distance + 1.414f;
                }

                if (distance + 1.414f < Map.DistanceToNearestBorder[x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y - 1 < 0 ? 0 : y - 1])
                {
                    ActionQueue.Add(() => Spread(x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y - 1 < 0 ? 0 : y - 1, distance + 1.414f, eightDirections));
                    Map.DistanceToNearestBorder[x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y - 1 < 0 ? 0 : y - 1] = distance + 1.414f;
                }

                if (distance + 1.414f < Map.DistanceToNearestBorder[x - 1 < 0 ? 0 : x - 1, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1])
                {
                    ActionQueue.Add(() => Spread(x - 1 < 0 ? 0 : x - 1, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1, distance + 1.414f, eightDirections));
                    Map.DistanceToNearestBorder[x - 1 < 0 ? 0 : x - 1, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1] = distance + 1.414f;
                }

                if (distance + 1.414f < Map.DistanceToNearestBorder[x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1])
                {
                    ActionQueue.Add(() => Spread(x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1, distance + 1.414f, eightDirections));
                    Map.DistanceToNearestBorder[x + 1 > Map.Width - 1 ? Map.Width - 1 : x + 1, y + 1 > Map.Height - 1 ? Map.Height - 1 : y + 1] = distance + 1.414f;
                }
            }


        }
    }
}
