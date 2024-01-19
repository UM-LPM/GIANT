using System;
using System.Collections;
using System.Net;
using System.Text;
using System.Threading;
using TheKiwiCoder;
using UnityEngine;
using UnityEngine.SceneManagement;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections.Generic;
using System.Linq;

public class Communicator2 : MonoBehaviour {
    [SerializeField] private string BtSource; // = "SoccerBts";
    [SerializeField] private string GameSceneName; // = "SoccerSceneGame";
    [SerializeField] private string[] GameScenarios; /* = { 
        "SoccerSceneAgents1", 
        "SoccerSceneAgents2", 
        "SoccerSceneAgents3", 
        "SoccerSceneAgents4", 
        "SoccerSceneAgents5", 
        //"SoccerSceneAgents6", 
        //"SoccerSceneAgents7", 
        "SoccerSceneAgents8" 
    };*/

    [SerializeField] private float timeScale = 1f;
    [SerializeField] private int batchSize = 1;

    [HideInInspector] public static Communicator2 Instance;

    private int currentIndividualID = 0;

    private int currentLayerId = 6;

    private HttpListener listener;
    private Thread listenerThread;

    private PopFitness PopFitness;

    private bool batchExecuting;

    private int currentResponses;

    private BehaviourTree[] popBTs;

    private int currentBatchSize;

    // Start is called before the first frame update
    void Start() {
        if (Instance != null) {
            Destroy(this);
        }
        else {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:4444/");
        listener.Start();

        listenerThread = new Thread(StartListener);
        listenerThread.Start();

        EnvironmentControllerBase.OnGameFinished += EnvironmentController_OnGameFinished;

        batchExecuting = false;
        currentResponses = 0;

        Time.timeScale = timeScale;
    }

    private void EnvironmentController_OnGameFinished(object sender, EnvironmentControllerBase.OnGameFinishedEventargs e) {
        foreach (GroupFitness fitnessGroup in e.FitnessGroups) {
            foreach (FitnessIndividual fitnessIndividual in fitnessGroup.FitnessIndividuals) {
                if (fitnessIndividual.IndividualId < 0)
                    Debug.LogError("IndividualID is -1");

                PopFitness.Fitnesses[fitnessIndividual.IndividualId] += fitnessIndividual.Fitness.GetFitness();
            }
        }

        if (++currentResponses == currentBatchSize) {
            batchExecuting = false;
        }
    }

    private void StartListener() {
        while (listener.IsListening) {
            try {
                var context = listener.GetContext(); // Block until a client connects.
                ThreadPool.QueueUserWorkItem(o => HandleRequest(context));
            }
            catch (HttpListenerException) {
                // The Listener was stopped, exit the loop
                break;
            }
        }
    }

    private void HandleRequest(HttpListenerContext context) {
        if (batchExecuting) {
            context.Response.StatusCode = 888;
            context.Response.OutputStream.Close();
            return;
        }

        UnityMainThreadDispatcher.Instance().Enqueue((PerformEvaluation(context)));
    }

    IEnumerator PerformEvaluation(HttpListenerContext context) {

        // Refresh the Asset Database
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        #endif

        // Load coresponding behaviour trees and order them by name 
        popBTs = Resources.LoadAll<BehaviourTree>(BtSource);
        popBTs = popBTs.OrderBy(bt => bt.id).ToArray();

        // Reset variables
        //pop_fitness = new float[PopBTs.Length];
        PopFitness = new PopFitness(popBTs.Length);

        currentIndividualID = 0;

        // For each behaviour Tree execute evaluation
        for (int i = 0; i < popBTs.Length; i += batchSize) {
            for (int soccerSceneIndex = 0; soccerSceneIndex < GameScenarios.Length; soccerSceneIndex++) {
                currentResponses = 0;
                currentLayerId = 7;

                // Load SoccerSceneBase
                SceneManager.LoadScene(GameSceneName);

                batchExecuting = true;
                // Load BatchSize of game scenes
                currentBatchSize = (Math.Abs(i - popBTs.Length) < batchSize) ? Math.Abs(i - popBTs.Length) : batchSize;
                for (int j = 0; j < currentBatchSize; j++) {
                    SceneManager.LoadScene(GameScenarios[soccerSceneIndex], LoadSceneMode.Additive);
                }

                // Wait until all environments finish
                while (batchExecuting) {
                    yield return null;
                }

                if (soccerSceneIndex < GameScenarios.Length - 1) {
                    currentIndividualID = currentIndividualID - currentBatchSize;
                }
            }
        }

        Debug.Log("PerformEvaluation function finished");

        // TODO add support for other methods (std. deviation, min, max, ...)
        // Average all fitneses by the number of scenes 
        for (int i = 0; i < PopFitness.Fitnesses.Length; i++) {
            PopFitness.Fitnesses[i] = PopFitness.Fitnesses[i] / GameScenarios.Length;
        }

        HttpServerResponse response = new HttpServerResponse() { PopFitness = PopFitness.Fitnesses };
        string responseJson = JsonUtility.ToJson(response);

        byte[] buffer = Encoding.UTF8.GetBytes(responseJson);
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.OutputStream.Close();

    }

    public bool ScenesLoaded(List<AsyncOperation> operations) {
        foreach (AsyncOperation op in operations) {
            if (!op.isDone) {
                return false;
            }
        }
        return true;
    }

    public int GetCurrentLayerId() {
        return this.currentLayerId++;
    }

    public int GetCurrentIndividualID() {
        return currentIndividualID++;
    }

    // TODO Add support for homogenous agens
    public BehaviourTree GetBehaviourTree(int index) {
        if (index < 0 || index > popBTs.Length)
            Debug.LogError("Invalid index for poopBTs");

        return popBTs[index];
    }
}
