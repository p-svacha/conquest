using Conquest.MapClasses;
using Conquest.Model;
using Conquest.PlayerClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conquest.AI
{
    abstract class AIBase
    {
        protected static Random Random = new Random();
        protected Player Player;
        public string Tag;
        public abstract void StartTurn(GameModel model);
        public abstract bool NextTurn(GameModel model);
        public abstract void EndTurn(GameModel model);
    }
}
