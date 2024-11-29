namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    [Serializable]
    public class Blackboard
    {
        public object? actionsOut; // Always null

        public Blackboard()
        {
            actionsOut = null;
        }
    }

}