using System.Collections.Generic;
using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;

public class Shoot : ActionNode {

    public int shoot = 1;
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        var discreteActionsOut = blackboard.actionsOut.DiscreteActions;
        blackboard.actionsOut.AddDiscreteAction("shoot", shoot);

        return State.Success;
    }
}
