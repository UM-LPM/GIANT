using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController {
    public abstract class BTNode : Node {
        public enum State {
            Running,
            Failure,
            Success
        }

        [HideInInspector] public State state = State.Running;
        [HideInInspector] public bool started = false;
        [HideInInspector] public Blackboard blackboard;
        [HideInInspector] public Context context;
        public int callFrequencyCount;

        public State Update() {

            if (!started) {
                OnStart();
                started = true;
            }

            state = OnUpdate();

            if (state != State.Running) {
                OnStop();
                started = false;
            }

            // BTNode is being called so we increase the call frequency count
            callFrequencyCount++;

            return state;
        }

        public virtual BTNode Clone() {
            return Instantiate(this);
        }

        public void Abort() {
            BehaviorTreeAgentController.Traverse(this, (node) => {
                node.started = false;
                node.state = State.Running;
                node.OnStop();
            });
        }

        protected abstract void OnStart();
        protected abstract void OnStop();
        protected abstract State OnUpdate();
    }
}