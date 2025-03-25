
namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController {
    public abstract class ConditionNode : Node {
        protected abstract bool CheckConditions();

        protected override State OnUpdate() {
            if(CheckConditions()) {
                return State.Success;
            }
            else { 
                return State.Failure; 
            }
        }
    }
}
