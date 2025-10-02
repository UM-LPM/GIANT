using System;
using UnityEngine;
using System.Threading;
using System.Net;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using AgentOrganizations;
using Evaluators;
using Evaluators.CompetitionOrganizations;
using Evaluators.CompetitionOrganizations;
using Fitnesses;
using Utils;
using Configuration;

namespace Base
{
    public class Coordinator : MonoBehaviour
    {
        [HideInInspector] public static Coordinator Instance;

        [Header("Base Configuration")]
        [SerializeField] public ComponentSetupType CoordinatorSetup = ComponentSetupType.MOCK;
        [SerializeField] public string IndividualsSourceJSON;
        [SerializeField] public string IndividualsSourceSO;
        [SerializeField] public bool ConvertSOToJSON = false;

        [Header("HTTP Server Configuration")]
        [SerializeField] public string CoordinatorURI = "http://localhost:4000/";

        [Header("Evaluation Configuration")]
        [SerializeField] public int InitialSeed = 316227711;
        [SerializeField] public EvaluatiorType EvaluatorType;
        [SerializeField] public RatingSystemType RatingSystemType;
        [SerializeField] public bool CreateNewTeamsEachRound;
        [SerializeField] public CompetitionOrganizationType CompetitionOrganizationType;
        [SerializeField] public int CompetitionRounds;
        [SerializeField] public bool SwapCompetitionMatchTeams = false; // Specific for games like (Robostrike, ...)

        [Header("Individuals Configuration")]
        [SerializeField] public Individual[] Individuals;

        private HttpListener Listener;
        private Thread ListenerThread;

        private CompetitionTeamOrganizator TeamOrganizator;

        public System.Random Random { get; private set; }

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

            ReadDataFromConfig();

            InitializeHttpServer();
        }

        void Start()
        {
            ListenerThread = new Thread(StartListener);
            ListenerThread.Start();
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
                    SwapCompetitionMatchTeams = MenuManager.Instance.MainConfiguration.SwapCompetitionMatchTeams;
                    CreateNewTeamsEachRound = MenuManager.Instance.MainConfiguration.CreateNewTeamsEachRound;
                    CompetitionOrganizationType = MenuManager.Instance.MainConfiguration.CompetitionOrganizationType;
                    CompetitionRounds = MenuManager.Instance.MainConfiguration.CompetitionRounds;
                    IndividualsSourceJSON = MenuManager.Instance.MainConfiguration.IndividualsSourceJSON;
                    IndividualsSourceSO = MenuManager.Instance.MainConfiguration.IndividualsSourceSO;
                    ConvertSOToJSON = MenuManager.Instance.MainConfiguration.ConvertSOToJSON;
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
            UnityMainThreadDispatcher.Instance().Enqueue(CordinateEvaluation(context));
        }

        public void StopListener()
        {
            Listener.Stop();
            Listener.Close();
            ListenerThread.Join();
            ListenerThread.Abort();


            Destroy(gameObject);
        }

        /// <summary>
        /// This method is called when the coordinator receives a request to evaluate a range of individuals. 
        /// It will distribute the evaluation of the individuals to the evaluation environments and return the final population fitnesses and BTS node call frequencies.
        /// </summary>
        IEnumerator CordinateEvaluation(HttpListenerContext context)
        {
            InitializeRandomGenerator();

            // Read body from the request
            CoordinatorEvalRequestData evalRequestData = ReadDataFromRequestBody(context);
            Debug.Log("Coordinator Cordinate Evaluator: EvalInstances: " + evalRequestData.EvalEnvInstancesToString());

            if (CoordinatorSetup == ComponentSetupType.REAL)
            {
                // Load Individuals from IndividualsSource
                LoadIndividualsFromJSON(evalRequestData.EvalRangeStart.HasValue ? evalRequestData.EvalRangeStart.Value : -1, evalRequestData.EvalRangeEnd.HasValue ? evalRequestData.EvalRangeEnd.Value : -1);
            }
            else if (CoordinatorSetup == ComponentSetupType.MOCK && ConvertSOToJSON)
            {
                SaveIndividualsToJSON();
            }

            Evaluator evaluator;
            Task<CoordinatorEvaluationResult> evaluationResultTask = null;

            RatingSystem ratingSystem;
            CompetitionOrganization competitionOrganizator;

            switch (EvaluatorType)
            {
                case EvaluatiorType.Simple:
                    // Simple evaluation
                    evaluator = new SimpleEvaluator();
                    evaluationResultTask = evaluator.ExecuteEvaluation(evalRequestData, Individuals);
                    break;
                case EvaluatiorType.Competition:
                    // Tournament evaluator
                    getTeamOrganizator();
                    competitionOrganizator = getCompetitionOrganizator(Individuals);

                    evaluator = new CompetitionEvaluator(competitionOrganizator);
                    // TODO Include LastEvalPopRatings?
                    evaluationResultTask = evaluator.ExecuteEvaluation(evalRequestData, Individuals);
                    break;
                case EvaluatiorType.Rating:
                    // Rating evaluator
                    getTeamOrganizator();
                    ratingSystem = getRatingSystem();
                    competitionOrganizator = getCompetitionOrganizator(Individuals);

                    evaluator = new RatingEvaluator(ratingSystem, competitionOrganizator);
                    ratingSystem.DefinePlayers(Individuals, ratingSystem.PrepareLastEvalPopRatings(evalRequestData.LastEvalIndividualFitnesses));
                    evaluationResultTask = evaluator.ExecuteEvaluation(evalRequestData, Individuals);
                    break;
                default:
                    throw new Exception("Invalid EvaluatorType");
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
            }

            System.IO.Stream body = context.Request.InputStream;
            Encoding encoding = context.Request.ContentEncoding;
            System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);

            try
            {
                return JsonConvert.DeserializeObject<CoordinatorEvalRequestData>(reader.ReadToEnd());
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading the request data: " + ex.Message);
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

        public void LoadIndividualsFromJSON(int evalRangeStart, int evalRangeEnd)
        {
            if (IndividualsSourceJSON == null || IndividualsSourceJSON.Length == 0 || IndividualsSourceSO == null || IndividualsSourceSO.Length == 0)
            {
                throw new Exception("IndividualsSourceJSON or IndividualsSourceSO are not defined");
            }

            // Loading individuals from JSON files
            Individuals = UnityAssetParser.ParseIndividualsFromFolder(IndividualsSourceJSON, evalRangeStart, evalRangeEnd);

            // Save individuals to Scriptable Objects if in Editor mode
            UnityAssetParser.SaveSOIndividualsToSO(Individuals, IndividualsSourceSO);
        }

        public void SaveIndividualsToJSON()
        {
            if (IndividualsSourceJSON == null || IndividualsSourceJSON.Length == 0)
            {
                throw new Exception("IndividualsSourceJSON or IndividualsSourceSO are not defined");
            }

            UnityAssetParser.SaveSOIndividualsToJSON(Individuals, IndividualsSourceJSON);
        }

        private RatingSystem getRatingSystem()
        {
            switch (RatingSystemType)
            {
                case RatingSystemType.TrueSkill:
                    return new TrueSkillRatingSystem();
                case RatingSystemType.Glicko2:
                    return new Glicko2RatingSystem();
                case RatingSystemType.Elo:
                    return new EloRatingSystem();
                default:
                    MqttNetLogger.Log("Invalid rating system type", MqttNetLogType.Error);
                    return null;
            }
        }

        private void getTeamOrganizator()
        {
            if (TeamOrganizator != null) return;

            TeamOrganizator = GetComponent<CompetitionTeamOrganizator>();
            if (TeamOrganizator == null)
            {
                throw new Exception("CompetitionTeamOrganizator is not defined");
            }
        }

        private CompetitionOrganization getCompetitionOrganizator(Individual[] individuals)
        {
            switch (CompetitionOrganizationType)
            {
                case CompetitionOrganizationType.RoundRobin:
                    return new RoundRobinTournament(TeamOrganizator, individuals, CreateNewTeamsEachRound, CompetitionRounds);
                case CompetitionOrganizationType.SwissSystem:
                    return new SwissSystemTournament(TeamOrganizator, individuals, CreateNewTeamsEachRound, CompetitionRounds);
                case CompetitionOrganizationType.LastVsAll:
                    return new LastVsAllTournament(TeamOrganizator, individuals, CreateNewTeamsEachRound, CompetitionRounds);
                case CompetitionOrganizationType.SingleElimination:
                    return new SingleEliminationTournament(TeamOrganizator, individuals, CreateNewTeamsEachRound, CompetitionRounds);
                case CompetitionOrganizationType.DoubleElimination:
                    return new DoubleEliminationTournament(TeamOrganizator, individuals, CreateNewTeamsEachRound, CompetitionRounds);
                case CompetitionOrganizationType.KRandomOpponents:
                    return new KRandomOpponentsTournament(TeamOrganizator, individuals, CreateNewTeamsEachRound, CompetitionRounds);
                case CompetitionOrganizationType.SimilarStrengthOpponentSelection:
                    return new SimilarStrengthOpponentSelection(TeamOrganizator, individuals, CreateNewTeamsEachRound, CompetitionRounds);
                default:
                    Debug.LogError("Invalid competition organization type");
                    return null;
            }
        }

        void OnDestroy()
        {
            if (Listener != null)
            {
                StopListener();
            }
        }
    }

    public class CoordinatorEvalRequestData
    {
        public string[] EvalEnvInstances { get; set; }
        public int? EvalRangeStart { get; set; }
        public int? EvalRangeEnd { get; set; }
        public IndividualFitness[] LastEvalIndividualFitnesses { get; set; } // TODO implement

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
        public FinalIndividualFitness[] IndividualFitnesses { get; set; }
    }
}