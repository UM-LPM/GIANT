using TheKiwiCoder;
using UnityEngine;

public class DetectsObjectOfType : ConditionNode
{
    public enum ObjectType
    {
        Wall,
        Water,
        Threat,
        Food
    }
    public ObjectType objectType;
    private AntAgentComponent agent;
    public float detectionDistance = 2f; 
    public float angleOffset = 15f; 

    protected override void OnStart()
    {
        agent = context.gameObject.GetComponentInParent<AntAgentComponent>();
    }

    protected override bool CheckConditions()
    {
        switch (objectType)
        {
            case ObjectType.Wall:
                return DetectWall();
            case ObjectType.Water:
                return DetectObject<WaterComponent>(detectionDistance);
            case ObjectType.Threat:
                return DetectObject<Threat>(5f);
            case ObjectType.Food:
                return DetectObject<FoodComponent>(detectionDistance);
            default:
                return false;
        }
    }

    private bool DetectWall()
    {
        Vector2 forward = agent.transform.right;

        if (RaycastForWalls(forward) ||
            RaycastForWalls(Quaternion.Euler(0, 0, angleOffset) * forward) ||
            RaycastForWalls(Quaternion.Euler(0, 0, -angleOffset) * forward))
        {
            return true;
        }

        return false;
    }

    private bool RaycastForWalls(Vector2 direction)
    { RaycastHit2D hit = Physics2D.Raycast(agent.transform.position, direction, detectionDistance);

       
        if (hit.collider != null && hit.collider.GetComponent<WallComponent>() != null)
        {
            return true;
        }

        return false;
    }

    private bool DetectObject<T>(float radius) where T : Component
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(agent.transform.position, radius);

        foreach (Collider2D collider in hitColliders)
        {
            if (collider.GetComponent<T>() != null)
            {
                if (typeof(T) == typeof(FoodComponent) || typeof(T) == typeof(WaterComponent))
                {
                    agent.detectCarriableItem = collider.gameObject;
                }
                return true;
            }
        }

        return false;
    }

    protected override void OnStop()
    {
    }
}
