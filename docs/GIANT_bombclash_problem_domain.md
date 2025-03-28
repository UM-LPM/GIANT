# BombClash Problem Domain
The **BombClash problem domain** is a **multi-agent** environment, inspired by the [Bomberland multi-agent AI competition](https://github.com/CoderOneHQ/bomberland), where agents interact within a maze-like environment. The agents' primary objective is to place bombs strategically, destroy obstacles, avoid damage from explosions, and eliminate or outlast other agents. This problem domain involves handling various game mechanics such as power-ups, health management, bomb placement, and agent movement. It is typically used to train agents in decision-making and survival tactics in a dynamic and competitive environment.

<center><img src="/docs/images/bombClashProblemDomain/bombClash_problem_domain_main.png" alt="BombClash" width="600"/></center>

## Objective
The main objective for agents in the BombClash domain is to navigate the environment, avoid being destroyed by explosions, and outlast opponents. Agents must make decisions on when to place bombs, how to avoid their own bombs, and how to manage power-ups to enhance their chances of survival. The problem domain rewards strategic actions like bomb placement, surviving, and collecting power-ups while penalizing self-destructive behaviors, such as being hit by bombs or placing bombs in ineffective positions.

## Environment Details
The environment consists of a **grid-based arena** where agents are positioned in predefined locations. The arena contains three types of blocks:

- **Walkable** blocks, which agents can traverse freely.
- **Destructible** blocks, which can be destroyed by explosions.
- **Indestructible** blocks, which are permanent obstacles that must be avoided.

### Agent
 Agents can move within the arena, avoid explosions, place bombs, and collect power-ups. Each agent starts with a predefined amount of health, a limited number of bombs, and a base movement speed.

<center><img src="/docs/images/bombClashProblemDomain/bombClash_problem_domain_agent.png" alt="BombClash" width="100"/></center>

#### Environment Perception
Agents perceives their surroundings using **Grid Sensor**. During configuration, **grid size** and **cell size** are defined. If a cell intersects with a detectable object (grid cell color is set to red), it returns information regarding the object and additional detection details. If no object is detected, grid sensor returns an empty result for that cell. Grid cells can detect other agents, power-ups, bombs, and blocks (destructible and indestructible).

<center><img src="/docs/images/bombClashProblemDomain/bombClash_problem_domain_object_detection.png" alt="BombClash" width="400"/></center>

### Bomb
Agents can place **bombs**, which detonate after a set period, damaging nearby blocks, opponents, and even the agent who placed them. If an agent collides with a bomb while moving, the bomb will be pushed in the direction of the agent’s movement. The blast radius of a bomb depends on the placing agent’s **blast radius** property.

<center><img src="/docs/images/bombClashProblemDomain/bombClash_problem_domain_object_bomb.png" alt="BombClash" width="100"/></center>

### Explosion
**Explosions** affect the surrounding area within the bomb’s **blast radius**. They can destroy destructible blocks, damage agents, and trigger chain reactions if other bombs are within range.

<center><img src="/docs/images/bombClashProblemDomain/bombClash_problem_domain_object_explosion.png" alt="BombClash" width="300"/></center>

### Blocks
The simulation environment contains three types of blocks:
- **Destructible Blocks** – Obstacles that can be destroyed by explosions (first image).
- **Indestructible Blocks** – Permanent barriers that cannot be destroyed and must be avoided (second image).
- **Walkable Blocks** – Open spaces where agents can move freely (third image).

<center><img src="/docs/images/bombClashProblemDomain/bombClash_problem_domain_object_destructible_block.png" alt="BombClash" width="100"/>
<img src="/docs/images/bombClashProblemDomain/bombClash_problem_domain_object_border_block.png" alt="BombClash" width="100"/>
<img src="/docs/images/bombClashProblemDomain/bombClash_problem_domain_object_walkable_block.png" alt="BombClash" width="100"/></center>

### Power-ups
The environment contains randomly spawned **power-ups** that enhance an agent’s abilities, such as:
- Increasing the maximum number of bombs an agent can place (first image).
- Expanding the explosion blast radius (second image).
- Boosting movement speed (third image).

<center><img src="/docs/images/bombClashProblemDomain/bombClash_problem_domain_power_up_bomb_increase.png" alt="BombClash" width="100"/>
<img src="/docs/images/bombClashProblemDomain/bombClash_problem_domain_power_up_damage_increase.png" alt="BombClash" width="100"/>
<img src="/docs/images/bombClashProblemDomain/bombClash_problem_domain_power_up_speed_increase.png" alt="BombClash" width="100"/>
</center>

## Fitness Evaluation
Fitness evaluation in the BombClash problem domain is used to measure an agent's performance in the simulation. The fitness values are based on the agent's behavior and achievements throughout the game. Here are some key fitness components:

- **SectorExploration**: A penalty is applied for exploring new sectors of the arena.
- **BombsPlaced**: A small penalty is applied for placing bombs, encouraging strategic placement.
- **BlockDestroyed**: A penalty is imposed for destroying blocks, as it can create unnecessary chaos or block movement.
- **PowerUpsCollected**: A small penalty is given for collecting power-ups, ensuring agents focus on survival and strategic actions.
- **BombsHitAgent**: A penalty is given if an agent is hit by their own bomb, promoting careful decision-making.
- **AgentsKilled**: A significant penalty is applied for killing other agents, emphasizing the importance of self-preservation.
- **AgentDeath**: A penalty is given when an agent dies, reinforcing the goal of survival.
- **SurvivalBonus**: A small penalty is awarded for simply surviving without taking substantial action, rewarding agents who engage with the environment rather than hiding.

The overall fitness score is a sum of these factors, with higher penalties for harmful actions (like dying or being hit by bombs) and smaller penalties for less impactful behaviors.

## Problem Domain Configuration File
The BombClash problem domain configuration file contains parameters that control how the environment behaves, how agents interact with it, and how their actions are evaluated. Below is a breakdown of key sections within the configuration file:

```json5
{
    "AutoStart": true, // Automatically starts the simulation without requiring manual initiation.
    "ProblemDomain": "BombClash", // Specifies the problem domain as "BombClash".
    "CoordinatorURI": "http://localhost:4000/", // URI for the coordinator managing the communication for the simulation.
    "IndividualsSourceJSON": "...\\GIANT\\Assets\\Resources\\JSONs\\BombClash\\", // Path to JSON files containing agent definitions for the BombClash problem.
    "IndividualsSourceSO": "Assets\\Resources\\SOs\\BombClash", // Path to ScriptableObject files for agent definitions.
    "ConvertSOToJSON": false, // Whether to convert ScriptableObjects into JSON files.
    "StartCommunicatorURI": "http://localhost:4444/", // URI for the communication interface for starting the simulation.
    "TimeScale": 2, // Time scale multiplier, affecting the speed at which the simulation progresses.
    "FixedTimeStep": 0.02, // Fixed time step for physics updates in the simulation.
    "RerunTimes": 1, // Number of times the simulation should be rerun with the same setup.
    "InitialSeed": 963852, // Initial random seed for reproducibility of the simulation.
    "Render": true, // Whether the environment should be rendered visually during the simulation.
    "RandomSeedMode": 1, // Mode for random seed generation (e.g., fixed vs. randomized).
    "SimulationSteps": 2000, // Maximum number of steps the simulation will run.
    "SimulationTime": 0, // Maximum simulation time in seconds (0 means time controlled by steps).
    "IncludeEncapsulatedNodesToFreqCount": false, // Whether to include encapsulated behavior tree nodes in frequency count analysis.
    
    "EvaluatorType": 1, // Type of evaluator used to assess agent performance.
    "RatingSystemType": -1, // Rating system type (e.g., TrueSkill or custom system).
    "TournamentOrganizationType": 3, // Type of tournament organization (affects how agents are grouped for matches).
    "TournamentRounds": -1, // Number of rounds for the tournament.
    "SwapTournamentMatchTeams": false, // Whether to swap teams in tournament matches after each round.

    "GameScenarios": [
        {
            "GameSceneName": "BombClashGameScene" // Specifies the name of the game scene used in this simulation.
        }
    ],

    "AgentScenarios": [
        {
            "AgentSceneName": "BombClashAgentScene", // Defines the agent scene for this simulation.
            "GameScenarios": [
                {
                    "GameSceneName": "BombClashGameScene" // Game scene where the agents will perform actions.
                }
            ]
        }
    ],

    "ProblemConfiguration": {
        "DecisionRequestInterval": 2, // Time interval in steps for when agents request decisions.
        "AgentStartMoveSpeed": 5, // Starting speed at which agents can move in the game.
        "StartAgentBombAmount": 1, // Starting number of bombs each agent begins with.
        "StartExplosionRadius": 1, // Initial explosion radius of bombs placed by agents.
        "StartHealth": 2, // Starting health points of agents (indicates how many hits they can take).
        "ExplosionDamageCooldown": 1, // Time in seconds between consecutive explosions affecting an agent.
        "DestructibleDestructionTime": 1, // Time in seconds for destructible blocks to be destroyed after being hit.
        "PowerUpSpawnChance": 0.4, // Probability (0.0 to 1.0) that power-ups will spawn in the environment.
        "BombFuseTime": 3.0, // Duration in seconds for bombs to fuse before exploding.
        "ExplosionDuration": 1.0 // Duration in seconds of bomb explosions.
    },

    "FitnessValues": {
        "SectorExploration": -20, // Reward for exploring new sectors of the environment.
        "BombsPlaced": -30, // Reward for placing bombs.
        "BlockDestroyed": -50, // Reward for destroying blocks in the environment.
        "PowerUpsCollected": -20, // Reward for collecting power-ups.
        "BombsHitAgent": -100, // Reward for an agent hitting other agents with its bomb.
        "AgentsKilled": -500, // Reward for an agent killing another agent.
        "AgentHitByBombs": 0, // Penalty or penalty for agents being hit by bombs.
        "AgentHitByOwnBombs": 0, // Penalty for an agent hitting itself with its own bomb.
        "AgentDeath": 0, // Penalty for an agent's death.
        "SurvivalBonus": -100, // Reward for surviving.
        "LastSurvivalBonus": -100 // Reward for surviving to the last moment.
    }
}
```
