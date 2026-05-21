using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController {

    public class RootNode : BTNode {
        [HideInInspector] public BTNode child;

        public RootNode(RootNode other): base(other)
        {
            if (other == null)
                return;

            if (other.child != null) {
                    this.child = other.child.Clone();
            }
        }

        protected override void OnStart() {

        }

        protected override void OnStop() {

        }

        protected override State OnUpdate() {
            return child.Update();
        }

        public override BTNode Clone() {
            return new RootNode(this);
        }
    }
}