using System.Numerics;
using WebAPI.Models;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public abstract class Node : ScriptableObject
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
        public Position position;
        public object? context; // Always null
        public Blackboard blackboard;
        public int callFrequencyCount;
        public string description;
        public bool drawGizmos = false;

        public Node(Guid guid, string name, List<Property>? properties, Position? position) : base(name, 0)
        {
            this.state = State.Running;
            this.started = false;
            this.guid = guid.ToString();
            this.context = null;
            this.blackboard = new Blackboard();
            this.callFrequencyCount = 0;
            this.description = "";
            this.drawGizmos = false;
            this.position = position != null ? position : new Position(0,0);

            MapProperties(properties);
        }

        protected abstract void MapProperties(List<Property>? properties);
    }
}
