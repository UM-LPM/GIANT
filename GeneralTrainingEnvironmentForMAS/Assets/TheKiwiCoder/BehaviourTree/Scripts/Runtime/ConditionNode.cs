using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheKiwiCoder {
    public abstract class ConditionNode : Node {
        protected abstract bool CheckConditions();

        protected override State OnUpdate() {
            if(CheckConditions()) {
                return State.Success;
            }
            else { 
                return State.Failure; 
            }
        }
    }
}
