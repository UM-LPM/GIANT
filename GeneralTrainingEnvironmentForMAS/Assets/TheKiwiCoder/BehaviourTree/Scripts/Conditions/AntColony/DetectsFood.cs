using TheKiwiCoder;
using UnityEngine;

public class DetectsFood : ConditionNode
{
    public LayerMask foodLayer;
    private AntAgentComponent agent;

    protected override void OnStart()
    {
        agent = context.gameObject.GetComponentInParent<AntAgentComponent>();
    }

    protected override bool CheckConditions()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(agent.transform.position, 2f);

        foreach (Collider2D collider in hitColliders)
        {
            FoodComponent foodComponent = collider.GetComponent<FoodComponent>();
            if (foodComponent != null)
            {
                return true; 
            }
        }

        return false; 
    }

    protected override void OnStop()
    {
    }
}
