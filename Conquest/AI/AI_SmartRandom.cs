﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Conquest.Model;
using Conquest.MapClasses;
using Conquest.PlayerClasses;

namespace Conquest.AI
{
    class AI_SmartRandom : AIBase
    {
        public AI_SmartRandom(Player player)
        {
            Player = player;
            Tag = "SR";
        }

        public override void StartTurn(GameModel model)
        {
            for (int i = 0; i < Player.Countries.Count; i++)
            {
                if (Random.Next(4) <= 2)
                {
                    List<Country> targets = Player.Countries.Where(c => c.Army < c.MaxArmy).Where(c => c.Neighbours.Where(n => n.Player != Player).Count() > 0).ToList();
                    if (targets.Count == 0) targets = Player.Countries.Where(c => c.Army < c.MaxArmy).ToList();
                    if (targets.Count == 0) return;
                    model.DistributeArmy(targets[Random.Next(targets.Count)]);
                }
                else
                {
                    List<Country> targets = Player.Countries.Where(c => c.Army < c.MaxArmy).ToList();
                    if (targets.Count == 0) return;
                    model.DistributeArmy(targets[Random.Next(targets.Count)]);
                }
            }
        }

        public override bool NextTurn(GameModel model)
        {
            if (Player.Countries.Where(c => c.Army > 0).Where(c => c.Neighbours.Where(n => n.Player != Player).Count() > 0).OrderByDescending(c => c.Army).Count() == 0) return false;
            foreach(Country source in Player.Countries.Where(c => c.Army > 0).Where(c => c.Neighbours.Where(n => n.Player != Player).Count() > 0).OrderByDescending(c => c.Army))
            {
                foreach(Country target in source.Neighbours.Where(c => c.Player != Player).OrderBy(c => c.Army))
                {
                    if(source.Army > target.Army)
                    {
                        model.Attack(source, target);
                        return Random.Next(10) < 7;
                    }
                }
            }
            return false;
        }

        public override void EndTurn(GameModel model)
        {
            // Move big army from inland country to border country
            foreach(Country c in Player.Countries.OrderByDescending(x => x.Army).ToList())
            {
                if(c.Neighbours.Where(x => x.Player != Player).Count() == 0)
                {
                    foreach(Country n in c.Neighbours)
                    {
                        if (n.Player == Player && n.Neighbours.OrderBy(x => x.Army).Where(x => x.Player != Player).Count() > 0)
                        {
                            model.MoveArmy(c, n, (int)(c.Army * 0.75));
                            return;
                        }
                    }
                }
            }
        }

    }
}
