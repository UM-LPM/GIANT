using System.Collections.Generic;
using System.Linq;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController {
    public class Parallel : CompositeNode {
        List<State> childrenLeftToExecute = new List<State>();

        protected override void OnStart() {
            childrenLeftToExecute.Clear();
            children.ForEach(a => {
                childrenLeftToExecute.Add(State.Running);
            });
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            bool stillRunning = false;
            for (int i = 0; i < childrenLeftToExecute.Count(); ++i) {
                if (childrenLeftToExecute[i] == State.Running) {
                    var status = children[i].Update();
                    if (status == State.Failure) {
                        AbortRunningChildren();
                        return State.Failure;
                    }

                    if (status == State.Running) {
                        stillRunning = true;
                    }

                    childrenLeftToExecute[i] = status;
                }
            }

            return stillRunning ? State.Running : State.Success;
        }

        void AbortRunningChildren() {
            for (int i = 0; i < childrenLeftToExecute.Count(); ++i) {
                if (childrenLeftToExecute[i] == State.Running) {
                    children[i].Abort();
                }
            }
        }
    }
}