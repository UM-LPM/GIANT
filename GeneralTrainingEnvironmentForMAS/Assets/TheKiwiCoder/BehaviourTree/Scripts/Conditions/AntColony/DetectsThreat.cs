using TheKiwiCoder;
using UnityEngine;

public class DetectsThreat : ConditionNode
{
    public LayerMask threatLayer;
    private AntAgentComponent agent;

    protected override void OnStart()
    {
        agent = context.gameObject.GetComponentInParent<AntAgentComponent>();
    }

    protected override bool CheckConditions()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(agent.transform.position, 5f);


        foreach (Collider2D collider in hitColliders)
        {
            Threat threatComponent = collider.GetComponent<Threat>();
            if (threatComponent != null)
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
