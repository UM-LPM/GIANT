using UnityEngine;

namespace AgentControllers.AIAgentControllers
{
    public abstract class Node //: ScriptableObject
    {
        public string name;
        [HideInInspector] public Vector2 position;
        [TextArea] public string description;
        [HideInInspector] public string guid;
        public bool drawGizmos = false;

        public Node()
        {
        }

        public Node(Node other)
        {
            if (other == null)
                return;

            this.name = other.name;
            this.position = other.position;
            this.description = other.description;
            this.guid = other.guid;
            this.drawGizmos = other.drawGizmos;
        }

        public virtual void OnDrawGizmos()
        {
        }
    }
}