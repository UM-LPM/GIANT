using System.Collections.Generic;
using AITechniques.BehaviorTrees;

public enum ShieldLevel
{
    Low,
    Medium,
    High
}

public class ShieldLevelBellow : ConditionNode
{
    public ShieldLevel shieldLevel;
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
        /*ShieldComponent shieldComponent = context.gameObject.GetComponent<ShieldComponent>();

        if (shieldComponent != null)
        {
            if (shieldComponent.Shield <= ShieldLevelToValue(shieldLevel))
            {
                return true;
            }
        }*/

        return false;
    }

    public static Node CreateNodeFromBehaviourTreeNodeDef(BehaviourTreeNodeDef behaviourTreeNodeDef, List<BehaviourTreeNodeDef> behaviourTreeNodeDefs, BehaviourTree tree)
    {
        // Create node
        ShieldLevelBellow shieldLevelNode = new ShieldLevelBellow();

        // Set node properties
        shieldLevelNode.shieldLevel = (ShieldLevel)int.Parse(behaviourTreeNodeDef.node_properties["shieldLevel"]);

        tree.nodes.Add(shieldLevelNode);
        return shieldLevelNode;
    }

    public static int ShieldLevelToValue(ShieldLevel ShieldLevel)
    {
        switch (ShieldLevel)
        {
            case ShieldLevel.Low:
                return 4;
            case ShieldLevel.Medium:
                return 6;
            case ShieldLevel.High:
                return 8;
            default:
                return 0;
        }
    }

}