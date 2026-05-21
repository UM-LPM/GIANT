
namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController {
    public class Selector : CompositeNode {
        protected int current;

        public Selector(Selector other) : base(other)
        {
            if (other == null)
                return;

            this.current = other.current;
        }

        protected override void OnStart() {
            current = 0;
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            for (int i = current; i < children.Count; ++i) {
                current = i;
                var child = children[current];

                switch (child.Update()) {
                    case State.Running:
                        return State.Running;
                    case State.Success:
                        return State.Success;
                    case State.Failure:
                        continue;
                }
            }

            return State.Failure;
        }

        public override BTNode Clone()
        {
            return new Selector(this);
        }
    }
}