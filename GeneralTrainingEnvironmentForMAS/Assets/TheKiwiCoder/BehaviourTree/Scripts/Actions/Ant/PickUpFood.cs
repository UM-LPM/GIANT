using TheKiwiCoder;
using UnityEngine;

public class PickUpFood : ActionNode
{
    private AntEnvironmentController environmentController;

    protected override void OnStart()
    {
        environmentController = context.gameObject.GetComponentInParent<AntEnvironmentController>();

    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
            Vector3 agentPosition = context.transform.position;

            AntAgentComponent agentComponent = context.agent.GetComponent<AntAgentComponent>();


        foreach (GameObject food in environmentController.FoodItems)
        {
            if (Vector3.Distance(agentPosition, food.transform.position) < 1.0f)
            {
                if (!agentComponent.hasFood)
                {
                    agentComponent.hasFood = true;
                    Destroy(food);
                    return State.Success;
                }
            }
        }

        return State.Failure;
    }
}
