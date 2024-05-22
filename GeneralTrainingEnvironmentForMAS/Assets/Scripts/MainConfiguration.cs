using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MainConfiguration {

    // Communicator configuration
    public string[] URIs { get; set; }
    public string BtSource { get; set; }
    GameScenario[] GameScenarios { get; set; }
    AgentScenario[] AgentScenarios { get; set; }
    public float TimeScale { get; set; }
    public int RerunTimes { get; set; }
    public int InitialSeed { get; set; }
    public RandomSeedMode RandomSeedMode { get; set; }
    public bool Render { get; set; }
    public FitnessStatisticType FitnessStatisticType { get; set; }

    // Environment configuration
    public float SimulationSteps { get; set; }
    public float SimulationTime { get; set; }

    // Problem specific configuration
    public Dictionary<string, string> ProblemSpecificConfiguration { get; set; }

    public static void Serialize(MainConfiguration mainConfiguration, string destFilePath) {

    }

    public static MainConfiguration Deserialize(string srcFilePath) {
        return null;
    }
}