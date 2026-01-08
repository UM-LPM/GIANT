using UnityEngine;
using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    [Serializable]
    public abstract class Node : ScriptableObject
    {
        public Vector2 position;
        public string description;
        public string guid;
        public bool drawGizmos = false;

        public Node(string guid, string name, Position? position) : base(name)
        {
            this.position = position != null ? new UnityEngine.Vector2(position.X, position.Y) : new UnityEngine.Vector2(0, 0);
            description = "";
            this.guid = guid;
        }

    }
}
