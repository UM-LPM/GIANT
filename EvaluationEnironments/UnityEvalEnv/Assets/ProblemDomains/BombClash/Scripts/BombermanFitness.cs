using System.Collections.Generic;
using System.Linq;

namespace Problems.BombClash
{
    public class BombermanFitness
    {
        public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
        { "SectorExploration", -20 },
        { "BombsPlaced", -30 },
        { "BlockDestroyed", -50 },
        { "PowerUpsCollected", -20 },
        { "BombsHitAgent", -100 },
        { "AgentsKilled", -500},
        { "AgentHitByBombs", 50 },
        { "AgentHitByOwnBombs", 50 },
        { "AgentDeath", 100 },
        { "SurvivalBonus", -100 },
        { "LastSurvivalBonus", -100 }
    };

        public static string[] Keys = FitnessValues.Keys.ToArray();

        public enum FitnessKeys
        {
            SectorExploration,
            BombsPlaced,
            BlockDestroyed,
            PowerUpsCollected,
            BombsHitAgent,
            AgentsKilled,
            AgentHitByBombs,
            AgentHitByOwnBombs,
            AgentDeath,
            SurvivalBonus,
            LastSurvivalBonus,
        }
    }
}