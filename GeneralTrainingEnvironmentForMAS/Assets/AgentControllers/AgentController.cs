using UnityEngine;

namespace AgentControllers
{
    public enum ControllerType
    {
        AI,
        Manual
    }

    public abstract class AgentController: ScriptableObject
    {
        public ControllerType ControllerType {  get; protected set; }

        /// <summary>
        /// Processes the input and returns the ActionBuffer with the decisions that agent should perform in the next fixedUpdate() method call
        /// </summary>
        public abstract void GetActions(in ActionBuffer actionsOut);
    }
}