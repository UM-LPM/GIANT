using AgentControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Problems.BoxTact
{
    public class BoxTactManualAgentController
    {
        [CreateAssetMenu(fileName = "BoxTactManualAgentController", menuName = "AgentControllers/ManualAgentControllers/BoxTactManualAgentController")]
        public class BombermanManualAgentController : ManualAgentController
        {
            public override void GetActions(in ActionBuffer actionsOut)
            {
                if (Input.GetKey(KeyCode.W))
                    actionsOut.AddDiscreteAction("moveUpDirection", 1);
                else if (Input.GetKey(KeyCode.S))
                    actionsOut.AddDiscreteAction("moveUpDirection", 2);

                if (Input.GetKey(KeyCode.D))
                    actionsOut.AddDiscreteAction("moveSideDirection", 2);
                else if (Input.GetKey(KeyCode.A))
                    actionsOut.AddDiscreteAction("moveSideDirection", 1);
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
}
