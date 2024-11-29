using AgentControllers;
using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using WebAPI.Models;

namespace AgentOrganizations
{
    [Serializable]
    public class Individual : ScriptableObject
    {
        public int IndividualId;
        public AgentController[] AgentControllers;

        public Individual(int individualId, TreeModel treeModel) : base("Individual_" + individualId.ToString())
        {
            IndividualId = individualId;
            // TODO Update in the future to allow for multiple agent controllers
            AgentControllers = new AgentController[1];

            MapTreeModelToAgentController(treeModel);
        }

        private void MapTreeModelToAgentController(TreeModel treeModel)
        {
            AgentControllers[0] = new BehaviorTreeAgentController("AgentController");

            // Map TreeModel to BehaviorTreeAgentController
            AgentControllers[0].MapTreeModelToAgentController(treeModel);
        }
    }
}