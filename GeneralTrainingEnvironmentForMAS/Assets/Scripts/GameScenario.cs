using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class GameScenario {
    public string AgentSceneName;
    public BTLoadMode BTLoadMode;
}

public enum BTLoadMode {
    Single,
    Full,
    Custom
}