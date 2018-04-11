using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Conquest.Model;
using Conquest.MapClasses;
using Conquest.PlayerClasses;

namespace Conquest.AI
{
    class AI_FullRandom : AIBase
    {
        public AI_FullRandom(Player player)
        {
            Player = player;
        }

        public override void StartTurn(GameModel model)
        {
            for (int i = 0; i < Player.Countries.Count; i++)
            {
                model.DistributeArmy(Player.Countries[Random.Next(Player.Countries.Count)]);
            }
        }

        public override bool NextTurn(GameModel model)
        {
            List<Country> sources = Player.Countries.Where(c => c.Army > 0).Where(c => c.Neighbours.Where(n => n.Player != Player).Count() > 0).ToList();
            if (sources.Count == 0) return false;
            Country source = sources[Random.Next(sources.Count)];

            List<Country> targets = source.Neighbours.Where(c => c.Player != Player).ToList();
            Country target = targets[Random.Next(targets.Count)];

            model.Attack(source, target);
            return false;
        }

        public override void EndTurn(GameModel model)
        {
        }

    }
}
