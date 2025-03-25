# Add New Problem Domain To The Unity Evaluation Environment
To add a new problem domain to the platform, follow these steps to integrate your custom environment, agents, and configurations. This guide will walk you through creating the necessary folders, scenes, and prefabs, and implementing the required logic to define your problem domain.

### **Step 1: Create New Folders for Your Problem Domain**

1. **Problem Domains Folder**: Inside the main project directory, navigate to the `Problem Domains` folder. Create a new folder with the name of your problem domain (e.g., `Dummy`).
    - Example: `Problem Domains/Dummy`
2. **Resources Folder**: Next, navigate to the `Resources` directory.
    - Inside `Resources`, create the following two subfolders for your problem domain:
        - `JSONs`: This folder will contain any JSON configuration files for your problem domain.
        - `SOs`: This folder will contain any `ScriptableObject` files related to your problem domain.
    - Example:
        
        ```
        Resources/JSONs/Dummy
        Resources/SOs/Dummy
        
        ```
        

### **Step 2: Create Scenes for Your Problem Domain**

For your custom problem domain, you need to create three main scenes that handle the game environment, agent logic, and configuration setup.

1. **DummyGameScene**:
    - This scene will contain the core **game environment** for your problem domain.
    - It should include all the elements and objects related to the environment in which the agents will interact (e.g., terrain, obstacles, targets, etc.).
2. **DummyAgentScene**:
    - This scene will contain the **agent-related components**.
    - It should include the **`EnvironmentController`**, which is responsible for controlling the simulation, as well as other necessary components for running the simulation and the agent’s behavior.
    - This scene is where most of the action will occur, such as agent movement, decision-making, and interaction with the environment.
3. **DummyBaseScene**:
    - This scene will contain the **Coordinator** and **Communicator**, as well as all **general configuration logic**.
    - The **Coordinator** manages the overall evaluation, while the **Communicator** handles the evaluation of one individual group (match).
    - This is where you define global settings, such as initial conditions, environment setup, and simulation control.

### **Step 3: Create Simulation/Game Prefabs**

For the simulation to work, you need to create the necessary **prefabs** that define the objects and entities within your problem domain.

- **Agent Prefab**: Every problem domain must have at least one **Agent prefab**. This prefab will define the agent's appearance, components, and behavior. It should include components such as:
    - A **Rigidbody** for movement if required.
    - An **AgentController** to handle the agent’s decision-making and actions.
    - Any other components needed for your agent’s behavior (e.g., sensors, abilities, etc.).
- **!Important!** - All prefabs that are added to the scene need to be **unpacked**, otherwise the determinism of the simulation is at risk.
    
### **Step 4: Implement New Problem Domain Logic**

At this stage, you need to implement the logic specific to your new problem domain. This involves extending and defining various classes that will form the core of your problem domain.

1. **EnvironmentControllerBase**:
    - This is the base class for controlling the simulation environment.
    - You must extend this class to define the rules and interactions for your specific problem domain. For example, if you're implementing a soccer game, the environment controller would handle the player and ball movement and game rules.
2. **AgentComponent**:
    - This class represents the agent's functionality and behavior.
    - Extend it to include the agent's logic, such as movement, action selection, and interactions with the environment.
3. **MatchSpawner**:
    - This class is responsible for spawning and initializing the matches.
    - You can customize it to specify how the agents are spawned in the arena, and how the agents are respawned.
4. **ActionExecutor**:
    - This class is responsible for translating the agent’s actions into actual in-game movements. Agent's actions are stored in the ActionBuffer property.
    - Implement this class to handle the specific actions that agents can perform (e.g., move, attack, interact with objects).
5. **Fitness**:
    - Fitness logic determines how well an agent performs in the environment.
    - Define your custom fitness keys and fitness values, which are used to set the agent's performance based on the objectives of your problem domain (e.g., scoring goals in soccer, surviving in a combat game, etc.).
6. **ManualAgentController**:
    - This class allows for manual control of the agent if desired.
    - You can create an agent controller that allows human input or a hybrid approach with automatic and manual control for testing purposes.

### **Step 5: Additional Scripts (Problem Domain Specific)**

Beyond the core classes listed above, you may need to implement other scripts specific to your problem domain. These could include:
- Custom components for agent interactions.
- Scripts to manage game rules, events, or environmental hazards.
- Specialized systems for handling resources, scoring, or rewards.

Make sure to clearly define the problem domain-specific behavior in separate scripts to maintain modularity and ensure each aspect of the problem domain is well-defined.

### **Step 6: Test and Validate**

Once you have implemented the core logic and configurations for your new problem domain:
- Test the simulation to ensure that agents behave as expected and interact correctly with the environment.
- Validate the fitness evaluation to ensure it reflects the desired performance metrics (e.g., agents should be rewarded for winning, surviving, or completing objectives).
- Iterate on your implementation to fix any issues or improve the agent's performance.

## Next Step
To see which problem domains are already included in the platform, check out the [existing problem domains](/docs/GenIATraP_problem_domains.md). 