using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoccerFitness : Fitness
{
    public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
        { "GoalScored", -30 },
        { "GoalReceived", 20 },
        { "AutoGoal", 35 },
        { "PassToOponentGoal", -2 },
        { "PassToOwnGoal", 1 },
        { "Pass", -1 },
        // Include ???
        { "TouchOtherAgent", 2 },
        { "PositionCloseToBall", -1 },
        { "WallTouch", 2 },
        { "GoalTouch", 4 },
        { "Move", -1 },
        { "LookBall", -1 }
    };

    public static string[] Keys = FitnessValues.Keys.ToArray();

    public enum FitnessKeys {
        GoalScored,
        GoalReceived,
        AutoGoal,
        PassToOponentGoal,
        PassToOwnGoal,
        Pass,
        TouchOtherAgent,
        PositionCloseToBall,
        WallTouch,
        GoalTouch,
        Move,
        LookBall
    }

}
