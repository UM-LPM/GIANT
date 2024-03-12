using TheKiwiCoder;

public class DropPheromone : ActionNode
{

    public int shoot = 1;
    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        //drop pheromone

        return State.Success;
    }
}
