
namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController {
    public class InterruptSelector : Selector {
        public InterruptSelector(InterruptSelector other) : base(other)
        {
        }

        protected override State OnUpdate() {
            int previous = current;
            base.OnStart();
            var status = base.OnUpdate();
            if (previous != current) {
                if (children[previous].state == State.Running) {
                    children[previous].Abort();
                }
            }

            return status;
        }

        public override BTNode Clone()
        {
            return new InterruptSelector(this);
        }
    }
}