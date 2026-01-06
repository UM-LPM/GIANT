using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    [Serializable]
    [CreateAssetMenu(fileName = "ADiSAgentController", menuName = "AgentControllers/AIAgentControllers/ADiSAgentController")]
    public class ADiSAgentController : AIAgentController
    {
        public List<Connection> Connections;

        public override void AddAgentControllerToSO(ScriptableObject parent)
        {
            throw new NotImplementedException();
        }

        public override AgentController Clone()
        {
            var clone = Instantiate(this);
            clone.Connections = new List<Connection>();
            foreach (var connection in Connections)
            {
                clone.Connections.Add(connection.Clone());
            }

            return clone;
        }

        public override void GetActions(in ActionBuffer actionsOut)
        {
            // 1. Find all activated connections
            var activatedConnections = new List<Connection>();
            foreach (var connection in Connections)
            {
                if (connection.IsActivated())
                {
                    activatedConnections.Add(connection);
                }
            }

            // 2. From all activated connections get the one with the highest weight (or first if multiple have the same weight)
            var selectedConnection = activatedConnections
                .OrderByDescending(c => c.Weight)
                .FirstOrDefault();

            // 3. Get and set the actions from the selected connection
            if(selectedConnection != null)
            {
                selectedConnection.GetActions(actionsOut);
            }
        }

        public void BindAndInit(Context context)
        {
            foreach (var connection in Connections)
            {
                connection.BindAndInit(context);
            }
        }

        public static Context CreateADiSContext(GameObject agentGameObject)
        {
            return Context.CreateFromGameObject(agentGameObject);
        }
    }
}