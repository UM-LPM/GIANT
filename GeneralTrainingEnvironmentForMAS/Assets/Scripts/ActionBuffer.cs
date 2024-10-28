using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ActionBuffer {

    public Dictionary<string, int> DiscreteActions;
    public Dictionary<string, float> ContinuousActions;


    public ActionBuffer() {
        DiscreteActions = new Dictionary<string, int>();
        ContinuousActions = new Dictionary<string, float>();
    }

    public void AddDiscreteAction(string actionName, int actionValue)
    {
        if(DiscreteActions.ContainsKey(actionName))
        {
            DiscreteActions[actionName] = actionValue;
        }
        else
        {
            DiscreteActions.Add(actionName, actionValue);
        }
    }

    public void AddContinuousAction(string actionName, float actionValue)
    {
        if(ContinuousActions.ContainsKey(actionName))
        {
            ContinuousActions[actionName] = actionValue;
        }
        else
        {
            ContinuousActions.Add(actionName, actionValue);
        }
    }

    public int GetDiscreteAction(string actionName)
    {
        if(DiscreteActions.ContainsKey(actionName))
        {
            return DiscreteActions[actionName];
        }
        return 0;
    }

    public float GetContinuousAction(string actionName)
    {
        if(ContinuousActions.ContainsKey(actionName))
        {
            return ContinuousActions[actionName];
        }
        return 0.0f;
    }

    public void ResetActions()
    {
        ResetContinuousActions();
        ResetDiscreteActions();
    }

    public void ResetContinuousActions()
    {
        ContinuousActions.Clear();
    }

    public void ResetDiscreteActions()
    {
        DiscreteActions.Clear();
    }

}
