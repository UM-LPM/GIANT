using System.Collections.Generic;
using System.Linq;

namespace Problems.Soccer
{
    public class SoccerFitness
    {
        public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
             { "SectorExploration", -20f },
             { "GoalsScored", -500f },
             { "GoalsReceived", 250f },
             { "AutoGoals", 250f },
             { "PassesToOponentGoal", -50f },
             { "PassesToOwnGoal", -30f },
             { "Passes", -40f },
             { "AgentToBallDistance", -30f },
             { "BallToOpponentGoalDistance", -30f },
             { "TimeWithoutGoalBonus", - 50f},
             { "TimeLookingAtBall", - 50f},
        };

        public static string[] Keys = FitnessValues.Keys.ToArray();

        public enum FitnessKeys
        {
            SectorExploration,
            GoalsScored,
            GoalsReceived,
            AutoGoals,
            PassesToOponentGoal,
            PassesToOwnGoal,
            Passes,
            AgentToBallDistance,
            TimeWithoutGoalBonus,
            TimeLookingAtBall,
            BallToOpponentGoalDistance
        }
    }
}