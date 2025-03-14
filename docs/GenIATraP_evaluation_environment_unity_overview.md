# GenIATraP - Unity Evaluation Environment
The **Unity Evaluation Environment** within GenIATraP is designed to provide a robust and flexible platform for testing, training, and evaluating AI agents. Built on the **[Unity Game Engine](https://unity.com/)**, this environment leverages Unity’s capabilities to create dynamic, interactive, and complex simulations that are ideal for evaluating a wide range of AI behaviors. Below is a detailed breakdown of its architecture, key components, workflows, and extensibility features.

The **Unity evaluation environment** builds upon many **established functionalities** and concepts from [**ML-Agents**](https://github.com/Unity-Technologies/ml-agents/tree/develop), a Unity-based framework designed to facilitate machine learning workflows within Unity. By adapting these concepts, GenIATraP extends the capabilities of the Unity environment to provide broader support for the evaluation and testing of different AI controller types, including **Neural Networks, Behavior Trees, Finite State Machines, and more**.

The platform's integration with Unity allows for the seamless creation of complex environments and scenarios where AI agents can be trained, tested, and evaluated. Key features of this evaluation environment include:
- **Customizable Problem Domains**: Platform enables easy addition of new **problem domains**, whether for game-like environments, robotics, or other real-life simulations. Users can modify environments to suit their specific evaluation needs.
- **Flexible Parallelization**: Unity's integration supports various parallelization strategies, facilitating the **simultaneous training and evaluation** of multiple agents in diverse environments, thus optimizing computation time and efficiency.
- **Extensible Architecture**: The Unity evaluation environment is designed to be highly extensible, allowing users to **add new components**, **sensor types**, **AI controllers**, and adjust simulation parameters without significant modifications to the core system.

## Agent
In the **Unity Evaluation Environment**, the **Agent** is the fundamental entity that interacts with the environment, makes decisions based on observations, and executes actions. Each Agent represents a controlled entity whose behavior is dictated by an associated **Agent Controller** (e.g., Neural Networks, FSMs, Behavior Trees, etc.) that is part of an **Individual** definition. Agents are the building blocks of any simulation in GenIATraP and form the core of the **training and evaluation workflows**.

An **Agent** in GenIATraP performs several key **tasks** within the evaluation process:

- **Perception**: The agent collects data from the environment through various **sensors**. Sensors can provide different types of input, such as visual data (via cameras), distance measurements (via raycast sensors), or environmental readings. These inputs form the **observations** that the agent uses to decide on subsequent actions.
- **Decision-Making**: Based on the collected observations, the agent uses its **Agent Controller** to make decisions. These decisions determine the actions the agent will take in the environment (e.g., moving, interacting with objects, attacking opponents).
- **Action Execution**: The agent performs actions based on its Controller’s decisions. The available action space is defined by the environment and can be either **discrete** (e.g., moving left, right, jumping) or **continuous** (e.g., steering, accelerating). The actions affect the environment and the agent's interactions within it.
- **Reward Collection**: The agent receives **rewards** or **penalties** based on the outcomes of its actions. These rewards guide the agent’s learning process, incentivizing behaviors that lead to favorable outcomes and discouraging undesirable actions.
- **Interaction with Other Agents/Objects**: Agents can also interact with other agents (**Multi-Agent Systems**) or objects within the environment. In competitive or cooperative scenarios, agents may need to assess and respond to the actions of other agents, leading to complex **Multi-Agent dynamics**.

### Agent Architecture and Components
The structure of an **Agent** and its key components is illustrated in the image below. Each agent consists of multiple essential components that define its behavior, interaction with the environment, and role within the evaluation framework.

![Agent Scheme](/docs/images/agent_scheme.png)

#### Agent Component
The **Agent Component** serves as the central entity that encapsulates all key functionalities of an agent. It manages critical subsystems, including the **Agent Controller**, **Action Buffer**, **Agent Fitness**, and other problem-specific systems.

- **Action Buffer**: Stores the current set of actions that the agent must execute in the next simulation step. This buffer updates whenever a new decision request is issued. It supports two types of actions:
    - **Discrete Actions**: Used for actions with distinct choices such as **shooting**, **jumping**, or **activating abilities**.
    - **Continuous Actions**: Used for actions that require smooth variations, such as **steering**, **acceleration**, or **aiming precision**.
- **Agent Controller**: Defines the logic that governs the agent’s behavior. At each decision step, the **Agent Controller** determines and fills the **Action Buffer** based on the AI model's output and the agent’s current observations from the environment. The controller can be implemented using various AI techniques such as **Neural Networks, Behavior Trees, Finite State Machines (FSMs), or Custom Scripts**.
- **Agent Fitness**: Tracks all **rewards** and **penalties** accumulated during a simulation episode. The fitness function is domain-specific, meaning each problem defines its own reward structure to guide agent behavior. This component is essential for evaluating an agent’s performance and optimizing its decision-making process.

#### Sensors
Sensors define how an agent perceives its environment. The platform currently supports multiple types of sensors, each optimized for different perception methods:

- ⚡**Ray Sensor**: Uses raycasting to detect objects in the agent’s surroundings. Rays originate from the agent’s position and extend in predefined directions. Various configurable parameters, such as **ray length, detection radius, layer masks**, and **hit sensitivity**, allow for fine-tuned detection capabilities.
- 🔲**Grid Sensor**: Detects objects in the environment using a grid-based approach. The grid structure surrounds the agent, and each cell detects the presence of objects. Grid and cell sizes can be adjusted to optimize for different use cases, balancing precision and computational efficiency.
- 🎥**Camera Sensor**: Captures a 2D image of the agent’s surroundings using an in-game camera. This image can be processed in **grayscale** or **color**, depending on the problem’s requirements. The agent’s AI model can use pixel data as input to make decisions.
- 👁️**Vision Sensor**: Simulates **human-like vision** by capturing all visible objects within a predefined **field of view (FOV)**. The **vision range and width** are adjustable, enabling realistic simulations of varying vision capabilities. This sensor is particularly useful for evaluating how vision quality affects agent performance in different scenarios.

#### Model (2D/3D)
The **Model** defines the **physical representation** of an agent within the simulation. This influences both how the agent moves and how other agents perceive it. A complete model typically consists of:

- 🔳**Mesh (2D/3D)**: The agent’s graphical representation in the game world.
- 🛑**Collider**: Defines the agent’s collision boundaries, ensuring accurate physics interactions with the environment. The collider type (e.g., box, sphere, capsule) depends on the agent’s shape and movement mechanics.

#### Problem-Specific Components
Problem-specific components extend the base agent functionalities to support unique **game logic and mechanics**. These components vary based on the evaluation environment’s problem domain. For example:

- In the **RoboStrike** problem domain, an agent may include:
    - **Health System**: Tracks remaining health points.
    - **Ammo Management**: Controls available ammunition for attacks.
    - **Shield System**: Determines active defensive capabilities.

Each problem domain defines its own specialized components to support complex interactions and decision-making processes.

## Individual
An **Individual** is a fundamental **unit** that undergoes evaluation within the evaluation environment. In the context of Genetic Programming, an individual represents a single **solution** within a population. Each individual comprises an array of **Agent Controllers**, which can be either **AI Controllers** or **Manual Controllers**.

- **Homogeneous Individuals**: If an individual has only one Agent Controller, all agents controlled by this individual exhibit the **same behavior**, regardless of their specific roles or positions.
- **Heterogeneous Individuals**: If an individual consists of multiple Agent Controllers, different agents—or groups of agents—can exhibit **distinct behaviors**.

For example, in the **soccer problem domain**, a homogeneous individual would result in all players following the same decision-making strategy, whereas a heterogeneous individual allows different players (e.g., defenders, midfielders, strikers) to adopt **specialized behaviors**. This configuration enables the evolution of **more adaptive and sophisticated** solutions.

### Teams and Matches
When multiple individuals are grouped together, they form a **Team**. In a team-based setup, agents (representing individuals) must **cooperate** to achieve a common objective. In many problem domains, effective teamwork is essential, as the goal **cannot be achieved through individual effort alone**.

Once teams are defined, they can participate in a **Match**. A match consists of at least one team and each team consists of at least on individual, depending on the requirements of the specific problem domain. Currently, different teams operate **independently**, meaning that inter-team cooperation is not supported—each team functions autonomously within its own strategy.

![Individual Scheme](/docs/images/individual_scheme.png)

## Simulation
A **Simulation** is one of the core processes through which predefined matches are **evaluated** within the platform. It consists of several key components:

- **Match** – Defines which individuals will be evaluated.
- **Agent Scenario** – Specifies how agents are **spawned** and **controlled** within the environment.
- **Game Scenario** – Defines the environment in which agents will operate.
- **Environment Controller** – Manages the entire simulation process and communicates with the [**Communicator**](/docs/GenIATraP_evaluation_environment_unity_overview.md#communicator) component.

### Spawner
The platform includes two primary types of **Spawners**:

1. **Match Spawner** – Handles spawning of individuals and teams for a simulation.
2. **Additional Data Spawner** – Manages the spawning of supplementary objects (e.g., items, obstacles, or interactive elements).

Both spawners include **respawn mechanisms**, which are useful in scenarios such as resetting player positions and respawning a soccer ball when a goal is scored. The spawner system is designed to be **easily extendable**, allowing for problem domain-specific adaptations.

### Action Executor
The **Action Executor** is responsible for executing actions that control agents during the simulation. It provides the logic required for translating high-level decisions into low-level actions. When adding a new problem domain, the Action Executor can either be **extended** with new functionality or **reused** from existing implementations.

### Environment Controller
The **Environment Controller** acts as the **central orchestration component** of the simulation. It contains the following:
- The **Match** to be evaluated.
- All **simulation-specific configurations** (e.g., duration, simulation parameters, execution type).
- Required **prefabs** and assets.
- **Agents** and other dynamic elements in the simulation.
- The **problem domain logic** governing the environment.

### Simulation Workflow
0. **Pre-Init** – Evaluation request is made and the Environment Controller is loaded.
1. **Initialization** – The Environment Controller loads the required data from the **Main Configuration** and sets up the simulation.
2. **Spawning** – Agents and all necessary objects are instantiated within the environment.
3. **Execution** – The simulation progresses through **fixed updates**, where agent behaviors, interactions, and physics-based calculations (such as collisions) are processed.
4. **Termination** – The simulation continues running until the defined termination criteria are met.

![Simulation Scheme](/docs/images/simulation_scheme.png)

### Ensuring Deterministic Environment
⚠️ **Important:** A deterministic environment is a crucial aspect of an evaluation environment, as it guarantees the reproducibility of previously conducted experiments. To achieve this, new environments must be designed to meet these requirements. One key consideration is ensuring that all prefabs are unpacked before being added to the scene, as using packed prefabs can compromise the determinism of the simulation.
## Communicator

The **Communicator** class is a central component responsible for managing the communication between the Unity environment and [**Coordinator**](/docs/GenIATraP_evaluation_environment_unity_overview.md#coordinator) via an HTTP server. It also facilitates scene management and simulation execution for evaluating agent performance in different game scenarios.

### Key Responsibilities

- **Singleton Pattern Implementation:** Ensures only one instance of **Communicator** exists.
- **Configuration Management:** Reads and applies configurations from **MenuManager**.
- **HTTP Server Management:** Initializes and maintains an HTTP listener to handle incoming evaluation requests.
- **Scene Management:** Loads, runs, and unloads simulation scenes based on different modes (**LayerMode**, **GridMode**).
- **Evaluation Processing:** Handles incoming requests, sets up matches, and calculates fitness scores for agents.

### Main Components

#### 1. **Singleton Implementation**

Ensures only one instance of **Communicator** exists in one Unity program (built game) instance. If multiple instances of Unity program are used, then each instance requires it's own **Communicator**.

#### 2. **Configuration Management**

- Loads parameters from **MenuManager** to dynamically adjust the behavior of the communicator.
- Updates properties such as **CommunicatorURI**, **TimeScale**, **FixedTimeStep**, and **InitialSeed**.

#### 3. **HTTP Server Management**

- Uses **HttpListener** to listen for incoming HTTP requests.
- Runs a background thread (**ListenerThread**) to handle requests asynchronously.
- Processes evaluation requests and forwards them to Unity’s main thread using **UnityMainThreadDispatcher** class.
#### 4. **Scene Management**

Supports two modes of scene loading:

- **Layer Mode**: Uses layered scene management with predefined layer IDs.
- **Grid Mode**: Arranges scenes in a grid pattern based on **GridSize** and **GridSpacing** parameters.
#### 5. **Evaluation Processing**

- Reads match requests and sets up tournament matches.
- Waits for scene execution to complete before responding.
- Uses special coroutine to handle evaluation logic asynchronously.

## Coordinator
The **Coordinator** is a central management component responsible for orchestrating the evaluation process of individuals within the platform. It acts as the bridge between the **Web API** and the evaluation environments, ensuring that individuals are properly evaluated and their results are processed effectively.

## Core Responsibilities

The **Coordinator** has three primary functions:

1. **Handling HTTP Requests**
    - It hosts an HTTP server that listens for incoming evaluation requests.
    - It receives requests from the **Web API**), processes the request, and returns evaluation results.
    - It ensures that evaluation tasks are handled asynchronously and efficiently.
2.  **Configuring and Managing Individuals**
    - It loads individuals from JSON files or Scriptable Objects (depending on the **Environment Controller** setup).
    - It can convert and store individuals into different formats for persistence.
3. **Coordinate the Evaluation Process**
    - It receives evaluation requests containing a batch of individuals.
    - It initializes evaluators based on the chosen evaluation strategy (Simple Evaluation, Tournament Evaluation, or Rating System Evaluation).
    - It assigns individuals to evaluators and organizes tournaments if required.

The **Coordinator** interacts with several key components in the system:
- **Web API** - The **Coordinator** handles all the requests, sent from the **WebAPI** and delegates them forward to active **Communicators**. 
- **Communicator** - The **Communicator** is responsible for evaluating batches of individuals that were sent from the **Coordinator**. 
- **Evaluators** - The **Coordinator** delegates the actual evaluation process to an appropriate evaluator, based on the configured evaluation type. The possible evaluators include:
	- **Simple Evaluator**: A direct fitness evaluation without additional structure.
	- **Tournament Evaluator**: Organizes matches between individuals and ranks them based on performance.
	- **Rating Evaluator**: Uses a rating system (e.g., TrueSkill, Elo, or Glicko2) to assess individual skill levels over time.

### Configuration Options

The **Coordinator** can be configured using various parameters, allowing users to define how evaluations should be conducted. Key configurations include:

- **Evaluator Type**: Determines the evaluation strategy (Simple, Tournament, or Rating-based evaluation).
- **Rating System Type**: Selects the rating system for performance tracking (TrueSkill, Elo, Glicko2, etc.).
- **Tournament Organization Type**: Defines how matches are structured (Round Robin, Swiss System, Single Elimination, etc.).
- **Individuals Source**: Specifies the source of individual configurations (JSON files or Scriptable Objects).

### Evaluation Workflow
1. The **Web API** sends a request to the **Coordinator**, providing a set of **Individuals** to be evaluated.
2. The **Coordinator** retrieves the specified individuals from either a JSON file or Scriptable Objects.
3. The **Coordinator** initializes the appropriate **Evaluator**, which organizes the required **Matches** and distributes them to all available **Communicators**.
4. **Communicators** evaluates all matches and return the results back to the **Coordinator**.
5. Once all **Communicators** have submitted their results, the **Coordinator** compiles the data and sends the final evaluation results back to the **Web API**.

![Coordinator Scheme](/docs/images/coordinator_scheme.png)

## Summary and Next Steps

The **Unity Evaluation Environment** in **GenIATraP** is a flexible and extensible platform for testing, training, and evaluating AI agents in interactive simulations. Built on the **Unity Game Engine**, it allows for the creation of diverse problem domains, from game-like environments to real-world-inspired simulations. It supports multiple AI controllers, including **Neural Networks, Behavior Trees, and Finite State Machines**, allowing for diverse evaluation methods. Users can define custom problem domains and simulation rules, making the environment adaptable to different research and application needs. With built-in **parallel evaluation**, multiple simulations can run simultaneously, significantly improving computational efficiency. The **extensible architecture** ensures seamless integration of new sensors, AI controllers, and simulation parameters, making it a robust tool for AI research, reinforcement learning, and evolutionary algorithms.

In terms of next steps:
- Learn how to [add a new problem](/docs/GenIATraP_add_new_problem_domain.md) in **Unity Evaluation Environment**.
- Learn how [**Web API**](/docs/GenIATraP_webapi_overview.md) is connected with **Unity Evaluation Environment** and how it works.
- Learn how [**EARS framework**](/docs/GenIATraP_machine_learning_framework_ears_overview.md) is connected with **Web API** and how an optimization process can be executed.
