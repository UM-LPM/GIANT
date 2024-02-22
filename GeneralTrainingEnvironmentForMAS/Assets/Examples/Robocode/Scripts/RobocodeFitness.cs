using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RobocodeFitness: Fitness {

    public static int MISSILE_HIT_AGENT = 5;
    public static int AGENT_HIT_BY_ROCKET = -10;
    public static int MISSILE_MISSED_AGENT = -5;
    public static int SURVIVAL_BONUS = 50;
    public static int LAST_SURVIVAL_BONUS = 30;
    public static int DEATH_PENALTY = -30;
    public static int AGENT_DESTROYED_BONUS = 25;
    public static int AGENT_MOVED_BONUS = 5;
    public static int AGENT_AIMING_OPPONENT = 2;
}