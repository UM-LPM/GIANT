using System.Numerics;
using UnityEngine;
using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public abstract class BTNode : ScriptableObject
    {
        public enum State
        {
            Running,
            Failure,
            Success
        }

        public State state = State.Running;
        public bool started;
        public string guid;
        public UnityEngine.Vector2 position;
        public object? context; // Always null
        public Blackboard blackboard;
        public int callFrequencyCount;
        public string description;
        public bool drawGizmos = false;

        public BTNode(Guid guid, string name, List<Property>? properties, Position? position) : base(name, 0)
        {
            this.state = State.Running;
            this.started = false;
            this.guid = guid.ToString();
            this.context = null;
            this.blackboard = new Blackboard();
            this.callFrequencyCount = 0;
            this.description = "";
            this.drawGizmos = false;
            this.position = position != null ? new UnityEngine.Vector2(position.X, position.Y) : new UnityEngine.Vector2(0,0);

            MapProperties(properties);
        }

        protected abstract void MapProperties(List<Property>? properties);

        public abstract void AddChild(BTNode child);
    }
}
