
namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController {
    public class Sequencer : CompositeNode {
        protected int current;

        public Sequencer(Sequencer other) : base(other)
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
                    case State.Failure:
                        return State.Failure;
                    case State.Success:
                        continue;
                }
            }

            return State.Success;
        }

        public override BTNode Clone()
        {
            return new Sequencer(this);
        }
    }
}