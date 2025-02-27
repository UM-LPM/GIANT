# GenIATraP Overview

The **General Intelligent Agents Training Platform (GenIATraP)** is an open-source project that provides a flexible and modular environment for training intelligent agents using a variety of machine-learning techniques. It integrates existing solutions such as metaheuristic frameworks and game engines, enabling seamless training and evaluation of agents. With support for Unity as a simulation environment and the EARS framework for optimization, GenIATraP allows researchers and developers to explore advanced decision-making systems, including Behavior Trees, Neural Networks, and evolutionary algorithms.

Agents can be trained in both **Single** and **Multi-Agent** scenarios, leveraging parallelization at two levels: within a single Unity instance and across multiple Unity instances. The platform is designed for extensibility, making introducing new problem domains easy, comparing different training methods, and visualizing results. Additionally, manual testing support enables human players to compete against AI-driven solutions, offering valuable insights into agent performance.

GenIATraP bridges AI research and game development, providing a structured environment where new AI techniques can be tested, benchmarked, and refined within interactive and dynamic simulations.

So if you are an AI researcher, game developer, or hobbyist—you may have different goals when using GenIATraP. To ease your transition, we provide background materials covering key concepts in **[machine learning](/docs/introduction_to_machine_learning.md)**, **[game design](/docs/basics_of_game_design.md)**, **[AI controllers](/docs/AI_controllers.md)** and more. If you're new to any of these topics, we highly recommend reviewing these resources before getting started.

The rest of this documentation provides a deep dive into GenIATraP, including its core components, training methodologies, and implementation details. By the end, you’ll have a clear understanding of how to use the platform effectively. Subsequent sections will walk you through setting up and using GenIATraP for your projects.

# General Idea
The conceptual platform comprises three fundamental components: a Machine Learning (ML) framework, an Evaluation Environment, and a Web API intended to facilitate seamless integration between these two components.

The **Machine Learning framework** encompasses a sophisticated suite of algorithms tailored to optimize Single-agent and Multi-agent Systems. Currently among these frameworks is only EARS, which offers genetic programming algorithms tuned for Single-agent and Multi-agent optimization tasks.

**Evaluation Environments** contain a diverse set of problems that replicate real-world challenges encountered in Single-Agent and Multi-Agent Systems.

Functioning as an intermediary, the **Web API** serves to effectively bridge the ML frameworks and Evaluation Environments. Its primary objective is to translate individual representations of solutions within the ML framework into formats (such as programs or trees) that can be interpreted and evaluated by the Evaluation Environment. Subsequently, it relays pertinent information concerning the success of these solutions back to the ML framework.

![Platform General Idea](/docs/images/platform_general_idea.png)
# Optimization Process (Example with EARS framework and Unity Game engine)

### 1. Optimization Process Start 
The optimization process initiates within the **EARS framework**, where the initial population is generated, and the optimization begins. During the evaluation process, a **POST** request is triggered via an HTTP call to the **Web API**. The entire population with extra parameters is encapsulated in JSON format within the request body.
## 2. Data Translation
- The Web API undertakes the translation of the Tree Models array from the request to **Agent Controllers**, subsequently saving them on the filesystem within the Evaluation Environment's source folder.
- Following the translation of all Tree Models, the Web API initiates a POST request to the Unity HTTP Server (Coordinator).

## 3. Evaluation
- Upon receiving the request, the **Coordinator** module retrieves all **Individuals** from the designated source file and initiates the evaluation process.
- Based on the configuration, matches are generated, including all individuals. There exist different [types of evaluators](/docs/GenIATraP_unity_overview.md), each creating different types of matches.
- Depending on the number of **Unity instances** and **matches**, the **Coordinator** distributes the matches across all instances (each instance receives an equal amount of matches to evaluate). Each Unity instance contains a **Communicator**, whose task is to coordinate the evaluation inside the instance and upon successful evaluation forward the match results.
- The **Communicator** iterates through the matches and, based on the configuration, loads corresponding scenes for evaluation. Scene loading persists until either all matches are in the evaluation process or the maximum batch size is attained. When all matches are executed, the match results are forwarded to the **Coordinator**.
## 4. Simulation Execution
- Each scene incorporates an **EnvironmentController** responsible for monitoring the simulation state and controlling the simulation for one match at a time.
- Upon scene loading, all individuals inside the teams are spawned (**Match Spawner** contains the rules for spawning individuals) and the evaluation process begins.
- In fixed time intervals, the **EnvironmentController** updates each agent; first, actions from **Agent Controller** are gathered, then based on the actions gathered they are applied to the agent (move, jump, shoot, etc.). Each **Individual** can contain multiple **Agent Controllers** but one agent can only have one (**Behavior Tree**, **Neural Network**, **Manual Controller**, etc.).
- Upon meeting the termination criterion, the **EnvironmentController** emits an event, signaling termination, appended with a match result (The performance of each agent is gathered and set to the individual that the agent refers to).
- The **Communicator** receives and saves the match result data and continues with the evaluation of a new match. 
## 5. Response
- Following the completion of the evaluation process, the **Coordinator** forwards the response to the Web API, which subsequently relays it back to EARS, where the optimization process continues.

Full optimization process is shown on the image below:

![Platform optimization process](/docs/images/platform_architecture.png)
# Next Step
Before jumping to platform setup, the following concepts should be known to you:
- Introduction to [Machine Learning](/docs/introduction_to_machine_learning.md)
- Understanding [Single-Agent & Multi-Agent](/docs/single_and_multi_agent_systems.md) Optimization
- Basics of [Game Design](/docs/basics_of_game_design.md)
- Introduction to [AI Controllers](/docs/AI_controllers.md)
- [3D Modeling](/docs/3d_modeling.md) for Game Development
- [Game Testing and Debugging](/docs/game_testing_and_debugging.md)

By following these [instructions](/docs/GenIATraP_platform_setup.md), the platform can be set up in just a few simple steps.
