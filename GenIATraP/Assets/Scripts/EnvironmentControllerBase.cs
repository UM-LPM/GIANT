using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AITechniques.BehaviorTrees;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class EnvironmentControllerBase : MonoBehaviour {

    public static event EventHandler<OnGameFinishedEventargs> OnGameFinished;

    [Header("Base Configuration")]
    [SerializeField] public EnvironmentControllerSetupType EnvironmentControllerSetup = EnvironmentControllerSetupType.MOCK;

    public abstract void UpdateAgents(bool updateBTs); // Move updateBTs to somwhere else
}

public class OnGameFinishedEventargs : EventArgs
{
}

public enum GameState {
    IDLE,
    RUNNING,
    FINISHED
}
public enum GameType
{
    _2D,
    _3D
}

public enum EnvironmentControllerSetupType
{
    MOCK,
    REAL
}