using TheKiwiCoder;

public class PickUpFood : ActionNode
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
        //pick up food

        return State.Success;
    }
}
