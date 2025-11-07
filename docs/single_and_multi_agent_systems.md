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

![Agent Architecture](/docs/images/agent_scheme.png)

In gaming environments, autonomous agents often follow a goal-directed architecture. Agents decide which goal to pursue based on periodic checks, with each goal linked to an algorithm that dictates rational behavior. Teams of evolving agents decide on goals based on a **decision tree** developed through **genetic programming**.

Within goal-directed agent architectures, **goals** can be either **Atomic** (defining a single task) or **Composite** (composed of multiple subgoals). Composite goals can be broken down into simpler subgoals, creating a hierarchical structure for each agent’s behavior.

**Middle agents** manage a list of services offered by all agents. When an agent seeks a specific service, it first contacts the middle agent, who directs it to the appropriate agent providing the service. Depending on implementation, middle agents can be:

- **Facilitators**, where all communication between agents must pass through this agent;
- **Mediators**, where agents can communicate directly but route requests to the central agent when specific information is needed, without knowing which agent is responsible.

## Single-Agent Systems (SAS)

In a **Single-Agent System**, an **individual AI agent** aims to maximize its own performance by **optimizing a defined objective function**. There is **no competition or collaboration** with other AI entities—the agent simply works towards its own goal.

### **Key Characteristics**

- Focus on individual performance
- Optimization is independent of other agents
- Uses Reinforcement Learning, Evolutionary Algorithms, or other heuristic-based approaches

### **Applications in Game AI**
- **Pathfinding:** Finding the most efficient path from point A to B using algorithms like *A (A-star)** or **Dijkstra’s algorithm**.
- **Decision Making:** AI-controlled characters choosing optimal actions in single-player games.
- **Skill Progression:** AI improving its strategy over time in **puzzle games or simulations**.

## Multi-Agent Systems (MAS)
A multi-agent system (MAS) is a group of autonomous, interacting entities that share a common environment, which they sense using sensors and act upon with actuators. Together, they aim to achieve a common goal → They can **cooperate**, **compete**, **share**, or **not share** information.

MAS is a subfield of **Distributed Artificial Intelligence (DAI)**, which has rapidly grown due to its adaptability and intelligence in solving distributed problems. These systems can be used in various fields to solve problems where a single agent cannot achieve the goal, or its achievement is time-consuming and inefficient. The decision-making problem in MAS is **complex**. Each agent must be equipped with an independent decision-making system to interact with other agents and the environment. Communication between agents is limited, and the amount of information that can be sent or received is usually constrained by the environment's type.

Each agent has different functions and skills that can be performed within a cycle or time period. To perform the best action for achieving the system’s goal, two main problems must be addressed:
1. First, the agent's functions and skills need to be optimized to achieve the best possible outcome.
2. Second, when all functions and skills are available, a good decision-making system is required to apply them at the right time for an efficient and rapid achievement of the goal.

**Advantages** of using MAS technology:
- **Increased speed and efficiency** through parallel processing and asynchronous operation.
- **Gradual system degradation** when one or more agents fail, improving system reliability and robustness.
- **Scalability and flexibility:** Agents can be added as needed.
- **Lower costs:** Individual agents cost much less than in centralized architectures.
- **Reusability:** Agents have a modular structure and can be easily replaced in other systems or upgraded more easily than monolithic systems.

**Disadvantages** of using MAS technology:
- **Environment:** In MAS, each agent’s action not only affects the environment but also other agents (neighbors). This means each agent must predict how other agents will behave, which can lead to unstable behavior and chaos.
- **Perception:** Decisions based on partial observations by each agent may be suboptimal, making it difficult to achieve a global solution.
- **Abstraction:** In MAS, agents may not have experience with all states, so they must learn from other agents with similar capabilities. Cooperation among agents with similar goals enables learning through communication. However, competing agents cannot exchange information, as they aim to improve their chances of success. Therefore, it is important to define the necessary knowledge and capabilities for improving environment modeling.
- **Conflict resolution:** An agent’s decision based on local observations can negatively impact other agents, requiring improved cooperation between agents (e.g., exchanging information about constraints, preferences). The main challenge is knowing when to communicate this information and to which agents for optimal benefit.
- **Inference:** Using MAS technology makes it difficult to draw conclusions because predicting how agents' actions will affect the global state is hard, especially when heterogeneous agents are present.

According to the **level of cooperation**, multi-agent systems are classified into:
- **Cooperative:** Agents work together to achieve the best possible outcome (overall performance).
- **Competitive/Adversarial:** Agents compete with each other and seek to maximize their own fitness.
- **Mixed:** A combination of the above two techniques.

There are many **techniques** for learning behavior: **Adaptive control**, **Genetic algorithms** and **Reinforcement learning.**

**Approaches** to multi-agent optimization:
- **Decentralized** → Each agent learns independently from others and has no information about the actions and states of other agents.
    - Simpler approach (No need for logic to communicate with others).
    - Agents' performance may not be optimal (if one agent has already performed an action and another is unaware of it, it will repeat the action, requiring additional time and resources).
    - Requires a static environment (If agents lack information about others, changes in the environment can cause collisions, preventing goal achievement or solution convergence).
- **Centralized** → Each agent learns alongside others, with data about the other agents shared within the group.
    - Experiences and parameters are shared among agents (experiences are stored in a shared buffer).
    - More complex approach (needs logic to share experiences, etc.).
    - Ensures a static environment (all agents are treated as a single unit).
    - Faster learning (agents gain experience from others).

The division of MAS based on **general agent organization**:
- **Hierarchical Organization:** This is one of the oldest organizational approaches in MAS.
    - Agents are organized in a **tree structure**. Agents at different levels have **different levels of autonomy**. Data from lower levels of the hierarchy typically **flow upward** to agents at higher hierarchies. Control or supervisory signals flow from higher to lower hierarchies. The hierarchical architecture can further be divided into:
    - **Simple Hierarchy:** Only the agent at the highest level can make decisions. The problem with this organization is the “single point failure.”
    - **Uniform Hierarchy:** Authority is distributed among different agents to increase efficiency, fault tolerance, and gradual degradation in the case of one or more failures.
        - Decisions are made by agents with the relevant information. These decisions are sent upward in the hierarchy only if there is a conflict of interest between agents at different hierarchies.
  ![Hierarchical Organization](/docs/images/mas_hierarchical_agent_org.png)
- **Holonic Agent Organization:** A holon is a stable and coherent structure, similar or fractal in nature, made up of several holons as substructures, and is itself part of a larger network.
    - The hierarchical structure of the holon and its interactions have been used to model a large number of organizational behaviors in manufacturing and business fields.
    - In a holonic multi-agent system, an agent that appears as a single entity is composed of several sub-agents, which are **connected by commitments**.
    - Each holon designates or selects a main agent that can communicate with the environment or other agents in the environment. The selection of the main agent typically depends on resource availability, communication capabilities, and the internal architecture of each agent.
    - In a homogeneous multi-agent system, the selection may be random. In a heterogeneous architecture, the selection depends on the agent's capabilities. Formed holons can further merge into groups according to anticipated benefits in creating a coherent structure: **superholons**.
    - The abstraction of the internal workings of holons provides a **greater degree of freedom in choosing behaviors**.
    - The main disadvantage is the **lack of a model or knowledge of the internal architecture of the holons**, which makes it difficult for other agents to predict the subsequent actions of the holons.
  ![Hierarchical Organization](/docs/images/mas_holonic_agent_org.png)
- **Coalitions:** In a coalition architecture, a group of agents temporarily joins together to increase the utility or performance of individual agents within the group.
    - The coalition ceases to exist once the goal is achieved.
    - Agents forming a coalition can have a **flat** or **hierarchical architecture**. In both cases, there may be a **leader agent**.
    - Overlapping agents between coalition groups is allowed, as it increases the overall knowledge within the coalition group. The use of overlap increases the complexity of calculating negotiation strategies.
    - Maintaining a coalition is **challenging in a dynamic environment** due to changes in the group's performance. Agents may need to be regrouped to achieve the greatest efficiency of the system.
    - Theoretically, by forming a single coalition containing all agents, the system's performance could be maximized; However, forming such a coalition is impractical due to communication and resource limitations.
    - The number of created coalitions should be kept to a minimum to reduce the costs associated with forming and dissolving a coalition group.
    - A coalition-based MAS may have better short-term efficiency than other agent architectures.
  ![Hierarchical Organization](/docs/images/mas_coalition_agent_org.png)
- **Team:** By design, it is similar to coalition architecture, but agents in the group collaborate to increase the overall performance of the team.
    - Interactions between agents in the group can be quite arbitrary, and the goals or roles assigned to individual agents may change over time based on improvements resulting from the team's performance.
    - Large teams allow for better situational awareness and more important information. However, this impacts the learning or integration of individual agents' experiences into a unified team framework.
    - A smaller group enables faster learning, but due to a limited view of the environment, it does not allow for optimal performance.  When choosing the optimal team size, a compromise must be found between learning and performance. → This increases computational costs, which are much higher than those in coalition-based multi-agent system architectures.
  ![Hierarchical Organization](/docs/images/mas_team_agent_org.png)
### MAS Properties
- **Leadership** (leader-follower; without leader): In the leader-follower approach, the leader determines actions for all other agents, whereas in the no-leader approach, agents decide their actions independently. Followers communicate among themselves to determine the leader's position. The leader can be predefined or selected by the agents, and it may be static or dynamic, moving through the space.
- **Decision function** (linear; nonlinear): In a linear MAS, an agent's decision is proportional to the environmental parameters it perceives (e.g., a thermostat turns off the heater when the temperature reaches a threshold). These are easy to analyze mathematically. In nonlinear MAS, an agent's decision is not proportional to perceived metrics due to nonlinear input data (e.g., several airplanes must agree on the tilt angle, which is affected by ground location and tilt angle).
- **Heterogeneity** (Homogeneous; heterogeneous): In a Homogeneous approach, the same program is used for all agents in the group without considering their specific roles, whereas in a Heterogeneous approach, separate programs are developed for each agent or for different groups of agents with similar roles (e.g., in football: defender, midfielder, and attacker).
- **Agreement parameters** (first-order, second-order, high-order): In first-order, agents cooperate to agree on a single metric. In second-order, agents must agree on two metrics (e.g., considering position and speed when designing spacecraft). In high-order MAS, agents agree when the metrics (one or two) and their high-order derivatives converge to a common value. For example, in a bird flock, birds consider acceleration in addition to position and speed when agreeing on flying direction.
- **Delay consideration** (with delay; without delay): Refers to the problem of whether to account for delays in operations (e.g., communication) or not.
- **Topology** (static; dynamic): Refers to the location and relationships among agents.
- **Data transmission frequency** (time triggers, event triggers): Refers to the problem of whether to send acquired data based on a time interval or when a specific trigger event occurs.
- **Mobility** (static agents, dynamic agents)

For a more detailed description of Single-Agent and Multi-Agent Systems please refer to the [references](/docs/single_and_multi_agent_systems.md/#references) listed below. 

## References
- Multi-agent system, https://en.wikipedia.org/wiki/Multi-agent_system
- An Introduction to MultiAgent Systems: [https://books.google.si/books?hl=sl&lr=&id=X3ZQ7yeDn2IC&oi=fnd&pg=PR13&dq=An+Introduction+to+MultiAgent+Systems&ots=WHpkop7wa0&sig=JCfBr6bQtpGw_rF02UQCdIv4Zd8&redir_esc=y#v=onepage&q=An Introduction to MultiAgent Systems&f=false](https://books.google.si/books?hl=sl&lr=&id=X3ZQ7yeDn2IC&oi=fnd&pg=PR13&dq=An+Introduction+to+MultiAgent+Systems&ots=WHpkop7wa0&sig=JCfBr6bQtpGw_rF02UQCdIv4Zd8&redir_esc=y#v=onepage&q=An%20Introduction%20to%20MultiAgent%20Systems&f=false)
- Aronsson, Jonatan. ‘**Genetic Programming of Multi-Agent System in the RoboCup Domain**’, 1 January 2003.
- Gustafson, Steven, and William Hsu. ‘**Layered Learning in Genetic Programming for a Cooperative Robot Soccer Problem**’, 2001. https://doi.org/10.1007/3-540-45355-5_23.
- Seaton, Tom, Julian Miller, and Tim Clarke. **Semantic Bias in Program Coevolution**. Vol. 7831, 2013. https://doi.org/10.1007/978-3-642-37207-0_17.
- Panait, Liviu, and Sean Luke. ‘**Cooperative Multi-Agent Learning: The State of the Art**’. Autonomous Agents and Multi-Agent Systems 11, no. 3 (1 November 2005): 387–434. https://doi.org/10.1007/s10458-005-2631-2.
- Parasumanna Gokulan, Balaji, and D. Srinivasan. ‘**An Introduction to Multi-Agent Systems**’. In Studies in Computational Intelligence, 310:1–27, 2010. https://doi.org/10.1007/978-3-642-14435-6_1.
- Qin, Jiahu, Qichao Ma, Yang Shi, and Long Wang. ‘**Recent Advances in Consensus of Multi-Agent Systems: A Brief Survey**’. IEEE Transactions on Industrial Electronics 64, no. 6 (June 2017): 4972–83. https://doi.org/10.1109/TIE.2016.2636810.
- J. Barambones, J. Cano-Benito, I. Sánchez-Rivero, R. Imbert and F. Richoux, "**Multiagent Systems on Virtual Games: A Systematic Mapping Study**" in IEEE Transactions on Games, vol. 15, no. 2, pp. 134-147, June 2023, doi: 10.1109/TG.2022.3214154.
- A. Dorri, S. S. Kanhere and R. Jurdak, "**Multi-Agent Systems: A Survey**," in IEEE Access, vol. 6, pp. 28573-28593, 2018, doi: 10.1109/ACCESS.2018.2831228.
