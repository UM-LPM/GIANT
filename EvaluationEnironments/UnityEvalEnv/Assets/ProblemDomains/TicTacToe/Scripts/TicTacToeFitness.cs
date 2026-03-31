using System.Collections.Generic;
using System.Linq;

namespace Problems.TicTacToe
{
    public class TicTacToeFitness
    {
        public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
            { "Win", -1000f },
            { "Draw", -500f },
            { "PlaceMarker", -20f },
            { "OpportunitiesCreated", -20f },
            { "OpponentBlocked", -20f },
            { "OptimalMove", -10f },
        };

        public static string[] Keys = FitnessValues.Keys.ToArray();

        public enum FitnessKeys
        {
            Win,
            Draw,
            PlaceMarker,
            OpportunitiesCreated,
            OpponentBlocked,
            OptimalMove,
        }
    }
}