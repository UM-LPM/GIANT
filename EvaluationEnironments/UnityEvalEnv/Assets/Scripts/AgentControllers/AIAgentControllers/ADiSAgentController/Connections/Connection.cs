using System;
using System.Collections.Generic;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    [Serializable]
    public class Connection: ScriptableObject
    {
        public List<ActivatorConnection> ActivatorConnections;
        public List<Action> Actions;
        public double Weight;

        public bool IsActivated()
        {
            foreach (var activatorConnection in ActivatorConnections)
            {
                bool activated = activatorConnection.Activator.IsActivated();
                if (activatorConnection.IsNegated)
                {
                    activated = !activated;
                }
                if (!activated)
                {
                    return false;
                }
            }
            return true;
        }

        public void GetActions(ActionBuffer actionsOut)
        {
            foreach (var action in Actions)
            {
                action.Execute(actionsOut);
            }
        }

        public void BindAndInit(Context context)
        {
            foreach (var activatorConnection in ActivatorConnections)
            {
                activatorConnection.Activator.BindAndInit(context);
            }

            foreach (var action in Actions)
            {
                action.BindAndInit(context);
            }
        }

        public Connection Clone()
        {
            var clone = ScriptableObject.CreateInstance<Connection>();
            clone.Weight = Weight;

            clone.ActivatorConnections = new List<ActivatorConnection>();
            foreach (var activatorConnection in ActivatorConnections)
            {
                var activatorClone = activatorConnection.Activator.Clone() as Activator;
                var activatorConnectionClone = new ActivatorConnection
                {
                    Activator = activatorClone,
                    IsNegated = activatorConnection.IsNegated
                };
                clone.ActivatorConnections.Add(activatorConnectionClone);
            }

            clone.Actions = new List<Action>();
            foreach (var action in Actions)
            {
                var actionClone = action.Clone() as Action;
                clone.Actions.Add(actionClone);
            }

            return clone;
        }
    }
}