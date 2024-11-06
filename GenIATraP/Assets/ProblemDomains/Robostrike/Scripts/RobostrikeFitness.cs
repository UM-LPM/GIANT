using System.Collections.Generic;
using System.Linq;

namespace Problems.Robostrike
{
    public class RobostrikeFitness
    {
        public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
        { "MissileHitAgent", -5 },
        { "AgentHitByRocket", 5 },
        { "MissileMissedAgent", 5 },
        { "SurvivalBonus", -50 },
        { "LastSurvivalBonus", -30 },
        { "DeathPenalty", 20 },
        { "AgentDestroyedBonus", -25 },
        { "AgentExploredSector", -0.5f },
        { "AgentAimingOpponent", -1 },
        { "AgentFiredMissile", -1 },
        { "AgentNearWall", 1 },
        {"AgentPickedUpHealthBoxPowerUp", -5 },
        {"AgentPickedUpShieldBoxPowerUp", -5 },
        {"AgentPickedUpAmmoBoxPowerUp", -10 }
    };

        public static string[] Keys = FitnessValues.Keys.ToArray();

        public enum FitnessKeys
        {
            MissileHitAgent,
            AgentHitByRocket,
            MissileMissedAgent,
            SurvivalBonus,
            LastSurvivalBonus,
            DeathPenalty,
            AgentDestroyedBonus,
            AgentExploredSector,
            AgentAimingOpponent,
            AgentFiredMissile,
            AgentNearWall,
            AgentPickedUpHealthBoxPowerUp,
            AgentPickedUpShieldBoxPowerUp,
            AgentPickedUpAmmoBoxPowerUp
        }
    }
}