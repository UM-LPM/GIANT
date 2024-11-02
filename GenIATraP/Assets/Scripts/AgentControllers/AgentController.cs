using System;
using System.Collections.Generic;
using UnityEngine;

namespace AgentControllers
{
    public enum ControllerType
    {
        AI,
        Manual
    }

    [Serializable]
    public class AgentController: ScriptableObject
    {
        public int AgentControllerId;
        public ControllerType ControllerType;

        public virtual void Initialize(Dictionary<string, object> initParams) { }

        /// <summary>
        /// Processes the input and returns the ActionBuffer with the decisions that Agent should perform in the next fixedUpdate() method call
        /// </summary>
        public virtual void GetActions(in ActionBuffer actionsOut) { }

        public virtual AgentController Clone() {
            return this;
        }
    }
}