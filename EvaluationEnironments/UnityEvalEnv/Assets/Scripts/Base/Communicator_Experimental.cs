using AgentOrganizations;
using Configuration;
using Fitnesses;
using Newtonsoft.Json;
using PimDeWitte.UnityMainThreadDispatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Base
{
    [RequireComponent(typeof(UnityMainThreadDispatcher))]
    public class Communicator_Experimental : MonoBehaviour
    {
        [HideInInspector] public static Communicator_Experimental Instance;

        [Header("Base Configuration")]
        [SerializeField] public ComponentSetupType CommunicatorSetup = ComponentSetupType.MOCK;

        [Header("HTTP Server Configuration")]
        [SerializeField] public string CommunicatorURI = "http://localhost:4444/";

        [Header("Scenes Configuration")]
        [SerializeField] GameScenario[] GameScenarios;
        [SerializeField] AgentScenario[] AgentScenarios;

        [Header("Execution Configuration")]
        [SerializeField] public float TimeScale = 1f;
        [SerializeField] public float FixedTimeStep = 0.02f;
        [SerializeField] public int RerunTimes = 1;
        [SerializeField] public float PredefinedMatchFitnessWinner = -1000f;
        [SerializeField] public float PredefinedMatchFitnessLoser = 1000f;

        [Header("Response Configuration TODO")]
        // TODO Implement

        [Header("Initial Seed Configuration")]
        [SerializeField] public int InitialSeed = 316227711;
        [SerializeField] public RandomSeedMode RandomSeedMode = RandomSeedMode.Fixed;

        [Header("Experimental Config")]
        [SerializeField] private GameObject[] EnvironmentPrefabs;
        [SerializeField] private int MaxSimultaneousEnvironments = 100;


        [Header("Matches Configuration")]
        [SerializeField] public Match[] Matches;

        private List<MatchFitness> MatchFitnesses;

        private Layer Layer;
        private Grid Grid;

        private int ScenesLoadedCount;
        private int ScenesLoadedCountRequired;

        private int SimulationStepsCombined; // TODO Add support in the future

        private HttpListener Listener;
        private Thread ListenerThread;

        // SetMatchPredefinedScores()
        MatchFitness matchFitness;
        TeamFitness teamFitness;

        // Experimental properties
        private readonly List<SimulationEnvironment> environments = new();
        private bool evaluationInProgress = false;

        private async void Awake()
        {
            // Singleton pattern
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }

            if (CommunicatorSetup == ComponentSetupType.REAL)
            {
                ReadParamsFromMainConfiguration();
            }

            InitializeHttpListener();
        }

        void Start()
        {
            ListenerThread = new Thread(StartListener);
            ListenerThread.Start();

            Time.timeScale = TimeScale;
            Time.fixedDeltaTime = FixedTimeStep;

            Physics.autoSyncTransforms = false;
            Physics.simulationMode = SimulationMode.Script;
            Physics2D.simulationMode = SimulationMode2D.Script;
        }

        private void FixedUpdate()
        {
            if(evaluationInProgress && environments.Count > 0)
            {
                environments.RemoveAll(env =>
                {
                    if (env.IsDone())
                    {
                        env.EnvironmentController.FinishGame();
                        GetSimulationEnvironmentResults(env);
                        env.Terminate();
                        return true;  // remove it
                    }
                    else
                    {
                        env.Step();
                        return false; // keep it
                    }
                });
            }
        }


        void ReadParamsFromMainConfiguration()
        {
            if (MenuManager.Instance != null)
            {
                if (MenuManager.Instance.MainConfiguration != null)
                {
                    CommunicatorURI = MenuManager.Instance.MainConfiguration.StartCommunicatorURI;
                    //IndividualsSource = MenuManager.Instance.MainConfiguration.IndividualsSource;
                    TimeScale = MenuManager.Instance.MainConfiguration.TimeScale;
                    FixedTimeStep = MenuManager.Instance.MainConfiguration.FixedTimeStep;
                    RerunTimes = MenuManager.Instance.MainConfiguration.RerunTimes;
                    InitialSeed = MenuManager.Instance.MainConfiguration.InitialSeed;
                    RandomSeedMode = MenuManager.Instance.MainConfiguration.RandomSeedMode;
                    if (GameScenarios != null && GameScenarios.Length > 0)
                        GameScenarios = MenuManager.Instance.MainConfiguration.GameScenarios;
                    if (AgentScenarios != null && AgentScenarios.Length > 0)
                        AgentScenarios = MenuManager.Instance.MainConfiguration.AgentScenarios;
                    MaxSimultaneousEnvironments = MenuManager.Instance.MainConfiguration.MaxSimultaneousEnvironments;
                }
                else
                {
                    CommunicatorURI = MenuManager.Instance.CommunicatorURI;
                }
            }
        }

        public void CreateSimulationEnvironments()
        {
            for (int j = 0; j < EnvironmentPrefabs.Length; j++)
            {
                for (int i = 0; i < Matches.Length; i++)
                {
                    var env = new SimulationEnvironment(EnvironmentPrefabs[j], $"Environment_{j}_{i}", Matches[i]);
                    environments.Add(env);
                }
            }
        }

        async void InitializeHttpListener()
        {
            string[] uriParts;
            while (true)
            {
                try
                {
                    Listener = new HttpListener();
                    Listener.Prefixes.Add(CommunicatorURI);
                    Listener.Start();

                    Debug.Log("Communicator HTTP server is running");

                    break;
                }
                catch (Exception e)
                {
                    uriParts = CommunicatorURI.Split(':');
                    uriParts[2] = uriParts[2].Split('/')[0];
                    uriParts[2] = int.Parse(uriParts[2]) + 1 + "";
                    CommunicatorURI = uriParts[0] + ":" + uriParts[1] + ":" + uriParts[2] + "/";
                }
            }
        }

        void StartListener()
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

        public void StopListener()
        {
            Listener.Stop();
            Listener.Close();
            ListenerThread.Join();
            ListenerThread.Abort();

            Destroy(gameObject);
        }
        
        private void HandleRequest(HttpListenerContext context)
        {
            if (evaluationInProgress)
            {
                context.Response.StatusCode = 888;
                context.Response.OutputStream.Close();
                return;
            }
            evaluationInProgress = true;

            UnityMainThreadDispatcher.Instance().Enqueue(PerformEvaluation(context));
        }

        IEnumerator PerformEvaluation(HttpListenerContext context)
        {
            if (CommunicatorSetup == ComponentSetupType.REAL)
            {
                SetEvalRequestMatches(context);
            }

            Debug.Log("PerformEvaluation  with " + Matches.Length + " Matches.");

            MatchFitnesses = new List<MatchFitness>();
            SimulationStepsCombined = 0;

            SetMatchPredefinedScores();

            for (int j = 0; j < EnvironmentPrefabs.Length; j++)
            {
                for (int i = 0; i < Matches.Length; i++)
                {
                    // Load simulation environments up to MaxSimultaneousEnvironments
                    while (environments.Count >= MaxSimultaneousEnvironments)
                    {
                        yield return null;
                    }

                    var env = new SimulationEnvironment(EnvironmentPrefabs[j], $"Environment_{j}_{i}", Matches[i]);
                    environments.Add(env);
                }
            }

            // Wait until all environments are done
            while (environments.Count > 0)
            {
                yield return null;
            }

            CommunicatorEvalResponseData evalResponseData = new CommunicatorEvalResponseData() { MatchFitnesses = MatchFitnesses };

            string evalResponseJson = JsonConvert.SerializeObject(evalResponseData);

            byte[] evalResponseBuffer = Encoding.UTF8.GetBytes(evalResponseJson);
            context.Response.ContentLength64 = evalResponseBuffer.Length;
            context.Response.OutputStream.Write(evalResponseBuffer, 0, evalResponseBuffer.Length);
            context.Response.OutputStream.Close();

            evaluationInProgress = false;
        }

        /// <summary>
        /// Reads the request data and sets the TournamentMatches that need to be executed
        /// </summary>
        private void SetEvalRequestMatches(HttpListenerContext context)
        {
            // Read body from the request
            if (!context.Request.HasEntityBody)
            {
                throw new Exception("No client data was sent with the request.");
            }
            System.IO.Stream body = context.Request.InputStream;
            Encoding encoding = context.Request.ContentEncoding;
            System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);

            CommunicatorEvalRequestData evalRequestData;
            try
            {
                evalRequestData = JsonConvert.DeserializeObject<CommunicatorEvalRequestData>(reader.ReadToEnd(), MainConfiguration.JSON_SERIALIZATION_SETTINGS);
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading the request data: " + ex.Message);
            }

            if (evalRequestData == null || evalRequestData.Matches == null || evalRequestData.Matches.Length == 0)
            {
                throw new Exception("Empty request or no TournamentMatches to execute");
            }

            // Check if any two TournamentMatches have the same id and throw and exception if they do
            if (evalRequestData.Matches.GroupBy(x => x.MatchId).Any(g => g.Count() > 1))
            {
                throw new Exception("There are two or more TournamentMatches with the same id");
            }

            Matches = evalRequestData.Matches;
        }

        /// <summary>
        /// Sets predefined scores for the TournamentMatches that have predefined scores (One of the teams has an ID of -1)
        /// </summary>
        void SetMatchPredefinedScores()
        {
            // Check if any Match has more teams but only their ID is -1
            List<Match> predefinedMatches = new List<Match>();
            for (int i = 0; i < Matches.Length; i++)
            {
                for (int j = 0; j < Matches[i].Teams.Length; j++)
                {
                    if (Matches[i].Teams[j].TeamId == -1)
                    {
                        predefinedMatches.Add(Matches[i]);
                        break;
                    }
                }
            }

            // Set predefined scores for the predefined TournamentMatches and remove them from the Matches list
            for (int i = 0; i < predefinedMatches.Count; i++)
            {
                matchFitness = new MatchFitness()
                {
                    MatchName = predefinedMatches[i].name,
                    MatchId = predefinedMatches[i].MatchId,
                    IsDummy = true
                };

                for (int j = 0; j < predefinedMatches[i].Teams.Length; j++)
                {
                    teamFitness = new TeamFitness();
                    teamFitness.TeamID = predefinedMatches[i].Teams[j].TeamId;
                    teamFitness.IndividualFitness = new List<IndividualFitness>();

                    if (predefinedMatches[i].Teams[j].TeamId == -1)
                    {
                        teamFitness.IndividualFitness.Add(new IndividualFitness() { IndividualID = -1, Value = PredefinedMatchFitnessLoser });
                    }
                    else
                    {
                        foreach (var individual in predefinedMatches[i].Teams[j].Individuals)
                        {
                            teamFitness.IndividualFitness.Add(new IndividualFitness() { IndividualID = individual.IndividualId, Value = PredefinedMatchFitnessWinner });
                        }
                    }
                    matchFitness.TeamFitnesses.Add(teamFitness);
                }

                MatchFitnesses.Add(matchFitness);
                Matches = Matches.Where(val => val.MatchId != predefinedMatches[i].MatchId).ToArray();
            }
        }

        private void GetSimulationEnvironmentResults(SimulationEnvironment environment)
        {
            MatchFitnesses.Add(environment.EnvironmentController.MapAgentFitnessesToMatchFitness());
            SimulationStepsCombined += environment.EnvironmentController.SimulationSteps;
        }
    }
}