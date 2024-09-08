using TheKiwiCoder;
using UnityEngine;

public class DetectsWater : ConditionNode
{
    public LayerMask waterLayer;
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
            WaterComponent waterComponent = collider.GetComponent<WaterComponent>();
            if (waterComponent != null)
            {
                agent.detectCarriableItem = collider.gameObject;
                return true;
            }
        }

        return false;
    }

    protected override void OnStop()
    {
    }
}
