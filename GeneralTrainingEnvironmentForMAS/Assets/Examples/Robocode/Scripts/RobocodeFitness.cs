using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RobocodeFitness: Fitness {

    public static Dictionary<string, int> FitnessValues = new Dictionary<string, int> {
        { "MissileHitAgent", 5 },
        { "AgentHitByRocket", -10 },
        { "MissileMissedAgent", -5 },
        { "SurvivalBonus", 50 },
        { "LastSurvivalBonus", 30 },
        { "DeathPenalty", -30 },
        { "AgentDestroyedBonus", 25 },
        { "AgentMovedBonus", 5 },
        { "AgentAimingOpponent", 2 }
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
        AgentAimingOpponent
    }
}