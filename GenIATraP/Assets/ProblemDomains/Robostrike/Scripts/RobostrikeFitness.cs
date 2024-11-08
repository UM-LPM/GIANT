using System.Collections.Generic;
using System.Linq;

namespace Problems.Robostrike
{
    public class RobostrikeFitness
    {
        public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
        { "MissileHitAgent", -10 },
        { "AgentHitByRocket", 10 },
        { "MissileMissedAgent", 1 },
        { "SurvivalBonus", -50 },
        { "LastSurvivalBonus", -30 },
        { "DeathPenalty", 100 },
        { "AgentDestroyedBonus", -100 },
        { "AgentExploredSector", -4f },
        { "AgentAimingOpponent", -1 },
        { "AgentFiredMissile", -2 },
        { "AgentNearWall", 1 },
        {"AgentPickedUpHealthBoxPowerUp", -5 },
        {"AgentPickedUpShieldBoxPowerUp", -5 },
        {"AgentPickedUpAmmoBoxPowerUp", -6 }
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