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
            { "BallsThrownAtOpponent", -50 },
            { "BallsIntercepted", -200f },
            { "OpponentsHit", -500f },
            { "SurvivalBonus", -30f },
            { "AgentToBallDistance", -30f }
        };

        public static string[] Keys = FitnessValues.Keys.ToArray();

        public enum FitnessKeys
        {
            SectorExploration,
            BallsPickedUp,
            BallsThrown,
            BallsThrownAtOpponent,
            BallsIntercepted,
            OpponentsHit,
            SurvivalBonus,
            AgentToBallDistance
        }
    }
}
