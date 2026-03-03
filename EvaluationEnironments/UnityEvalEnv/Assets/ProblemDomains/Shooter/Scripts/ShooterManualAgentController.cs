using AgentControllers;
using Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.Shooter
{
    [CreateAssetMenu(fileName = "ShooterManualAgentController", menuName = "AgentControllers/ManualAgentControllers/ShooterManualAgentController")]
    public class ShooterManualAgentController : ManualAgentController
    {
        public override void GetActions(in ActionBuffer actionsOut)
        {
            if (Input.GetKey(KeyCode.W))
                actionsOut.AddDiscreteAction("moveForwardDirection", 1);
            else if (Input.GetKey(KeyCode.S))
                actionsOut.AddDiscreteAction("moveForwardDirection", 2);

            if (Input.GetKey(KeyCode.D))
                actionsOut.AddDiscreteAction("rotateDirection", 2);
            else if (Input.GetKey(KeyCode.A))
                actionsOut.AddDiscreteAction("rotateDirection", 1);

            if (Input.GetKey(KeyCode.Space))
                actionsOut.AddDiscreteAction("shoot", 1);
        }

        public override AgentController Clone()
        {
            return this;
        }

        public override void AddAgentControllerToSO(ScriptableObject parent)
        {
            return;
        }
    }
}
