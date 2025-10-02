using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Evaluators.RatingSystems;
using Evaluators.TournamentOrganizations;
using Evaluators;
using Base;

namespace Configuration
{

    /// <summary>
    /// Main configuration class
    /// </summary>
    [Serializable]
    public class MainConfiguration
    {

        public static JsonSerializerSettings JSON_SERIALIZATION_SETTINGS = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MaxDepth = 128
        };

        public bool AutoStart { get; set; }

        // Communicator configuration
        public string ProblemDomain { get; set; }
        public string CoordinatorURI { get; set; }
        public string StartCommunicatorURI { get; set; }
        public string IndividualsSourceJSON { get; set; }
        public string IndividualsSourceSO { get; set; }
        public bool ConvertSOToJSON { get; set; }
        public float TimeScale { get; set; }
        public float FixedTimeStep { get; set; }
        public int RerunTimes { get; set; }
        public int InitialSeed { get; set; }
        public bool Render { get; set; }
        public RandomSeedMode RandomSeedMode { get; set; }
        public int GameMode { get; set; }
        public GameScenario[] GameScenarios { get; set; }
        public AgentScenario[] AgentScenarios { get; set; }
        public bool IncludeNodeCallFrequencyCounts { get; set; }

        // Environment configuration
        public int SimulationSteps { get; set; }
        public int SimulationTime { get; set; }
        public bool IncludeEncapsulatedNodesToFreqCount { get; set; }

        // Evaluation configuration
        public EvaluatiorType EvaluatorType { get; set; }
        public RatingSystemType RatingSystemType { get; set; }
        public bool CreateNewTeamsEachRound { get; set; }
        public TournamentOrganizationType TournamentOrganizationType { get; set; }
        public int TournamentRounds { get; set; }
        public bool SwapTournamentMatchTeams { get; set; }

        // Problem Specific Configuration
        public Dictionary<string, string> ProblemConfiguration { get; set; }

        // AgentFitness configuration
        public Dictionary<string, float> FitnessValues { get; set; }

        public static void Serialize(MainConfiguration mainConfiguration, string destFilePath)
        {
            string json = JsonConvert.SerializeObject(mainConfiguration, Formatting.Indented);
            File.WriteAllText(destFilePath, json);
        }

        public static MainConfiguration Deserialize(string srcFilePath)
        {
            try
            {
                string json = File.ReadAllText(srcFilePath);
                return JsonConvert.DeserializeObject<MainConfiguration>(json);
            }
            catch (Exception e)
            {
                Debug.Log("Configuration file not found: " + e.Message);
                return null;
            }
        }
    }
}