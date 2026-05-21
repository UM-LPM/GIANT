using System;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    [Serializable]
    public abstract class ADiSComponent: Node
    {
        [HideInInspector] public Context context;
        [HideInInspector] public bool IsExecuting = false;

        public ADiSComponent(ADiSComponent other) : base(other)
        {
            context = Context.CreateFromGameObject(other.context.gameObject);
            IsExecuting = other.IsExecuting;
        }

        public virtual void ToggleIsExecuting(bool isExecuting)
        {
            IsExecuting = isExecuting;
        }

        public abstract void Init();

        public virtual void BindAndInit(Context context)
        {
            this.context = context;
            Init();
        }

        public abstract ADiSComponent Clone();
    }
}
