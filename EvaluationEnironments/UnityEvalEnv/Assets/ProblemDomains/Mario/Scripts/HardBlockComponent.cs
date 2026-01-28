using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Problems.Mario
{
    public class HardBlockComponent: BlockComponent
    {
        public override void OnHit(MarioAgentComponent agent)
        {
            return;
        }
    }
}
