using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController {
    public class RandomSelector : CompositeNode {
        protected int current;

        public RandomSelector(RandomSelector other) : base(other)
        {
            if (other == null)
                return;

            this.current = other.current;
        }

        protected override void OnStart() {
            current = Random.Range(0, children.Count);
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            var child = children[current];
            return child.Update();
        }

        public override BTNode Clone()
        {
            return new RandomSelector(this);
        }
    }
}