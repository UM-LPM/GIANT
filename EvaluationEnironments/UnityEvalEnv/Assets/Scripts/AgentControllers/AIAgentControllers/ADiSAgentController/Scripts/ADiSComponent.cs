using System;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    [Serializable]
    public abstract class ADiSComponent: Node
    {
        [HideInInspector] public Context context;

        public abstract void Init();

        public virtual void BindAndInit(Context context)
        {
            this.context = context;
            Init();
        }

        public virtual ADiSComponent Clone()
        {
            return Instantiate(this);
        }
    }
}
