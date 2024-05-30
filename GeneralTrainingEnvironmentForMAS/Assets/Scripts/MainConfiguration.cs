using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// Main configuration class for the experiment
/// </summary>
[Serializable]
public class MainConfiguration {
    public bool AutoStart { get; set; }

    // Communicator configuration
    public string ProblemDomain { get; set; }
    public string StartURI { get; set; }
    public string BtSource { get; set; }
    public float TimeScale { get; set; }
    public float FixedTimeStep { get; set; }
    public int RerunTimes { get; set; }
    public int InitialSeed { get; set; }
    public bool Render { get; set; }
    public RandomSeedMode RandomSeedMode { get; set; }
    public int GameMode { get; set; }
    public GameScenario[] GameScenarios { get; set; }
    public AgentScenario[] AgentScenarios { get; set; }

    // Environment configuration
    public int SimulationSteps { get; set; }
    public int SimulationTime { get; set; }
    
    // Problem Specific Configuration
    public Dictionary<string, string> ProblemConfiguration { get; set; }

    // Fitness configuration
    public Dictionary<string, float> FitnessValues { get; set;}

    /*
    public FitnessStatisticType FitnessStatisticType { get; set; }

    // Problem specific configuration
    public Dictionary<string, string> ProblemSpecificConfiguration { get; set; }*/

    public static void Serialize(MainConfiguration mainConfiguration, string destFilePath) {
        string json = JsonConvert.SerializeObject(mainConfiguration, Formatting.Indented);
        System.IO.File.WriteAllText(destFilePath, json);
    }

    public static MainConfiguration Deserialize(string srcFilePath)
    {
        try
        {
            string json = System.IO.File.ReadAllText(srcFilePath);
            return JsonConvert.DeserializeObject<MainConfiguration>(json);
        }
        catch (Exception e)
        {
            Debug.Log("Configuration file not found: " + e.Message);
            return null;
        }
    }
}