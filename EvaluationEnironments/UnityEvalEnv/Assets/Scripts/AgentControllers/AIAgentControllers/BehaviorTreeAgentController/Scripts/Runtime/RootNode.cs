using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController {

    public class RootNode : BTNode {
        [HideInInspector] public BTNode child;

        protected override void OnStart() {

        }

        protected override void OnStop() {

        }

        protected override State OnUpdate() {
            return child.Update();
        }

        public override BTNode Clone() {
            RootNode node = Instantiate(this);
            node.child = child.Clone();
            return node;
        }
    }
}