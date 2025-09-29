using UnityEngine;

namespace AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController
{
    public abstract class ABiSNode : Node
    {
        public enum State
        {
            Running,
            Failure,
            Success,
            Idle
        }

        [HideInInspector] public State state = State.Running;
        [HideInInspector] public bool started = false;
        [HideInInspector] public Blackboard blackboard;
        [HideInInspector] public Context context;
        public int callFrequencyCount;

        public State Update()
        {
            if (!started)
            {
                OnStart();
                started = true;
            }

            state = OnUpdate();

            if (state != State.Running)
            {
                OnStop();
                started = false;
            }

            // ABiS is being called so we increase the call frequency count
            callFrequencyCount++;

            return state;
        }

        public virtual ABiSNode Clone()
        {
            return Instantiate(this);
        }

        public void Abort()
        {
            ActivatorBasedBehaviorSystemAgentController.Traverse(this, (node) => {
                node.started = false;
                node.state = State.Running;
                node.OnStop();
            });
        }

        // TODO: Implement (Do i need only need this in behavior nodes?)
        protected abstract void OnStart();
        protected abstract void OnStop();
        protected abstract State OnUpdate();

        public abstract void RemoveChild(ABiSNode child = null);
    }
}