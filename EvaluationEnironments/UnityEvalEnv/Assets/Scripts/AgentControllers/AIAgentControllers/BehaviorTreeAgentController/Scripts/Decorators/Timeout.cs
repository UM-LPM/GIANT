using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController {
    public class Timeout : DecoratorNode {
        public float duration = 1.0f;
        float startTime;

        public Timeout(Timeout other) : base(other)
        {
            if (other == null)
                return;

            this.duration = other.duration;
        }

        protected override void OnStart() {
            startTime = Time.time;
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            if (Time.time - startTime > duration) {
                return State.Failure;
            }

            return child.Update();
        }

        public override BTNode Clone()
        {
            return new Timeout(this);
        }
    }
}