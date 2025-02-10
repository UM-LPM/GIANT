using System.Collections.Generic;
using System.Linq;

namespace Problems.Moba_game
{
    public class Moba_gameFitness
    {
        public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
             { "SectorExploration", -0.1f },
             { "PowerUp_Pickup_Health", -0.1f },
             { "PowerUp_Pickup_Ammo", -0.10f },
             { "PowerUp_Pickup_Shield", -0.05f },
             { "MissilesFired", -0.1f },
             { "MissilesFiredAccuracy", -0.05f },
             { "SurvivalBonus", -0.05f },
             { "OpponentTrackingBonus", -0.05f },
             { "OpponentDestroyedBonus", -0.25f },
             { "DamageTakenPenalty", 0.15f },
             { "GoldHitsBonus", -0.05f },
             { "BaseHitsBonus", -0.1f },
             { "TeammateHitsPenalty", 0.1f },
             { "OwnBaseHitsPenalty", 0.1f },
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
            OpponentTrackingBonus,
            OpponentDestroyedBonus,
            DamageTakenPenalty,
            GoldHitsBonus,
            BaseHitsBonus,
            TeammateHitsPenalty,
            OwnBaseHitsPenalty
        }
    }
}