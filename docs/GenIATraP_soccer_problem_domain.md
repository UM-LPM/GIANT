# Soccer problem domain

The **Soccer problem domain** is a **multi-agent environment** based on a simplified version of soccer, inspired by the [Soccer Twos](https://github.com/Unity-Technologies/ml-agents/blob/develop/docs/Learning-Environment-Examples.md#soccer-twos) problem domain implemented in [ML-Agents](https://github.com/Unity-Technologies/ml-agents/tree/develop). It features two teams competing against each other in a dynamic environment. The main objective of the agents is to cooperate with their teammates and defeat the opposing team, either by scoring goals or preventing the other team from scoring. This problem domain allows for the implementation and testing of multi-agent systems, team strategies, and various agent control schemes.

<center><img src="/docs/images/soccerProblemDomain/soccer_problem_domain_main.png" alt="Soccer" width="600"/></center>
## Objective
The **Soccer problem domain** aims to simulate a soccer match where agents cooperate to score goals against an opposing team. Agents must employ effective strategies, including ball control, positioning, and teamwork, to outmaneuver their opponents. The simulation challenges agents to make quick decisions and adapt to their teammates' actions, the opponents’ strategies, and the evolving game state.
## Environment Details
The environment consists of a **soccer field** with two **goals**, a **ball**, and two **opposing teams**. The **soccer field** is bounded by defined edges, and agents must navigate this space while executing actions such as running, kicking the ball, and coordinating with teammates. The environment is divided into different sectors, which track the agent’s exploration progress.

<center><img src="/docs/images/soccerProblemDomain/soccer_problem_domain_exploration_progress.png" alt="Soccer" width="600"/></center>

### Goals
Each team has a designated goal area. A team scores when the ball enters the opponent's goal (highlighted in blue in the image below). Upon scoring, the agents and the ball are reset to their initial positions.

<center><img src="/docs/images/soccerProblemDomain/soccer_problem_domain_goal.png" alt="Soccer" width="400"/></center>

### Soccer Ball
The **soccer ball** is the focal point of the game, requiring agents to learn efficient strategies for passing, intercepting, and shooting toward the opponent’s goal.

<center><img src="/docs/images/soccerProblemDomain/soccer_problem_domain_soccerball.png" alt="Soccer" width="200"/></center>

### Agents
The simulation features two opposing teams, each consisting of two agents. Agents are responsible for scoring as many goals as possible. An agent can strike the ball upon collision, with the impact force determined by its velocity. Additionally, hitting the ball at different angles influences its trajectory.

<center><img src="/docs/images/soccerProblemDomain/soccer_problem_domain_agents.png" alt="Soccer" width="400"/></center>
#### Environment Perception
Agents perceives their surroundings using a **Ray Perception Sensor**. During configuration, a specific number of rays and their respective angles are defined. Basic raycasting can be replaced with **spherical rays**, which emit in a predefined direction. If a ray intersects with a detectable object (ray color is set to green), it returns information regarding the object and additional detection details. If no object is detected, the ray perception sensor returns an empty result. Rays can detect other agents, ball, goals and walls.

<center><img src="/docs/images/soccerProblemDomain/soccer_problem_domain_object_detection.png" alt="Soccer" width="600"/></center>
## Key Features:
- **Two Teams**: The simulation supports two teams of agents, with each team having its own set of controlled agents.
- **Ball Handling**: Agents are required to interact with the ball to move it across the field and score goals.
- **Goal Scoring**: The primary objective is to score goals against the opposing team.
- **Agent Actions**: Agents can perform actions such as running, and kicking, all of which contribute to the game’s progression.
## Fitness Evaluation
The fitness of agents in the Soccer problem domain is determined by their performance in the soccer game, with key metrics focusing on teamwork, ball control, and scoring ability. Fitness evaluation is based on the agent's ability to contribute to the team's success and outplay the opponent.
### Key Fitness Metrics:
- Goals Scored: Positive reward for each goal scored by the agent's team.
- Goals Received: Penalty for allowing the opposing team to score.
- Successful Passes: Positive reward for successful passes that move the ball forward or to a teammate.
- Distance Covered: Reward based on the distance the agent covers during the match, incentivizing active participation.

## Problem Domain Configuration File
Here is a sample configuration file for the Soccer problem domain:

```json5
{
    "AutoStart": true, // Automatically starts the simulation without requiring manual initiation.
    "ProblemDomain": "Soccer", // Specifies the problem domain as "Soccer".
    "CoordinatorURI": "http://localhost:4000/", // URI for the coordinator that manages the simulation's communication.
    "IndividualsSourceJSON": "...\\GenIATraP\\Assets\\Resources\\JSONs\\Soccer\\", // Path to JSON files containing agent definitions for the Soccer problem.
    "IndividualsSourceSO": "Assets\\Resources\\SOs\\Soccer", // Path to ScriptableObject files for agent definitions.
    "ConvertSOToJSON": false, // Whether to convert ScriptableObjects into JSON files; false means SO files are used directly.
    "StartCommunicatorURI": "http://localhost:4444/", // URI for starting the communication interface.
    "TimeScale": 2, // Time scale multiplier for the simulation, affecting how fast time progresses.
    "FixedTimeStep": 0.02, // The fixed time step for physics updates in the simulation.
    "RerunTimes": 1, // Number of times the simulation should be rerun with the same setup.
    "InitialSeed": 963852, // Initial seed for random number generation to ensure reproducibility.
    "Render": true, // Whether the environment should be rendered visually during simulation.
    "RandomSeedMode": 1, // Mode for random seed generation.
    "SimulationSteps": 2000, // Maximum number of steps for the simulation to run.
    "SimulationTime": 0, // Maximum simulation time in seconds.
    "IncludeEncapsulatedNodesToFreqCount": false, // Whether to include encapsulated behavior tree nodes in frequency count analysis.
    
    "EvaluatorType": 1, // Type of evaluator used for assessing agent performance.
    "RatingSystemType": -1, // Rating system type (e.g., a TrueSkill system or custom system).
    "TournamentOrganizationType": 3, // Type of tournament organization, affecting how agents are grouped for matches.
    "TournamentRounds": -1, // Number of tournament rounds (negative values might imply no limit).
    "SwapTournamentMatchTeams": false, // Whether to swap teams in the tournament after each match.

    "GameScenarios": [
        {
            "GameSceneName": "SoccerGameScene" // Specifies the name of the game scene used in this simulation.
        }
    ],

    "AgentScenarios": [
        {
            "AgentSceneName": "SoccerAgentScene", // Defines the agent scene for this simulation.
            "GameScenarios": [
                {
                    "GameSceneName": "SoccerGameScene" // Game scene in which the agents will participate.
                }
            ]
        }
    ],

    "ProblemConfiguration": {
        "DecisionRequestInterval": 2, // Time interval in steps for when agents request decisions.
        "ArenaSizeX": 30, // The length of the soccer field along the X-axis.
        "ArenaSizeY": 15, // The width of the soccer field along the Y-axis.
        "ArenaSizeZ": 0.5, // Height of the soccer field.
        "ArenaOffset": 4.0, // Offset for the positioning of the agents in the arena.
        "AgentRunSpeed": 1, // Speed at which agents run on the field.
        "AgentRotationSpeed": 90, // Rotation speed of the agents.
        "KickPower": 1000, // Power with which agents can kick the ball.
        "GameScenarioType": 1, // Specifies the type of game scenario.
        "PassTolerance": 0.3, // Tolerance level for successful passes between agents.
        "MaxGoals": 10, // Maximum number of goals allowed in the game.
        "RayHitObjectDetectionType": 1 // Type of object detection used.
    },

    "FitnessValues": { 
        "SectorExploration": -20, // Reward for exploring sectors of the field.
        "GoalsScored": -500, // Reward for scoring goals.
        "GoalsReceived": 250, // Penalty for goals received by the team.
        "AutoGoals": 250, // Penalty for automatic goals.
        "PassesToOponentGoal": -50, // Reward for making passes that lead the ball towards the opponent's goal.
        "PassesToOwnGoal": -30, // Reward for passes leading to one's own goal.
        "Passes": -40 // Reward for making a pass.
    }
}
```

