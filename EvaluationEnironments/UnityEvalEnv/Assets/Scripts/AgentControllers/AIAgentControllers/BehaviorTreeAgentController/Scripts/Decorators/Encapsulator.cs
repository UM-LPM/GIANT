using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
	public class Encapsulator : DecoratorNode
	{
		public Encapsulator(Encapsulator other) : base(other)
		{
        }

        protected override void OnStart()
		{
		}

		protected override void OnStop()
		{
		}

		protected override State OnUpdate()
		{
			switch (child.Update())
			{
				case State.Running:
					return State.Running;
				case State.Failure:
					return State.Failure;
				case State.Success:
					return State.Success;
			}
			return State.Failure;
		}

        public override BTNode Clone()
        {
            return new Encapsulator(this);
        }
	}
}