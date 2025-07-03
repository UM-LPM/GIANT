using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Problems.BoxTact
{
    public class BoxTactFitness
    {
        public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
            { "TimePenalty", 25f },
            { "SectorExploration", -50f },
            { "BoxesMoved", -100f },
            { "BoxesMovedToTarget", -500f }
        };

        public static string[] Keys = FitnessValues.Keys.ToArray();

        public enum FitnessKeys
        {
            TimePenalty,
            SectorExploration,
            BoxesMoved,
            BoxesMovedToTarget
        }
    }
}
