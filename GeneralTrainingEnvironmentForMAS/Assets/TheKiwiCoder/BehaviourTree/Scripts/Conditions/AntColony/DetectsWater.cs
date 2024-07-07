using TheKiwiCoder;
using UnityEngine;

public class DetectsWater : ConditionNode
{
    public float detectionRange = 10f;
    public LayerMask waterLayer;
    private AntAgentComponent agent;

    protected override void OnStart()
    {
        agent = context.gameObject.GetComponent<AntAgentComponent>();
    }

    protected override bool CheckConditions()
    {
        waterLayer = LayerMask.GetMask("Water");
        RaycastHit2D hit = Physics2D.CircleCast(agent.transform.position, detectionRange, Vector2.zero, 0f, waterLayer);
        return hit.collider != null;
    }

    protected override void OnStop()
    {
    }
}
