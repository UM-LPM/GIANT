using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoccerFitness : Fitness
{
    public static int GOAL_SCORED = -30;
    public static int GOAL_RECEIVED = 20;
    public static int AUTO_GOAL = 30;

    public static int PASS_TO_OPONENT_GOAL = -2;
    public static int PASS_TO_OWN_GOAL = 1;
    public static int PASS = -1;

    public static int TOUCH_OTHER_AGENT = 2;
    public static int POSITION_CLOSE_TO_BALL = -1;
    public static int WALL_TOUCH = 2;
    public static int GOAL_TOUCH = 4;
    public static int MOVE = -1;
    public static int LOOK_BALL = -1;
}
