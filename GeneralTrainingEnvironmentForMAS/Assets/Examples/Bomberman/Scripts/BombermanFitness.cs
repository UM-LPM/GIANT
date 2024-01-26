using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BombermanFitness : Fitness {

    public static int AGENT_MOVE = 1;
    public static int BOMB_PLACED = 5;
    public static int BLOCK_DESTROYED = 2;
    public static int POWER_UP_COLECTED = 3;
    public static int AGENT_HIT_BY_BOMB = -20;
    public static int AGENT_HEALTH_ZERO = -10;
    public static int BOMB_HIT_AGENT = 10;
    public static int AGENT_KILLED = 20;
    public static int SURVIVAL_BONUS = 8;
    public static int LAST_SURVIVAL_BONUS = 10;

}