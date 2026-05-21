using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController {
    public class Failure : DecoratorNode {
        public Failure(Failure other) : base(other)
        {
        }

        protected override void OnStart() {
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            var state = child.Update();
            if (state == State.Success) {
                return State.Failure;
            }
            return state;
        }

        public override BTNode Clone()
        {
            return new Failure(this);
        }
    }
}