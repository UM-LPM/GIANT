using TheKiwiCoder;
using UnityEngine;

public enum DropPheromoneDirection
{
    Forward = 1,
    Backward = 2,
    NoAction = 0,
    Random = 3
}

public class DropPheromone : ActionNode
{
    private AntEnvironmentController environmentController;
    public DropPheromoneDirection dropDirection = DropPheromoneDirection.Forward;

    protected override void OnStart()
    {
        environmentController = context.gameObject.GetComponent<AntEnvironmentController>();

    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        Vector3 agentPosition = context.transform.position;

        Instantiate(environmentController.PheromonePrefab, agentPosition, Quaternion.identity);

        var discreteActionsOut = blackboard.actionsOut.DiscreteActions;
        discreteActionsOut[3] = 1; // only 1 direction
        return State.Success;

    }
}
