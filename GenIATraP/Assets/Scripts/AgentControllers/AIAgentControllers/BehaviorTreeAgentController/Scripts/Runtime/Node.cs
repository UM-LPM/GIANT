using System.Collections.Generic;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController {
    public abstract class Node : ScriptableObject {
        public enum State {
            Running,
            Failure,
            Success
        }

        [HideInInspector] public State state = State.Running;
        [HideInInspector] public bool started = false;
        [HideInInspector] public string guid;
        [HideInInspector] public Vector2 position;
        [HideInInspector] public Context context;
        [HideInInspector] public Blackboard blackboard;
        public int callFrequencyCount;
        [TextArea] public string description;
        public bool drawGizmos = false;

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

            // Node is being called so we increase the call frequency count
            callFrequencyCount++;

            return state;
        }

        public virtual Node Clone() {
            return Instantiate(this);
        }

        public void Abort() {
            BehaviorTreeAgentController.Traverse(this, (node) => {
                node.started = false;
                node.state = State.Running;
                node.OnStop();
            });
        }

        public virtual void OnDrawGizmos() { }

        protected abstract void OnStart();
        protected abstract void OnStop();
        protected abstract State OnUpdate();

        //public abstract List<Node> GetChildren(); 

        public static string NodeStateToString(State state) {
            switch (state) {
                case State.Running:
                    return "Running";
                case State.Failure:
                    return "Failure";
                case State.Success:
                    return "Success";
                default:
                    return "Unknown";
            }
        }

        public static State NodeStateStringToNodeState(string state) {
            switch (state) {
                case "Running":
                    return State.Running;
                case "Failure":
                    return State.Failure;
                case "Success":
                    return State.Success;
                default:
                    return State.Running;
            }
        }
    }
}