using System.Collections.Generic;
using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;

public enum HealthLevel
{
    Low,
    Medium,
    High,

}

public class HealthLevelBellow : ConditionNode
{
    public HealthLevel healthLevel;
    // TODO: Support for raw value instead of enum ??? (public int healthLevel;)

    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }


    protected override bool CheckConditions()
    {
        // TODO Implement
        /*HealthComponent healthComponent = context.gameObject.GetComponent<HealthComponent>();

        if (healthComponent != null)
        {
            if(healthComponent.Health <= HealthLevelToValue(healthLevel))
            {
                return true;
            }
        }*/

        return false;
    }

    public static int HealthLevelToValue(HealthLevel healthLevel)
    {
        switch (healthLevel)
        {
            case HealthLevel.Low:
                return 2;
            case HealthLevel.Medium:
                return 5;
            case HealthLevel.High:
                return 8;
            default:
                return 0;
        }
    }

}