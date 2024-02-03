using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ActionBuffer {

    public float[] ContinuousActions { get; set; }
    public int[] DiscreteActions { get; set; }

    public ActionBuffer(float[] continuousActions, int[] discreteActions) {
        ContinuousActions = continuousActions;
        DiscreteActions = discreteActions;
    }

}
