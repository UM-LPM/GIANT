# Introduction to Single-Agent and Multi-Agent Systems

**Optimization** is at the heart of artificial intelligence (AI), allowing agents to make the best possible decisions based on given objectives. In the context of game AI, optimization techniques are essential for creating **intelligent**, **adaptive**, and **efficient agents**.

Depending on the nature of the game and the number of decision-making entities involved, optimization can be classified into two broad categories:
- **Single-Agent Optimization** – AI optimizes its own actions to achieve a goal in a static or dynamic environment.
- **Multi-Agent Optimization** – Multiple AI agents optimize their actions while interacting with each other in a dynamic environment.

Understanding these concepts is crucial when designing AI for competitive, cooperative, or hybrid game environments.

## Agent
An **Agent** is an autonomous entity capable of observing its environment and making decisions on how to act based on these observations. Its actions impact the environment's state. Agents can be classified from simple to complex types: **Passive agents** have no goals (e.g., obstacles) and their state does not change, **Active agents** have simple goals (e.g., animals), and **Cognitive agents** pursue complex goals (e.g., complex operations).

Key characteristics of an agent include **Autonomy**, meaning it is at least partially self-sufficient; **Local view**, as agents do not possess a global perspective; and **Decentralization**, as agents are not designed to control others.

The environments in which agents operate can be categorized as **Virtual**, **Discrete**, or **Continuous**. Additionally, these environments can be classified based on various criteria, such as:

- **Accessibility**, which determines if complete information about the environment can be gathered;
- **Determinism**, which refers to whether an action leads to a specific effect;
- **Dynamics**, which defines how many entities influence the environment at a given moment;
- **Discreteness**, which determines if the number of possible actions is finite;
- **Episodicity**, where agent actions in one period may affect subsequent periods;
- **Dimensionality**, which relates to whether spatial features are important in decision-making.

Agents can have various types of **relations**:

- **Collaborating agents** work together to complete a task;
- **Negotiating agents** collaborate but may negotiate on which actions each will perform if a conflict arises;
- **Controlling agents** can take control over another agent.

Several **properties** distinguish agents from simple controllers:

- **Situatedness** refers to the agent’s interaction with the environment using sensors and actuators, where all inputs are a direct result of this interaction.
- **Autonomy** is the ability to independently choose actions without external intervention.
- **Inferential capability** allows an agent to operate on abstract goals, deriving observations through information generalization.
- **Responsiveness** is the ability to quickly perceive the environment’s state and adapt to changes.
- **Pro-activeness** enables an agent to take initiative and adapt to changes in a dynamic environment.
- **Social behavior** involves the agent’s ability to cooperate with others to achieve a goal, share knowledge, and learn from experiences of other entities.

Agents can be further classified as **weak** or **strong** based on their capabilities.

The typical composition of an autonomous agent is depicted in the diagram below:

![Agent Architecture](/docs/images/agent_architecture.png)

In gaming environments, autonomous agents often follow a goal-directed architecture. Agents decide which goal to pursue based on periodic checks, with each goal linked to an algorithm that dictates rational behavior. Teams of evolving agents decide on goals based on a **decision tree** developed through **genetic programming**.

Within goal-directed agent architectures, **goals** can be either **Atomic** (defining a single task) or **Composite** (composed of multiple subgoals). Composite goals can be broken down into simpler subgoals, creating a hierarchical structure for each agent’s behavior.

**Middle agents** manage a list of services offered by all agents. When an agent seeks a specific service, it first contacts the middle agent, who directs it to the appropriate agent providing the service. Depending on implementation, middle agents can be:

- **Facilitators**, where all communication between agents must pass through this agent;
- **Mediators**, where agents can communicate directly but route requests to the central agent when specific information is needed, without knowing which agent is responsible.

## Single-Agent Systems

In a **Single-Agent System**, an **individual AI agent** aims to maximize its own performance by **optimizing a defined objective function**. There is **no competition or collaboration** with other AI entities—the agent simply works towards its own goal.

### **Key Characteristics**

- Focus on individual performance
- Optimization is independent of other agents
- Uses Reinforcement Learning, Evolutionary Algorithms, or other heuristic-based approaches

### **Applications in Game AI**
- **Pathfinding:** Finding the most efficient path from point A to B using algorithms like *A (A-star)** or **Dijkstra’s algorithm**.
- **Decision Making:** AI-controlled characters choosing optimal actions in single-player games.
- **Skill Progression:** AI improving its strategy over time in **puzzle games or simulations**.

## Multi-Agent Systems


