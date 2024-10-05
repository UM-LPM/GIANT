using TheKiwiCoder;
using UnityEngine;

public class DetectsWall : ConditionNode
{
    private AntAgentComponent agent;
    public float detectionDistance = 0.5f;
    public float angleOffset = 15f; 


    protected override void OnStart()
    {
        agent = context.gameObject.GetComponentInParent<AntAgentComponent>();
    }

    protected override bool CheckConditions()
    {
        Vector2 forward = agent.transform.right;

        if (RaycastForWalls(forward))
        {
            return true;
        }

        Vector2 leftDirection = Quaternion.Euler(0, 0, angleOffset) * forward;
        if (RaycastForWalls(leftDirection))
        {
            return true;
        }

        Vector2 rightDirection = Quaternion.Euler(0, 0, -angleOffset) * forward;
        if (RaycastForWalls(rightDirection))
        {
            return true;
        }

        return false;
    }

    private bool RaycastForWalls(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(agent.transform.position, direction, detectionDistance);

        if (hit.collider != null)
        {
            WallComponent wallComponent = hit.collider.GetComponent<WallComponent>();
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
