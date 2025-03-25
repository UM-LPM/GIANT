
namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{

    // This is the Blackboard container shared between all Nodes.
    // Use this to store temporary data that multiple Nodes need read and write access to.
    // Add other properties here that make sense for your specific use case.
    [System.Serializable]
    public class Blackboard {
        public ActionBuffer actionsOut;
    }


    // This is the Blackboard container shared between all behaviour trees in an BehaviourTreeGroup
    // Use this to store temp data
    // Use this for communication?
    [System.Serializable]
    public class BehaviourTreeGroupBlackboard {
    }
}