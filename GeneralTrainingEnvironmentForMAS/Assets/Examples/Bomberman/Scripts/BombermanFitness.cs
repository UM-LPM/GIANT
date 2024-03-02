using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BombermanFitness : Fitness {

    public static Dictionary<string, int> FitnessValues = new Dictionary<string, int> {
        { "AgentMove", 1 },
        { "BombPlaced", 5 },
        { "BlockDestroyed", 2 },
        { "PowerUpCollected", 3 },
        { "AgentHitByBomb", -20 },
        { "AgentHitByOwnBomb", -30 },
        { "AgentHealthZero", -10 },
        { "BombHitAgent", 10 },
        { "AgentKilled", 20 },
        { "SurvivalBonus", 8 },
        { "LastSurvivalBonus", 10 }
    };

    public static string[] Keys = FitnessValues.Keys.ToArray();

    public enum FitnessKeys {
        AgentMove,
        BombPlaced,
        BlockDestroyed,
        PowerUpCollected,
        AgentHitByBomb,
        AgentHitByOwnBomb,
        AgentHealthZero,
        BombHitAgent,
        AgentKilled,
        SurvivalBonus,
        LastSurvivalBonus
    }
}