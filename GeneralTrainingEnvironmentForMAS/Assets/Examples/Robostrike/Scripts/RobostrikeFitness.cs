using System.Collections.Generic;
using System.Linq;

public class RobostrikeFitness: Fitness {

    public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
        { "MissileHitAgent", -5 },
        { "AgentHitByRocket", 5 },
        { "MissileMissedAgent", 5 },
        { "SurvivalBonus", -50 },
        { "LastSurvivalBonus", -30 },
        { "DeathPenalty", 20 },
        { "AgentDestroyedBonus", -25 },
        { "AgentMovedBonus", -2 },
        { "AgentAimingOpponent", -1 }, // TODO: Add event to RayHitObject to trigger event
        { "AgentFiredMissile", -1 },
        { "AgentNearWall", 1 },
        {"AgentPickedUpHealthBoxPowerUp", -5 },
        {"AgentPickedUpShieldBoxPowerUp", -5 },
        {"AgentPickedUpAmmoBoxPowerUp", -10 }
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
        AgentNearWall,
        AgentPickedUpHealthBoxPowerUp,
        AgentPickedUpShieldBoxPowerUp,
        AgentPickedUpAmmoBoxPowerUp
    }
}