using System;
using UnityEngine;
using static Unity.VisualScripting.Member;
using UnityEngine.SocialPlatforms;
using System.Threading;
using System.Net;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using AgentOrganizations;
using Evaluators;
using Evaluators.RatingSystems;
using Evaluators.TournamentOrganizations;
using Fitnesses;

public class Coordinator : MonoBehaviour
{
    [HideInInspector] public static Coordinator Instance;

    [Header("Base Configuration")]
    [SerializeField] public string IndividualsSource;

    [Header("HTTP Server Configuration")]
    [SerializeField] public string CoordinatorURI = "http://localhost:4000/";

    [Header("Evaluation Configuration")]
    [SerializeField] public int InitialSeed = 316227711;
    [SerializeField] public EvaluatiorType EvaluatorType;
    [SerializeField] public RatingSystemType RatingSystemType;
    [SerializeField] public TournamentOrganizationType TournamentOrganizationType;
    [SerializeField] public int TournamentRounds;

    private HttpListener Listener;
    private Thread ListenerThread;

    private Individual[] Individuals;

    public System.Random Random { get; private set; }

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        ReadDataFromConfig();

        InitializeHttpServer();
    }

    void Start()
    {
        ListenerThread = new Thread(StartListener);
        ListenerThread.Start();

        // Set global JSON settings to ignore self-referencing loops but preserve references
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
    }

    void ReadDataFromConfig()
    {
        if (MenuManager.Instance != null)
        {
            if (MenuManager.Instance.MainConfiguration != null)
            {
                CoordinatorURI = MenuManager.Instance.MainConfiguration.CoordinatorURI;
                EvaluatorType = MenuManager.Instance.MainConfiguration.EvaluatorType;
                RatingSystemType = MenuManager.Instance.MainConfiguration.RatingSystemType;
                TournamentOrganizationType = MenuManager.Instance.MainConfiguration.TournamentOrganizationType;
                TournamentRounds = MenuManager.Instance.MainConfiguration.TournamentRounds;
                IndividualsSource = MenuManager.Instance.MainConfiguration.IndividualsSource;
            }
            else
            {
                CoordinatorURI = MenuManager.Instance.CoordinatorURI;
            }
        }
    }

    void InitializeHttpServer()
    {
        try
        {
            Listener = new HttpListener();
            Listener.Prefixes.Add(CoordinatorURI);
            Listener.Start();

            Debug.Log("Coordinator HTTP server is running");

        }
        catch (Exception e)
        {
            Debug.Log("Coordinator HTTP server is already running");
        }
    }

    private void StartListener()
    {
        while (Listener.IsListening)
        {
            try
            {
                var context = Listener.GetContext(); // Block until a client connects.
                ThreadPool.QueueUserWorkItem(o => HandleRequest(context));
            }
            catch (HttpListenerException)
            {
                // The Listener was stopped, exit the loop
                break;
            }
        }
    }

    private void HandleRequest(HttpListenerContext context)
    {
        UnityMainThreadDispatcher.Instance().Enqueue((CordinateEvaluation(context)));
    }

    public void StopListener()
    {
        Listener.Stop();
        Listener.Close();
        ListenerThread.Join();
        ListenerThread.Abort();


        Destroy(this.gameObject);
    }

    /// <summary>
    /// This method is called when the coordinator receives a request to evaluate a range of individuals. 
    /// It will distribute the evaluation of the individuals to the evaluation environments and return the final population fitnesses and BTS node call frequencies.
    /// </summary>
    IEnumerator CordinateEvaluation(HttpListenerContext context)
    {
        InitializeRandomGenerator();

        // Load Individuals from IndividualsSource
        LoadIndividuals();

        // Read body from the request
        CoordinatorEvalRequestData evalRequestData = ReadDataFromRequestBody(context);
        Debug.Log("Coordinator Cordinate Evaluator: EvalInstances: " + evalRequestData.EvalEnvInstancesToString());


        Evaluator evaluator;
        Task<CoordinatorEvaluationResult> evaluationResultTask = null;
        switch (EvaluatorType)
        {
            case EvaluatiorType.Simple:
                // Simple evaluation
                evaluator = new SimpleEvaluator();
                evaluationResultTask = evaluator.ExecuteEvaluation(evalRequestData, Individuals);
                break;
            case EvaluatiorType.Complex:
                // Complex evaluation
                // TODO Implement
                break;
            default:
                UnityEngine.Debug.LogError("Invalid evaluator type");
                break;
        }

        while (!evaluationResultTask.IsCompleted)
        {
            yield return null;
        }

        CoordinatorEvaluationResult evaluationResult = evaluationResultTask.Result;
        string responseJson_ = JsonConvert.SerializeObject(evaluationResult);

        byte[] buffer_ = Encoding.UTF8.GetBytes(responseJson_);
        context.Response.ContentLength64 = buffer_.Length;
        context.Response.OutputStream.Write(buffer_, 0, buffer_.Length);
        context.Response.OutputStream.Close();
    }
    
    CoordinatorEvalRequestData ReadDataFromRequestBody(HttpListenerContext context)
    {
        if (!context.Request.HasEntityBody)
        {
            throw new Exception("No client data was sent with the request.");
            // TODO Add error reporting here
        }

        System.IO.Stream body = context.Request.InputStream;
        System.Text.Encoding encoding = context.Request.ContentEncoding;
        System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);

        try
        {
            return JsonConvert.DeserializeObject<CoordinatorEvalRequestData>(reader.ReadToEnd());
        }
        catch (Exception ex)
        {
            throw new Exception("Error reading the request data: " + ex.Message);
            // TODO Add error reporting here
        }
    }

    void InitializeRandomGenerator()
    {
        // Initialize the random number generator
        if (InitialSeed >= 0)
        {
            Random = new System.Random(InitialSeed);
        }
        else
        {
            Random = new System.Random();
        }
    }

    public void LoadIndividuals()
    {
        if(IndividualsSource == null)
        {
            throw new Exception("IndividualsSource is not set");
            // TODO Add error reporting here
        }

        // Load individuals from the IndividualsSource
        Individuals = UnityAssetParser.ParseIndividualsFromFolder(IndividualsSource);

        if (Individuals == null || Individuals.Length == 0)
        {
            throw new Exception("No individuals were loaded from the IndividualsSource");
            // TODO Add error reporting here
        }
    }
}

public class CoordinatorEvalRequestData
{
    public string[] EvalEnvInstances { get; set; }

    public string EvalEnvInstancesToString()
    {
        string evalEnvInstancesString = "";
        foreach (string instance in EvalEnvInstances)
        {
            evalEnvInstancesString += instance + ", ";
        }
        return evalEnvInstancesString;
    }
}

public class CoordinatorEvaluationResult
{
    // TODO Replace this with IndividualFitnessExtended which will track Fitness values from different games and other specific data (BTs node call frequencies)
    public IndividualFitness[] IndividualFitnesses { get; set; } 
}