using TheKiwiCoder;
using UnityEngine;

public class DetectsWall : ConditionNode
{
    public LayerMask foodLayer;
    private AntAgentComponent agent;

    protected override void OnStart()
    {
        agent = context.gameObject.GetComponentInParent<AntAgentComponent>();
    }

    protected override bool CheckConditions()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(agent.transform.position, 1f);

        foreach (Collider2D collider in hitColliders)
        {
            WallComponent wallComponent = collider.GetComponent<WallComponent>();
            if (wallComponent != null)
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
