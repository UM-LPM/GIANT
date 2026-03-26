using AgentControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Problems.TicTacToe
{
    [CreateAssetMenu(fileName = "TicTacToeManualAgentController", menuName = "AgentControllers/ManualAgentControllers/TicTacToeManualAgentController")]
    public class TicTacToeManualAgentController : ManualAgentController
    {
        public override void GetActions(in ActionBuffer actionsOut)
        {
            // For now simulate random actions
            actionsOut.AddDiscreteAction("placeRandomMarker", 1);
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