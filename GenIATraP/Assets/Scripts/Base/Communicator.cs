using System;
using System.Collections;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using AgentOrganizations;
using Fitnesses;
using Configuration;

namespace Base
{
    [RequireComponent(typeof(UnityMainThreadDispatcher))]
    public class Communicator : MonoBehaviour
    {
        [HideInInspector] public static Communicator Instance;

        [Header("Base Configuration")]
        [SerializeField] public ComponentSetupType CommunicatorSetup = ComponentSetupType.MOCK;
        //[SerializeField] public string IndividualsSource;

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

        private void Awake()
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

            InitializeLayerAndGrid();

            InitializeHttpListener();
        }

        void Start()
        {
            ListenerThread = new Thread(StartListener);
            ListenerThread.Start();

            EnvironmentControllerBase.OnGameFinished += EnvironmentController_OnGameFinished;

            Time.timeScale = TimeScale;
            Time.fixedDeltaTime = FixedTimeStep;
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
                }
                else
                {
                    CommunicatorURI = MenuManager.Instance.CommunicatorURI;
                }
            }
        }

        void InitializeLayerAndGrid()
        {
            if (SceneLoadMode == SceneLoadMode.LayerMode)
            {
                Layer = new Layer(MinLayerId, MaxLayerId, BatchSize);
            }
            else if (SceneLoadMode == SceneLoadMode.GridMode)
            {
                Grid = new Grid(GridSize, GridSpacing);
            }
        }

        void InitializeHttpListener()
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
            if (Layer.NumberOfUsedLayeres() > 0)
            {
                context.Response.StatusCode = 888;
                context.Response.OutputStream.Close();
                return;
            }

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

            // Load the scenes and wait for them to be loaded and finished
            yield return LoadEvaluationScenes();

            CommunicatorEvalResponseData evalResponseData = new CommunicatorEvalResponseData() { MatchFitnesses = MatchFitnesses };

            string evalResponseJson = JsonConvert.SerializeObject(evalResponseData);

            byte[] evalResponseBuffer = Encoding.UTF8.GetBytes(evalResponseJson);
            context.Response.ContentLength64 = evalResponseBuffer.Length;
            context.Response.OutputStream.Write(evalResponseBuffer, 0, evalResponseBuffer.Length);
            context.Response.OutputStream.Close();
        }

        /// <summary>
        /// Reads the request data and sets the matches that need to be executed
        /// </summary>
        private void SetEvalRequestMatches(HttpListenerContext context)
        {
            // Read body from the request
            if (!context.Request.HasEntityBody)
            {
                throw new Exception("No client data was sent with the request.");
                // TODO Add error reporting here
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
                // TODO Add error reporting here
            }

            if (evalRequestData == null || evalRequestData.Matches == null || evalRequestData.Matches.Length == 0)
            {
                throw new Exception("Empty request or no matches to execute");
                // TODO Add error reporting here
            }

            // Check if any two matches have the same id and throw and exception if they do
            if (evalRequestData.Matches.GroupBy(x => x.MatchId).Any(g => g.Count() > 1))
            {
                throw new Exception("There are two or more matches with the same id");
                // TODO Add error reporting here
            }

            Matches = evalRequestData.Matches;
        }

        /// <summary>
        /// Sets predefined scores for the matches that have predefined scores (One of the teams has an ID of -1)
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

            // Set predefined scores for the predefined matches and remove them from the Matches list
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

        /// <summary>
        /// Loads all evaluation scenes for the given game and agent scenarios
        /// </summary>
        private IEnumerator LoadEvaluationScenes()
        {
            ScenesLoadedCountRequired = Matches.Length;

            for (int i = 0; i < RerunTimes; i++)
            {
                if (SceneLoadMode == SceneLoadMode.LayerMode)
                {
                    ////////////////////// LAYER MODE /////////////////////////////////
                    foreach (GameScenario gameScenario in GameScenarios)
                    {
                        SceneManager.LoadScene(gameScenario.GameSceneName);

                        foreach (AgentScenario scenario in AgentScenarios)
                        {
                            if (!scenario.ContainsGameScenario(gameScenario.GameSceneName))
                                continue;

                            ScenesLoadedCount = 0;
                            // For each agentScenario all population must be evaluated
                            while (ScenesLoadedCount < ScenesLoadedCountRequired)
                            {
                                while (!Layer.IsBatchExecuted(gameScenario.GameSceneName))
                                {
                                    yield return null;
                                }

                                LoadAgentScenarioLayerMode(scenario, gameScenario);

                                // Check if there is available layer
                                while (!Layer.CanUseAnotherLayer())
                                {
                                    yield return null;
                                }
                            }
                        }

                        // Wait for all agent scenarios for current game agentScenario be finished before continuing to the other one (To prevent collision detection problems)
                        while (Layer.NumberOfUsedLayeres() > 0)
                        {
                            yield return null;
                        }

                        SceneManager.UnloadSceneAsync(gameScenario.GameSceneName);
                    }

                    // Wait for all scene to finish when all different game and agent scenarios have been loaded
                    while (Layer.NumberOfUsedLayeres() > 0)
                    {
                        yield return null;
                    }
                }
                else if (SceneLoadMode == SceneLoadMode.GridMode)
                {
                    ////////////////////// GRID MODE /////////////////////////////////
                    SceneManager.LoadScene(GameScenarios[0].GameSceneName); // Dummy scene for deterministic execution
                                                                            // In GridMode we don't load gameScenes as they are supposed to be inside of every scene
                    foreach (AgentScenario scenario in AgentScenarios)
                    {
                        ScenesLoadedCount = 0;
                        // For each agentScenario all population must be evaluated
                        while (ScenesLoadedCount < ScenesLoadedCountRequired)
                        {
                            while (!Grid.IsBatchExecuted(GameScenarios[0].GameSceneName))
                            {
                                yield return null;
                            }

                            LoadAgentScenarioGridMode(scenario, GameScenarios[0]);

                            // Check if there is available gridCell
                            while (!Grid.CanUseAnotherGridCell())
                            {
                                yield return null;
                            }
                        }
                    }

                    // Wait for all scene to finish when all different game and agent scenarios have been loaded
                    while (Grid.NumberOfUsedGridCells() > 0)
                    {
                        yield return null;
                    }
                    SceneManager.UnloadSceneAsync(GameScenarios[0].GameSceneName);
                }

                // Configure the random seed for each individual run
                if (RandomSeedMode == RandomSeedMode.RandomAll)
                    InitialSeed = InitialSeed + 1;
            }

            if (RandomSeedMode == RandomSeedMode.RandomAll)
                InitialSeed = InitialSeed - RerunTimes;
        }

        void LoadAgentScenarioLayerMode(AgentScenario agentScenario, GameScenario gameScenario)
        {
            while (ScenesLoadedCount < ScenesLoadedCountRequired && Layer.GetAndReserveAvailableLayer(ScenesLoadedCount, gameScenario.GameSceneName, agentScenario.AgentSceneName) >= 0)
            {
                SceneManager.LoadScene(agentScenario.AgentSceneName, LoadSceneMode.Additive);
                ScenesLoadedCount++;
            }
        }

        void LoadAgentScenarioGridMode(AgentScenario agentScenario, GameScenario gameScenario)
        {
            while (ScenesLoadedCount < ScenesLoadedCountRequired && Grid.GetAndReserveAvailableGridlayer(ScenesLoadedCount, gameScenario.GameSceneName, agentScenario.AgentSceneName) != null)
            {
                SceneManager.LoadScene(agentScenario.AgentSceneName, LoadSceneMode.Additive);
                ScenesLoadedCount++;
            }
        }

        private void EnvironmentController_OnGameFinished(object sender, OnGameFinishedEventargs e)
        {
            MatchFitnesses.Add(e.MatchFitness);

            if (SceneLoadMode == SceneLoadMode.LayerMode)
                Layer.ReleaseLayer(e.LayerId);
            else if (SceneLoadMode == SceneLoadMode.GridMode)
                Grid.ReleaseGridCell(e.GridCell);

            SimulationStepsCombined += e.SimulationSteps;
        }

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
            if (Matches == null)
            {
                throw new Exception("Matches is not defined");
                // TODO Add error reporting here
            }

            return Matches[matchIndex];
        }
    }

    public class CommunicatorEvalRequestData
    {
        public Match[] Matches { get; set; }
    }

    public class CommunicatorEvalResponseData
    {
        public List<MatchFitness> MatchFitnesses { get; set; }
    }

    public class OnGameFinishedEventargs : EventArgs
    {
        public MatchFitness MatchFitness;
        public int SimulationSteps;
        public int LayerId;
        public GridCell GridCell;
    }

    public enum SceneLoadMode
    {
        LayerMode,
        GridMode
    }

    public enum RandomSeedMode
    {
        Fixed,
        RandomAll,
        RandomPerIndividual
    }
}