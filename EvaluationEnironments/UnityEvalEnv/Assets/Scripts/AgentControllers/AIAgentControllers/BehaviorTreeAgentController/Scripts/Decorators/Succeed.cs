using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController {
    public class Succeed : DecoratorNode {

        public Succeed(Succeed other) : base(other)
        {
        }

        protected override void OnStart() {
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            var state = child.Update();
            if (state == State.Failure) {
                return State.Success;
            }
            return state;
        }

        override public BTNode Clone()
        {
            return new Succeed(this);
        }
    }
}