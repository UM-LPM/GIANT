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

#### 1. Agent Component

The **Agent Component** serves as the central entity that encapsulates all key functionalities of an agent. It manages critical subsystems, including the **Agent Controller**, **Action Buffer**, **Agent Fitness**, and other problem-specific attributes.

- **Action Buffer**: Stores the current set of actions that the agent must execute in the next simulation step. This buffer updates whenever a new decision request is issued. It supports two types of actions:
    - **Discrete Actions**: Used for actions with distinct choices such as **shooting**, **jumping**, or **activating abilities**.
    - **Continuous Actions**: Used for actions that require smooth variations, such as **steering**, **acceleration**, or **aiming precision**.
- **Agent Controller**: Defines the logic that governs the agent’s behavior. At each decision step, the **Agent Controller** determines and fills the **Action Buffer** based on the AI model's output and the agent’s current observations from the environment. The controller can be implemented using various AI techniques such as **Neural Networks, Behavior Trees, Finite State Machines (FSMs), or Custom Scripts**.
- **Agent Fitness**: Tracks all **rewards** and **penalties** accumulated during a simulation episode. The fitness function is domain-specific, meaning each problem defines its own reward structure to guide agent behavior. This component is essential for evaluating an agent’s performance and optimizing its decision-making process.

#### 2. Sensors

Sensors define how an agent perceives its environment. The platform currently supports multiple types of sensors, each optimized for different perception methods:

- **Ray Sensor**: Uses raycasting to detect objects in the agent’s surroundings. Rays originate from the agent’s position and extend in predefined directions. Various configurable parameters, such as **ray length, detection radius, layer masks**, and **hit sensitivity**, allow for fine-tuned detection capabilities.
- **Grid Sensor**: Detects objects in the environment using a grid-based approach. The grid structure surrounds the agent, and each cell detects the presence of objects. Grid and cell sizes can be adjusted to optimize for different use cases, balancing precision and computational efficiency.
- **Camera Sensor**: Captures a 2D image of the agent’s surroundings using an in-game camera. This image can be processed in **grayscale** or **color**, depending on the problem’s requirements. The agent’s AI model can use pixel data as input to make decisions.
- **Vision Sensor**: Simulates **human-like vision** by capturing all visible objects within a predefined **field of view (FOV)**. The **vision range and width** are adjustable, enabling realistic simulations of varying vision capabilities. This sensor is particularly useful for evaluating how vision quality affects agent performance in different scenarios.

#### 3. Model (2D/3D)

The **Model** defines the **physical representation** of an agent within the simulation. This influences both how the agent moves and how other agents perceive it. A complete model typically consists of:

- **Mesh (2D/3D)**: The agent’s graphical representation in the game world.
- **Collider**: Defines the agent’s collision boundaries, ensuring accurate physics interactions with the environment. The collider type (e.g., box, sphere, capsule) depends on the agent’s shape and movement mechanics.

#### 4. Problem-Specific Components

Problem-specific components extend the base agent functionalities to support unique **game logic and mechanics**. These components vary based on the evaluation environment’s problem domain. For example:

- In the **RoboStrike** problem domain, an agent may include:
    - **Health System**: Tracks remaining health points.
    - **Ammo Management**: Controls available ammunition for attacks.
    - **Shield System**: Determines active defensive capabilities.

Each problem domain defines its own specialized components to support complex interactions and decision-making processes.
