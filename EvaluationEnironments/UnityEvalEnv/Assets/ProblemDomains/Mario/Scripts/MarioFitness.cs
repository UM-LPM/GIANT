using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Problems.Mario
{
    public class MarioFitness
    {
        public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
            { "TimePenalty", 50f },
            { "DeathPenalty", 50f },
            { "EnemiesKilled", -100f },
            { "CoinsCollected", -100f },
            { "MysteryBlocksDestroyed", -50f },
            { "Distance", -500f },
        };

        public static string[] Keys = FitnessValues.Keys.ToArray();

        public enum FitnessKeys
        {
            TimePenalty,
            DeathPenalty,
            EnemiesKilled,
            CoinsCollected,
            MysteryBlocksDestroyed,
            Distance
        }
    }
}
