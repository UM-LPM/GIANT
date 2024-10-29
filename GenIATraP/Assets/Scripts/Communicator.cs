using System;
using System.Collections;
using System.Net;
using System.Text;
using System.Threading;
using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using UnityEngine;
using UnityEngine.SceneManagement;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class Communicator : MonoBehaviour
{
    [HideInInspector] public static Communicator Instance;

    [Header("Initial Seed Configuration")]
    [SerializeField] public int InitialSeed = 316227711;
    [SerializeField] public RandomSeedMode RandomSeedMode = RandomSeedMode.Fixed;
}
public enum SceneLoadMode {
    LayerMode,
    GridMode
}

public enum RandomSeedMode {
    Fixed,
    RandomAll,
    RandomPerIndividual
}