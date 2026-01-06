using System;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    [Serializable]
    public abstract class ADiSComponent: ScriptableObject
    {
        [HideInInspector] public Context context;

        public abstract void Init();

        public void BindAndInit(Context context)
        {
            this.context = context;
            Init();
        }

        public ADiSComponent Clone()
        {
            return Instantiate(this);
        }
    }
}
