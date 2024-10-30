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
using AgentOrganizations;

public class Communicator : MonoBehaviour
{
    [HideInInspector] public static Communicator Instance;

    [Header("HTTP Server Configuration")]
    [SerializeField] public string CommunicatorURI = "http://localhost:4444/";

    [Header("Scene Loading Configuration")]
    [SerializeField] public SceneLoadMode SceneLoadMode = SceneLoadMode.LayerMode;

    [Header("LayerMode Configuration")]
    [SerializeField] int MinLayerId = 6;
    [SerializeField] int MaxLayerId = 26;
    [SerializeField] private int BatchSize = 1;

    [Header("GridMode Configuration")]
    [SerializeField] Vector3Int GridSize = new Vector3Int(10, 0, 10);
    [SerializeField] Vector3Int GridSpacing = new Vector3Int(50, 0, 50);

    [Header("Scenes Configuration")]
    [SerializeField] public string BtSource;
    [SerializeField] GameScenario[] GameScenarios;
    [SerializeField] AgentScenario[] AgentScenarios;

    [Header("Execution Configuration")]
    [SerializeField] public float TimeScale = 1f;
    [SerializeField] public float FixedTimeStep = 0.02f;
    [SerializeField] public int RerunTimes = 1;

    [Header("Response Configuration TODO")]
    // TODO Implement

    [Header("Initial Seed Configuration")]
    [SerializeField] public int InitialSeed = 316227711;
    [SerializeField] public RandomSeedMode RandomSeedMode = RandomSeedMode.Fixed;


    private List<Match> Matches;

    private Layer Layer;
    private Grid Grid;


    public LayerData GetReservedLayer()
    {
        return Layer.GetReservedLayer();
    }

    public GridCell GetReservedGridCell()
    {
        return Grid.GetReservedGridCell();
    }

    public Match GetMatch(int matchIndex)
    {
        if(Matches == null)
        {
            throw new System.Exception("Matches is not defined");
            // TODO Add error reporting here
        }
        
        return Matches[matchIndex];
    }
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