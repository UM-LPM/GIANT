using TheKiwiCoder;

public class AgentHasFood : ConditionNode
{

    private AntAgentComponent agent;

    protected override void OnStart()
    {
        //Get the current agent
        agent = context.gameObject.GetComponentInParent<AntAgentComponent>();
    }

    protected override void OnStop()
    {
    }

    protected override bool CheckConditions()
    {
        return agent.carriedItemObject!=null&& agent.carriedItemObject.GetComponent<FoodComponent>() != null;
    }
}