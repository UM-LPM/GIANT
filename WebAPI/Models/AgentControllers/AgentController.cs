
using WebAPI.Models;

namespace AgentControllers
{
    public enum ControllerType
    {
        AI,
        Manual
    }

    [Serializable]
    public abstract class AgentController : ScriptableObject
    {
        public int AgentControllerId;
        public ControllerType ControllerType;

        public AgentController(string name, ControllerType controllerType = ControllerType.AI) : base(name)
        {
            ControllerType = controllerType;
        }

        public abstract void MapTreeModelToAgentController(TreeModel treeModel);
    }
}