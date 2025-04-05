using System.Collections.Generic;
using System.Linq;

namespace Problems.PlanetConquest
{
    public class PlanetConquestFitness
    {
        public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
             { "SectorExploration", -0.1f },
             { "SurvivalBonus", -0.05f },
             { "LasersFired", -0.1f },
             { "LaserOpponentAccuracy", -0.05f },
             { "LaserOpponentBaseLaserAccuracy", -0.1f },
             { "OpponentDestroyedBonus", -0.25f },
             { "OpponentBaseDestroyedBonus", -0.15f },
             { "DamageTakenPenalty", 0.15f },
             { "LavaPlanetOrbitEnter", -0.1f },
             { "IcePlanetOrbitEnter", -0.1f },
             { "LavaPlanetOrbitCapture", -0.2f },
             { "IcePlanetOrbitCapture", -0.2f },
        };

        public static string[] Keys = FitnessValues.Keys.ToArray();

        public enum FitnessKeys
        {
            SectorExploration,
            SurvivalBonus,
            LasersFired,
            LaserOpponentAccuracy,
            LaserOpponentBaseLaserAccuracy,
            OpponentDestroyedBonus,
            OpponentBaseDestroyedBonus,
            DamageTakenPenalty,
            LavaPlanetOrbitEnter,
            IcePlanetOrbitEnter,
            LavaPlanetOrbitCapture,
            IcePlanetOrbitCapture,
        }
    }
}