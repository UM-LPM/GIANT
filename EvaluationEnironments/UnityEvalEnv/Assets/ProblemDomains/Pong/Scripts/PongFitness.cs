using System.Collections.Generic;
using System.Linq;

namespace Problems.Pong
{
    public class PongFitness
    {
        public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
            { "BallBounces", 0f },
            { "PointsScored", 0f },
        };

        public static string[] Keys = FitnessValues.Keys.ToArray();

        public enum FitnessKeys
        {
            BallBounces,
            PointsScored
        }
    }
}