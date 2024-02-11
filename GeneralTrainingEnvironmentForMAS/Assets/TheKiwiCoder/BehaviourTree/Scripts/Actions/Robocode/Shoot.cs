using TheKiwiCoder;

public class Shoot : ActionNode {

    public int shoot = 1;
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        var discreteActionsOut = blackboard.actionsOut.DiscreteActions;
        discreteActionsOut[4] = shoot;

        return State.Success;
    }
}
