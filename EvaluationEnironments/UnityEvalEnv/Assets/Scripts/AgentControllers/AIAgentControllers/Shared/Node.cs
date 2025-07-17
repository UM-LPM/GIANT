using UnityEngine;

namespace AgentControllers.AIAgentControllers
{
    public abstract class Node : ScriptableObject
    {
        [HideInInspector] public Vector2 position;
        [TextArea] public string description;
        [HideInInspector] public string guid;
        public bool drawGizmos = false;

        public virtual void OnDrawGizmos()
        {
        }
    }
}