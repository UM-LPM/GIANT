using TheKiwiCoder;

public class AgentHasFood : ConditionNode
{

    private AntAgentComponent agent;

    protected override void OnStart()
    {
        //Get the current agent
        agent = context.gameObject.GetComponent<AntAgentComponent>();
    }

    protected override void OnStop()
    {
    }

    protected override bool CheckConditions()
    {
        return agent.hasFood;
    }
}