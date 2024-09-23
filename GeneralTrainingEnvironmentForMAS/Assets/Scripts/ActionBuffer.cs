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

    public void ResetActions()
    {
        ResetContinuousActions();
        ResetDiscreteActions();
    }

    public void ResetContinuousActions()
    {
        for (int i = 0; i < ContinuousActions.Length; i++)
        {
            ContinuousActions[i] = 0;
        }
    }

    public void ResetDiscreteActions()
    {
        for (int i = 0; i < DiscreteActions.Length; i++)
        {
            DiscreteActions[i] = 0;
        }
    }

}
