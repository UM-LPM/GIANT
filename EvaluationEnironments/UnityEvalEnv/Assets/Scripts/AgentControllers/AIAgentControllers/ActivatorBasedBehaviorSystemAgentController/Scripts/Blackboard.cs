namespace AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController
{

    // This is the Blackboard container shared between all Nodes.
    // Use this to store temporary data that multiple Nodes need read and write access to.
    // Add other properties here that make sense for your specific use case.
    [System.Serializable]
    public class Blackboard
    {
        public ActionBuffer actionsOut;
    }
}