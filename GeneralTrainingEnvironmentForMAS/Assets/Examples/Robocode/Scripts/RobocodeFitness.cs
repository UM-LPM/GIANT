using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RobocodeFitness: Fitness {
    public enum ActionPerformed {
        MissileHitTank,
        TankHitByRocket,
        RamDamage,
        RocketHitObstacle,
        SurvivalBonus,
        LastSurvivalBonus
    }

    public static int MISSILE_HIT_TANK = 1;
    public static int TANK_HIT_BY_ROCKET = -2;
    public static int MISSILE_HIT_OBSTACLE = -2;
    public static int SURVIVAL_BONUS = 50;
    public static int LAST_SURVIVAL_BONUS = 20;
    public static int DEATH_PENALTY = -20;
    public static int TANK_DESTROYED_BONUS = 25;

    /*public void UpdateFitness(ActionPerformed action) {
        switch (action) {
            case ActionPerformed.MissileHitTank:
                UpdateFitness(MISSILE_HIT_TANK);
                break;
            case ActionPerformed.TankHitByRocket:
                UpdateFitness(TANK_HIT_BY_ROCKET);
                break;
            case ActionPerformed.RocketHitObstacle:
                UpdateFitness(MISSILE_HIT_OBSTACLE);
                break;
            case ActionPerformed.SurvivalBonus:
                UpdateFitness(SURVIVAL_BONUS);
                break;
            case ActionPerformed.LastSurvivalBonus:
                UpdateFitness(LAST_SURVIVAL_BONUS);
                break;
        }
    }*/
}