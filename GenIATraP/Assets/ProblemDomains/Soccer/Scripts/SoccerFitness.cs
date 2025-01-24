using System.Collections.Generic;
using System.Linq;

namespace Problems.Soccer
{
    public class SoccerFitness
    {
        public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
             { "SectorExploration", -30f },
             { "GoalScored", -500f },
             { "GoalReceived", 250f },
             { "AutoGoals", 250f },
             { "PassesToOponentGoal", -50f },
             { "PassesToOwnGoal", -10f },
             { "Passes", -25f },
        };

        public static string[] Keys = FitnessValues.Keys.ToArray();

        public enum FitnessKeys
        {
            SectorExploration,
            GoalScored,
            GoalReceived,
            AutoGoals,
            PassToOponentGoal,
            PassToOwnGoal,
            Pass
        }
    }
}