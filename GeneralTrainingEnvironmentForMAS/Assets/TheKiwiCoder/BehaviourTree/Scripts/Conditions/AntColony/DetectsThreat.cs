using TheKiwiCoder;
using UnityEngine;

public class DetectsThreat : ConditionNode
{
    public float detectionRange = 10f;
    public LayerMask threatLayer;
    private AntAgentComponent agent;

    protected override void OnStart()
    {
        agent = context.gameObject.GetComponentInParent<AntAgentComponent>();
    }

    protected override bool CheckConditions()
    {
        threatLayer = LayerMask.GetMask("ThreatLayer");
        RaycastHit2D hit = Physics2D.CircleCast(agent.transform.position, detectionRange, Vector2.zero, 0f, threatLayer);
        return hit.collider != null;
    }

    protected override void OnStop()
    {
    }
}
