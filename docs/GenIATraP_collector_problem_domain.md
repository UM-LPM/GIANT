# Collector Problem Domain

The Collector problem domain is a single-agent environment where an agent navigates a bounded arena to collect scattered objects. This problem is designed to test an agent’s ability to efficiently explore the environment, optimize movement, and maximize the number of collected objects within a given time frame.

## Objective
The main objective of the Collector environment is for the agent to collect as many objects as possible before the simulation ends. The agent must efficiently navigate the space while minimizing unnecessary movement and avoiding potential penalties such as collisions. This problem domain is useful for evaluating pathfinding strategies, decision-making heuristics, and reinforcement learning approaches.

## Environment Details
- Arena – Bounded environment where objects spawn randomly.
- Collectibles – Objects that appear at different locations and must be collected by the agent.
- Agent Movement – The agent can move freely within the arena to reach and collect objects.
- Simulation Duration – The environment runs for a fixed number of simulation steps.
- Collision Handling – The agent may receive penalties for colliding with walls or other obstacles (if enabled).

## Fitness Evaluation
The agent's performance is assessed using the following fitness metrics:
- Objects Collected (+) – The agent receives a positive reward for successfully picking up collectibles.
- Movement Efficiency (-) – Unnecessary movements may result in a small penalty to encourage optimal navigation.
- Collision Penalty (-) – If enabled, colliding with obstacles results in a fitness reduction.

## Problem Domain Configuration File
Below is an example configuration file for the Collector environment:

```json5
{
    "AutoStart": true, // Automatically starts the simulation without needing manual initiation.
    "ProblemDomain": "Collector", // Specifies that this configuration is for the "Collector" problem domain.
    "CoordinatorURI": "http://localhost:4000/", // URI for the coordinator that manages the communication during simulation.
    "IndividualsSourceJSON": "C:\\Users\\marko\\UnityProjects\\GenIATraP_refactor\\GeneralTrainingEnvironmentForMAS\\GenIATraP\\Assets\\Resources\\JSONs\\Collector\\", // Path to the JSON files containing agent definitions for this problem domain.
    "IndividualsSourceSO": "Assets\\Resources\\SOs\\Collector", // Path to the ScriptableObject files for agent definitions.
    "ConvertSOToJSON": false, // Whether to convert ScriptableObjects to JSON format; false means SO files are used directly.
    "StartCommunicatorURI": "http://localhost:4444/", // URI for starting the communication interface during the simulation.
    "TimeScale": 5, // Multiplier for the simulation's time scale, which affects how fast time progresses.
    "FixedTimeStep": 0.02, // The time step for physics updates in the simulation.
    "RerunTimes": 1, // Number of times the simulation should be rerun with the same setup.
    "InitialSeed": 963852, // Initial random seed used to ensure reproducibility of the simulation.
    "Render": true, // Whether the environment should be rendered visually during simulation.
    "RandomSeedMode": 1, // Mode for handling random seeds (e.g., fixed vs. randomized).
    "SimulationSteps": 10000, // Maximum number of steps for the simulation to run.
    "SimulationTime": 0, // Maximum simulation time in seconds (0 means the time is controlled by steps).
    "IncludeEncapsulatedNodesToFreqCount": false, // Whether to include encapsulated behavior tree nodes in frequency count analysis.

    "GameScenarios": [
        {
            "GameSceneName": "CollectorGameScene1" // The name of the game scene used in this simulation.
        }
    ],

    "AgentScenarios": [
        {
            "AgentSceneName": "CollectorAgentScene", // The scene used for the agent's setup within the simulation.
            "GameScenarios": [
                {
                    "GameSceneName": "CollectorGameScene1" // The game scene in which the agent will participate.
                }
            ]
        }
    ],

    "ProblemConfiguration": {
        "DecisionRequestInterval": 2, // Interval in steps for when the agent makes decisions.
        "ArenaRadius": 18, // The radius of the arena where the agent spawnes.
        "ArenaOffset": 0.0, // Positional offset for the agents in the arena.
        "AgentMoveSpeed": 10, // Movement speed of the agent in the arena.
        "AgentRotationSpeed": 180, // How fast the agent can rotate in degrees per second.
        "GameMode": 1, // Defines the mode of the game (single target pick-up, multiple targets pick-up).
        "StartNumberOfTargets": 1, // The initial number of targets spawned in the environment.
        "TargetMinDistanceFromAgents": 10, // The minimum distance between the agent and the targets.
        "TargetToTargetMinDistance": 10 // The minimum distance between targets to avoid overcrowding.
    },

    "FitnessValues": { 
        "AgentPickedTarget": -50, // Reward for the agent picking up a target (encourages exploration).
        "AgentExploredSector": -3, // Reward for the agent exploring a new sector.
        "AgentReExploredSector": -1, // Minor reward for revisiting already explored sectors.
        "AgentNearTarget": -2, // Reward for the agent being near a target without collecting it.
        "AgentSpottedTarget": -2, // Reward for spotting a target but not interacting with it.
        "AgentTouchedStaticObject": 5, // Reward for interacting with a static object in the environment.
        "AgentNotMoved": 0, // Penalty for the agent remaining stationary.
        "TimePassedPenalty": 0 // Penalty for time passing (may be adjusted for efficiency).
    }
}
```
