using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlagGeneration.Generator;
using FlagGeneration.FlagClasses;
using System.Windows.Controls;

namespace FlagGeneration.Model
{
    class FlagModel
    {
        Flag Flag;
        private List<Action> ActionQueue;

        public FlagModel()
        {
            ActionQueue = new List<Action>();
        }

        public void GenerateFlag()
        {
            Flag = new FlagGenerator().Generate(ActionQueue);
        }

        public Image GetFlagImage()
        {
            return Flag.Image;
        }
    }
}
