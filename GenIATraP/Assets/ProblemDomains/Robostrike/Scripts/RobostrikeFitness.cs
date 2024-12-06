using System.Collections.Generic;
using System.Linq;

namespace Problems.Robostrike
{
    public class RobostrikeFitness
    {
        public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
             { "SectorExploration", -0.1f },
             { "PowerUp_Pickup_Health", -0.1f },
             { "PowerUp_Pickup_Ammo", -0.10f },
             { "PowerUp_Pickup_Shield", -0.05f },
             { "MissilesFired", -0.1f },
             { "MissilesFiredAccuracy", -0.1f },
             { "SurvivalBonus", -0.05f },
             { "OpponentDestroyedBonus", -0.3f },
             { "DamageTakenPenalty", 0.1f },

            { "AgentExploredSector", -4f },
            { "MissileHitAgent", -10 },
            { "AgentHitByRocket", 10 },
            { "MissileMissedAgent", 1 },
            { "LastSurvivalBonus", -30 },
            { "DeathPenalty", 100 },
            { "AgentDestroyedBonus", -100 },
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
            SectorExploration,
            PowerUp_Pickup_Health,
            PowerUp_Pickup_Ammo,
            PowerUp_Pickup_Shield,
            MissilesFired,
            MissilesFiredAccuracy,
            SurvivalBonus,
            OpponentDestroyedBonus,
            DamageTakenPenalty,

            AgentExploredSector,
            MissileHitAgent,
            AgentHitByRocket,
            MissileMissedAgent,
            LastSurvivalBonus,
            DeathPenalty,
            AgentDestroyedBonus,
            AgentAimingOpponent,
            AgentFiredMissile,
            AgentNearWall,
            AgentPickedUpHealthBoxPowerUp,
            AgentPickedUpShieldBoxPowerUp,
            AgentPickedUpAmmoBoxPowerUp
        }
    }
}