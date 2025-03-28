# Collector Problem Domain

The **Collector problem domain** is a **single-agent environment**, inspired by [ML-Agents' Food Collector](https://github.com/Unity-Technologies/ml-agents/blob/develop/docs/Learning-Environment-Examples.md#food-collector), where an agent navigates a bounded arena to collect scattered objects. This problem is designed to test an agent’s ability to efficiently explore the environment, optimize movement, and maximize the number of collected objects within a given time frame.

<center><img src="/docs/images/collectorProblemDomain/collector_problem_domain_main.png" alt="Collector" width="600"/></center>
## Objective
The main objective of the Collector environment is for the agent to collect as many objects as possible before the simulation ends. The agent must efficiently navigate the space while minimizing unnecessary movement and avoiding potential penalties such as collisions. This problem domain is useful for evaluating pathfinding strategies, decision-making heuristics, and reinforcement learning approaches.

## Environment Details

The environment consists of an **agent**, **crates** (obstacles), and a **target**. The agent's movement is restricted by the arena’s boundaries (as shown in the image below). The environment is divided into different sectors, which track the agent’s exploration progress.
 
<center><img src="/docs/images/collectorProblemDomain/collector_problem_domain_exploration_progress.png" alt="Collector" width="600"/></center>

### Agent
The simulation features a single agent tasked with collecting as many **targets** as possible within a given time. The agent moves through the arena while avoiding obstacles and optimizing its trajectory. It can move forward, backward, and rotate left or right.
<center><img src="/docs/images/collectorProblemDomain/collector_problem_domain_agent.png" alt="Collector" width="200"/></center>

#### Environment Perception
Agents perceives their surroundings using a **Ray Perception Sensor**. During configuration, a specific number of rays and their respective angles are defined. Basic raycasting can be replaced with **spherical rays**, which emit in a predefined direction. If a ray intersects with a detectable object (ray color is set to green), it returns information regarding the object and additional detection details. If no object is detected, the ray perception sensor returns an empty result. Rays can detect obstacles, target and walls.

<center><img src="/docs/images/collectorProblemDomain/collector_problem_domain_object_detection.png" alt="Collector" width="600"/></center>

### Collectible (Target)
The **target** is the primary object that the agent must collect. After a target is collected, one of two possible outcomes occurs:

1. The target is respawned at a new random location within the arena, encouraging the agent to develop smarter search strategies.
2. The simulation ends, rewarding the agent for quickly locating and collecting the target.
<center><img src="/docs/images/collectorProblemDomain/collector_problem_domain_target.png" alt="Collector" width="200"/></center>

### Crate 
A **crate** serves as an obstacle that the agent must navigate around to reach the **target**. Increasing the number of crates adds complexity to the environment, requiring more advanced pathfinding and decision-making strategies.

<center><img src="/docs/images/collectorProblemDomain/collector_problem_domain_obstacle.png" alt="Collector" width="200"/></center>

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
    "IndividualsSourceJSON": "...\\Resources\\JSONs\\Collector\\", // Path to the JSON files containing agent definitions for this problem domain.
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
