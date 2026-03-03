using System.Collections.Generic;
using System.Linq;

namespace Problems.Shooter
{
    public class ShooterFitness
    {
        public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
             { "SectorExploration", -5f },
             { "WeaponItemPickUp", -30f },
             { "HealthItemPickUp", -20f },
             { "ShieldItemPickUp", -20f },
             { "BulletsFired", -50f },
             { "BulletsFiredAccuracy", -200f },
             { "OpponentDefeatedBonus", -500f },
             { "SurvivalBonus", -50f },
             { "DamageTakenPenalty", 50f }
        };

        public static string[] Keys = FitnessValues.Keys.ToArray();

        public enum FitnessKeys
        {
            SectorExploration,
            WeaponItemPickUp,
            HealthItemPickUp,
            ShieldItemPickUp,
            BulletsFired,
            BulletsFiredAccuracy,
            OpponentDefeatedBonus,
            SurvivalBonus,
            DamageTakenPenalty
        }
    }
}