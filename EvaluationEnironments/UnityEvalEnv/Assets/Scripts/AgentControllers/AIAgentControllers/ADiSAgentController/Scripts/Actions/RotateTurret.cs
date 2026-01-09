using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public class RotateTurret : Action
    {

        public RotateDirection rotateDirection = RotateDirection.Random;

        private Util Util;
        public override void Init()
        {
            Util = context.gameObject.GetComponentInParent<Util>();
        }

        public override void Execute(ActionBuffer actionsOut)
        {
            actionsOut.AddDiscreteAction("rotateTurretDirection", rotateDirection == RotateDirection.Random ? Util.NextIntAC(this.context.transform.GetInstanceID(), 3) : (int)rotateDirection);
        }
    }
}
