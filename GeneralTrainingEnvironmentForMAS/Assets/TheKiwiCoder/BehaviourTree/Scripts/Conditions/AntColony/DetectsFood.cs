using TheKiwiCoder;
using UnityEngine;

public class DetectsFood : ConditionNode
{
    public float detectionRange = 10f;
    public LayerMask foodLayer;
    private AntAgentComponent agent;

    protected override void OnStart()
    {
        agent = context.gameObject.GetComponentInParent<AntAgentComponent>();
    }

    protected override bool CheckConditions()
    {
        foodLayer = LayerMask.GetMask("Food");
        RaycastHit2D hit = Physics2D.CircleCast(agent.transform.position, detectionRange, Vector2.zero, 0f, foodLayer);
        return hit.collider != null;
    }

    protected override void OnStop()
    {
    }
}
