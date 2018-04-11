using Conquest.AI;
using Conquest.MapClasses;
using Conquest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Conquest.PlayerClasses
{
    class Player
    {
        public int Id;
        public Color PrimaryColor;
        public Color SecondaryColor;
        public List<Country> Countries;
        public List<Continent> Continents;
        private AIBase AI;

        public bool Alive
        {
            get { return Countries.Count > 0; }
        }

        public Player(int id, Color primary)
        {
            Id = id;
            PrimaryColor = primary;
            SecondaryColor = GameModel.RandomColor(new Color[] { PrimaryColor }, 300);

            Countries = new List<Country>();
            Continents = new List<Continent>();
            AI = new AI_MultipleSafeAttacks(this);
        }

        public void StartTurn(GameModel model)
        {
            AI.StartTurn(model);
        }
        public bool DoTurn(GameModel model)
        {
            return AI.NextTurn(model);
        }
        public void EndTurn(GameModel model)
        {
            AI.EndTurn(model);
        }
    }
}
