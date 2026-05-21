using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public class Connection: ADiSComponent
    {
        public List<ActivatorConnection> ActivatorConnections = new List<ActivatorConnection>();
        public List<Action> Actions = new List<Action>();
        public double Weight = 1.0;

        public Connection(Connection other) : base(other)
        {
            this.Weight = other.Weight;
            this.Actions = new List<Action>();
            foreach (var action in other.Actions)
            {
                this.Actions.Add(action.Clone() as Action);
            }

            this.ActivatorConnections = new List<ActivatorConnection>();
            foreach (var activatorConnection in other.ActivatorConnections)
            {
                var activatorClone = activatorConnection.Activator.Clone() as Activator;
                this.ActivatorConnections.Add(new ActivatorConnection(activatorConnection));
            }
        }

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

        public override void BindAndInit(Context context)
        {
            base.BindAndInit(context);
            foreach (var activatorConnection in ActivatorConnections)
            {
                activatorConnection.Activator.BindAndInit(context);
            }

            foreach (var action in Actions)
            {
                action.BindAndInit(context);
            }
        }

        public override ADiSComponent Clone()
        {
            return new Connection(this);
        }

        public override void Init()
        {
            return; // Nothing to init
        }

        public override void ToggleIsExecuting(bool isActive)
        {
            base.ToggleIsExecuting(isActive);

            foreach (var action in Actions)
            {
                action.ToggleIsExecuting(isActive);
            }

            foreach (var activatorConnection in ActivatorConnections)
            {
                activatorConnection.Activator.ToggleIsExecuting(isActive);
            }
        }
    }
}