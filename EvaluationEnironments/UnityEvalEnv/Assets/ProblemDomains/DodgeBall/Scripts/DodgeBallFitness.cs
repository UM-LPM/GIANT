using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Problems.DodgeBall
{
    public class DodgeBallFitness
    {
        public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
            { "SectorExploration", -30f },
            { "BallsPickedUp", -100f },
            { "BallsThrown", -100f },
            { "BallsIntercepted", -200f },
            { "OpponentsHit", -500f },
            { "BallsHitBy", 100f },
        };

        public static string[] Keys = FitnessValues.Keys.ToArray();

        public enum FitnessKeys
        {
            SectorExploration,
            BallsPickedUp,
            BallsThrown,
            BallsIntercepted,
            BallsHit,
            BallsHitBy
        }
    }
}
