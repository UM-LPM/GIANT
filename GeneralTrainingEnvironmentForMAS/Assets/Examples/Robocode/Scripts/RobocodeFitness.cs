using System.Collections.Generic;
using System.Linq;

public class RobocodeFitness: Fitness {

    public static Dictionary<string, int> FitnessValues = new Dictionary<string, int> {
        { "MissileHitAgent", -5 },
        { "AgentHitByRocket", 5 },
        { "MissileMissedAgent", 5 },
        { "SurvivalBonus", -50 },
        { "LastSurvivalBonus", -30 },
        { "DeathPenalty", 20 },
        { "AgentDestroyedBonus", -25 },
        { "AgentMovedBonus", -2 },
        { "AgentAimingOpponent", -1 },
        { "AgentFiredMissile", -1 },
        { "AgentNearWall", 1 }
    };

    public static string[] Keys = FitnessValues.Keys.ToArray();

    public enum FitnessKeys {
        MissileHitAgent,
        AgentHitByRocket,
        MissileMissedAgent,
        SurvivalBonus,
        LastSurvivalBonus,
        DeathPenalty,
        AgentDestroyedBonus,
        AgentMovedBonus,
        AgentAimingOpponent,
        AgentFiredMissile,
        AgentNearWall
    }
}