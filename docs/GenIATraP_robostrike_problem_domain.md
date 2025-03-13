# Robostrike Problem Domain

The **Robostrike problem domain** is a dynamic, **multi-agent** environment where robots compete in a combat environment. Agents, in the form of robots, are equipped with various abilities like shooting, moving, and interacting with the environment. The **goal** is to destroy enemy robots while avoiding damage from opponents and environmental hazards. This problem domain is ideal for evaluating agents that need to exhibit strategic thinking, resource management, and tactical combat decision-making in a real-time environment.

<center><img src="/docs/images/robostrikeProblemDomain/robostrike_problem_domain_main.png" alt="Robostrike" width="600"/></center>
## Objective
The primary objective for agents in the Robostrike domain is to survive and defeat enemy robots by managing their health, ammunition, and positioning in the arena. Agents should aim to be the last one standing while strategically using their resources to eliminate opponents. The problem domain tests agents' ability to prioritize survival, make tactical decisions under pressure, and react quickly to changing circumstances.
## Environment Details
The Robostrike environment consists of a battlefield where agents (tanks) compete in combat. Core elements that compose the environment are described bellow.
### Environment
The combat environment is a bounded 2D space where agents can move, hide, and interact. The layout may include obstacles that provide strategic positioning advantages. Agents can only move within the designated **green surface**, which defines the battle arena. The environment is divided into different sectors, which track the agent’s exploration progress.

<center><img src="/docs/images/robostrikeProblemDomain/robostrike_problem_domain_environment.png" alt="Robostrike" width="400"/></center>
### Agent
The simulation consists of two opposing **agents (tanks)**, each tasked with destroying the opponent as many times as possible. Additional scenarios, such as **1 vs. X** tank battles, can be configured if needed. Each agent has a **status bar** displaying its health, ammunition, and shield levels. If an agent's health reaches zero, it is destroyed and subsequently respawned at either a predefined starting position or a random location.

<center><img src="/docs/images/robostrikeProblemDomain/robostrike_problem_domain_agent.png" alt="Robostrike" width="200"/>
	<img src="/docs/images/robostrikeProblemDomain/robostrike_problem_domain_agent_stats.png" alt="Robostrike" width="200"/>
	</center>

#### Projectiles
When an agent picks up an **ammo power-up**, it gains the ability to fire projectiles. Each projectile is automatically destroyed after a set duration. If a projectile successfully hits an opponent, the agent responsible for the shot is rewarded.

<center><img src="/docs/images/robostrikeProblemDomain/robostrike_problem_domain_missile.png" alt="Robostrike" width="100"/></center>

#### Environment Perception
Agents perceives their surroundings using a **Ray Perception Sensor**. During configuration, a specific number of rays and their respective angles are defined. Basic raycasting can be replaced with **spherical rays**, which emit in a predefined direction. If a ray intersects with a detectable object (ray color is set to green), it returns information regarding the object and additional detection details. If no object is detected, the ray perception sensor returns an empty result. Rays can detect other agents, power-ups, missiles, obstacles and walls.

<center><img src="/docs/images/robostrikeProblemDomain/robostrike_problem_domain_object_detection.png" alt="Robostrike" width="400"/></center>

### Power-ups
There are three types of **power-ups** available for agents to collect, each replenishing a different attribute: **health**, **ammunition**, or **shield**. The number of power-ups present in the environment is configurable via the settings file. When a power-up is collected, it respawns at a new random location within the environment.

<center><img src="/docs/images/robostrikeProblemDomain/robostrike_problem_domain_ammo.png" alt="Robostrike" width="100"/>
	<img src="/docs/images/robostrikeProblemDomain/robostrike_problem_domain_health.png" alt="Robostrike" width="100"/>
	<img src="/docs/images/robostrikeProblemDomain/robostrike_problem_domain_shield.png" alt="Robostrike" width="100"/>
	</center>
## Fitness Evaluation
Fitness evaluation in the Robostrike problem domain is designed to measure the success of an agent in achieving its objectives, focusing on strategic behavior and combat performance. Here are some key fitness components:

- **RobotsKilled**: A significant reward is given for successfully eliminating enemy robots, rewarding combat effectiveness.
- Survival: A major fitness component is survival, with agents receiving a large bonus for being the last robot alive.
- **DamageTaken**: A penalty is applied for taking damage, encouraging robots to avoid unnecessary risks and be more strategic in their movements.
- **AmmoManagement**: A penalty or reward can be given based on how well the robot manages its ammo, encouraging resource conservation.
- **HealthManagement**: A penalty is applied for losing health quickly, promoting careful positioning and evasion.
- **ArenaControl**: A reward for controlling central or strategic areas of the arena, where robots can have better control over the combat.
- **PowerUpCollection**: A small reward for collecting power-ups, promoting resource gathering and enhancing robot capabilities.

## Problem Domain Configuration File
The problem configuration file includes various settings that define how the simulation is executed. It specifies general parameters such as auto-start behavior, the problem domain, and coordinator communication. It also configures agent sources, allowing the system to load individuals from JSON files or ScriptableObjects, with an option to convert between these formats. The simulation behavior is controlled through time settings, including time scale, fixed time step, and random seed handling for reproducibility. It also defines tournament settings, such as the tournament organization type, rating system, and number of rounds. The problem-specific configuration details the arena size, agent movement parameters, power-up mechanics, and missile properties. Finally, a fitness function assigns rewards or penalties based on agent actions, such as exploration, power-up collection, shooting accuracy, and opponent elimination, ensuring agents are evaluated based on strategic behavior and combat effectiveness.

```json5
{
	"AutoStart": true,  // Automatically starts the simulation without manual intervention.
	"ProblemDomain": "Robostrike",  // Specifies the problem domain (RoboStrike).
	"CoordinatorURI": "http://localhost:4000/",  // URL of the central coordinator for managing simulations.
	"IndividualsSourceJSON": "...\\GenIATraP\\Assets\\Resources\\JSONs\\Robostrike\\",  // Path to JSON files storing agent configurations.
	"IndividualsSourceSO": "Assets\\Resources\\SOs\\Robostrike",  // Path to ScriptableObjects (SO) storing agent configurations.
	"ConvertSOToJSON": false,  // If true, converts ScriptableObjects to JSON format for easier processing.
	"StartCommunicatorURI": "http://localhost:4444/",  // URL for the communicator service.
	"TimeScale": 10,  // Controls the speed of simulation (higher values speed up the simulation).
	"FixedTimeStep": 0.02,  // Fixed update interval for physics calculations.
	"RerunTimes": 1,  // Number of times to repeat the same simulation run for consistency.
	"InitialSeed": 963852,  // Initial random seed for reproducibility.
	"Render": true,  // If true, renders the simulation visually; otherwise, it runs headless.
	"RandomSeedMode": 1,  // Determines how random seeds are handled
	"SimulationSteps": 3000,  // Total number of simulation steps per episode.
	"SimulationTime": 0,  // Total simulation time (overridden if set to 0).
	"IncludeEncapsulatedNodesToFreqCount": false,  // If true, includes encapsulated nodes in frequency calculations.
	"EvaluatorType": 1,  // Specifies the evaluation method for agent performance.
	"RatingSystemType": -1,  // Defines which rating system is used.
	"TournamentOrganizationType": 1,  // Defines the tournament organization method (e.g., Swiss, round-robin).
	"TournamentRounds": -1,  // Number of tournament rounds (-1 for auto-calculated rounds).
	"SwapTournamentMatchTeams": true,  // If true, swaps teams in tournaments for balanced evaluation if the evaluation environment is not simetrical.
	
	"GameScenarios": [  // Defines different game scenarios.
		{"GameSceneName": "RobostrikeGameScene"}
	],
	
	"AgentScenarios": [  // Defines different agent scenarios.
		{
			"AgentSceneName": "RobostrikeAgentScene",  // Name of the scene for agent evaluation.
			"GameScenarios": [
				{"GameSceneName": "RobostrikeGameScene"}
			]
		}
	],

	"ProblemConfiguration": {  // Defines the specific settings for the RoboStrike environment.
		"DecisionRequestInterval": 2,  // Number of steps between decision requests.
		"ArenaSizeX": 30,  // Width of the battle arena.
		"ArenaSizeY": 18,  // Height of the battle arena.
		"ArenaSizeZ": 0,  // Depth (not used in 2D environments).
		"ArenaOffset": 4.0,  // Offset to ensure proper agent spawning.
		"AgentMoveSpeed": 8,  // Movement speed of agents.
		"AgentRespawnType": 0,  // Determines how agents respawn.
		"AgentRotationSpeed": 150,  // Rotation speed of the agent's body.
		"AgentStartAmmo": 0,  // Initial ammo count for agents.
		"AgentStartHealth": 10,  // Initial health value for agents.
		"AgentStartShield": 0,  // Initial shield value for agents.
		"AgentTurrentRotationSpeed": 180,  // Rotation speed of the agent's turret.
		"AmmoBoxSpawnAmount": 2,  // Number of ammo power-ups spawned.
		"AmmoPowerUpValue": 10,  // Amount of ammo gained from a power-up.
		"DestroyMissileAfter": 1,  // Time (in seconds) before a fired missile is destroyed.
		"GameMode": 0,  // Specifies the game mode.
		"GameScenarioType": 1,  // Type of game scenario used.
		"HealthBoxSpawnAmount": 1,  // Number of health power-ups spawned.
		"HealthPowerUpValue": 5,  // Amount of health restored by a power-up.
		"MaxAmmo": 20,  // Maximum ammo capacity for an agent.
		"MaxHealth": 10,  // Maximum health value for an agent.
		"MaxShield": 10,  // Maximum shield capacity for an agent.
		"MinPowerUpDistance": 8,  // Minimum distance between spawned power-ups.
		"MinPowerUpDistanceFromAgents": 8.0,  // Minimum distance between agents and spawned power-ups.
		"MissileDamage": 2,  // Damage dealt by a missile hit.
		"MissileShootCooldown": 1,  // Cooldown time before firing another missile.
		"MissleLaunchSpeed": 20,  // Speed at which missiles are launched.
		"ShieldBoxSpawnAmount": 1,  // Number of shield power-ups spawned.
		"ShieldPowerUpValue": 5,  // Amount of shield restored by a power-up.
		"RayHitObjectDetectionType": 1  // Type of object detection for raycasts.
	},

	"FitnessValues": {  // Defines how different actions impact agent fitness.
		"SectorExploration": -5,  // Reward for exploring new sectors.
		"PowerUp_Pickup_Health": -10,  // Reward for picking up a health power-up.
		"PowerUp_Pickup_Ammo": -15,  // Reward for picking up an ammo power-up.
		"PowerUp_Pickup_Shield": -5,  // Reward for picking up a shield power-up.
		"MissilesFired": -20,  // Reward for each missile fired.
		"MissilesFiredAccuracy": -50,  // Reward to encourage accuracy.
		"SurvivalBonus": -5,  // Small reward per step for surviving (can be adjusted to encourage aggression).
		"OpponentTrackingBonus": -5,  // Reward for tracking opponents.
		"OpponentDestroyedBonus": -500,  // Large reward for successfully eliminating an opponent.
		"DamageTakenPenalty": 50  // Penalty for receiving damage.
	}
}

```
