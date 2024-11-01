using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class AgentScenario {
    public string AgentSceneName;
    public GameScenario[] GameScenarios;

    public bool ContainsGameScenario(string gameSceneName) {
        foreach (GameScenario gs in GameScenarios) {
            if(gs.GameSceneName == gameSceneName)
                return true;
        }

        return false;
    }
}

[Serializable]
public class GameScenario {
    public string GameSceneName;
}