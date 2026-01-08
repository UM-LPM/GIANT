using UnityEngine;
using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public abstract class ADiSComponent : Node
    {
        public object? context;
        public bool IsExecuting = false;

        public ADiSComponent(string guid,string name, List<Property>? properties, Position? position) : base(guid, name, position)
        {
            MapProperties(properties);
        }

        protected abstract void MapProperties(List<Property>? properties);
    }
}
