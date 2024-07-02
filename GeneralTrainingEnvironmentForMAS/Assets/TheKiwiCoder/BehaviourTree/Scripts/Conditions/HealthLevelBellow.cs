using System.Collections.Generic;
using TheKiwiCoder;

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
        HealthComponent healthComponent = context.gameObject.GetComponent<HealthComponent>();

        if (healthComponent != null)
        {
            if(healthComponent.Health <= HealthLevelToValue(healthLevel))
            {
                return true;
            }
        }

        return false;
    }

    public static Node CreateNodeFromBehaviourTreeNodeDef(BehaviourTreeNodeDef behaviourTreeNodeDef, List<BehaviourTreeNodeDef> behaviourTreeNodeDefs, BehaviourTree tree)
    {
        // Create node
        HealthLevelBellow healthLevelNode = new HealthLevelBellow();

        // Set node properties
        healthLevelNode.healthLevel = (HealthLevel)int.Parse(behaviourTreeNodeDef.node_properties["healthLevel"]);

        tree.nodes.Add(healthLevelNode);
        return healthLevelNode;
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