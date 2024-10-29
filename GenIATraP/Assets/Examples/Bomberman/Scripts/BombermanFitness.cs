using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BombermanFitness : Fitness {

    public static Dictionary<string, float> FitnessValues = new Dictionary<string, float> {
        { "AgentMove", -1 },
        { "BombPlaced", -5 },
        { "BlockDestroyed", -2 },
        { "PowerUpCollected", -5 },
        { "AgentHitByBomb", 20 },
        { "AgentHitByOwnBomb", 30 },
        { "AgentHealthZero", 100 },
        { "BombHitAgent", -10 },
        { "AgentKilled", -100},
        { "SurvivalBonus", -8 },
        { "LastSurvivalBonus", -10 }
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