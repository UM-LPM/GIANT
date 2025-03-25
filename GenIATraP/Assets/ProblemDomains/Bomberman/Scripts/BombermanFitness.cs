using System.Collections.Generic;
using System.Linq;

namespace Problems.Bomberman
{
    public class BombermanFitness
    {
        public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
        { "SectorExploration", -20 },
        { "BombPlaced", -30 },
        { "BlockDestroyed", -50 },
        { "PowerUpCollected", -20 },
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
            BombPlaced,
            BlockDestroyed,
            PowerUpCollected,
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