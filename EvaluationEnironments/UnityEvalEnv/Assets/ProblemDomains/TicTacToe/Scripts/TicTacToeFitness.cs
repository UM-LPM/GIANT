using System.Collections.Generic;
using System.Linq;

namespace Problems.TicTacToe
{
    public class TicTacToeFitness
    {
        public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
            { "Win", -1000f },
            { "PlaceMarker", -20f },
        };

        public static string[] Keys = FitnessValues.Keys.ToArray();

        public enum FitnessKeys
        {
            Win,
            PlaceMarker,
        }
    }
}