# GenIATraP Overview

The **General Intelligent Agents Training Platform (GenIATraP)** is an open-source project that provides a flexible and modular environment for training intelligent agents using a variety of machine-learning techniques. It integrates existing solutions such as metaheuristic frameworks and game engines, enabling seamless training and evaluation of agents. With support for Unity as a simulation environment and the EARS framework for optimization, GenIATraP allows researchers and developers to explore advanced decision-making systems, including Behavior Trees, Neural Networks, and evolutionary algorithms.

Agents can be trained in both **Single** and **Multi-Agent** scenarios, leveraging parallelization at two levels: within a single Unity instance and across multiple Unity instances. The platform is designed for extensibility, making introducing new problem domains easy, comparing different training methods, and visualizing results. Additionally, manual testing support enables human players to compete against AI-driven solutions, offering valuable insights into agent performance.

GenIATraP bridges AI research and game development, providing a structured environment where new AI techniques can be tested, benchmarked, and refined within interactive and dynamic simulations.

So if you are an AI researcher, game developer, or hobbyist—you may have different goals when using GenIATraP. To ease your transition, we provide background materials covering key concepts in **[machine learning]()**, **[metaheuristic optimization]()**, and **[Unity] game engine]()**. If you're new to any of these topics, we highly recommend reviewing these resources before getting started.

The rest of this documentation provides a deep dive into GenIATraP, including its core components, training methodologies, and implementation details. By the end, you’ll have a clear understanding of how to use the platform effectively. Subsequent sections will walk you through setting up and using GenIATraP for your projects.

# General Idea
The conceptual platform comprises three fundamental components: a Machine Learning (ML) framework, an Evaluation Environment, and a Web API intended to facilitate seamless integration between these two components.

The **Machine Learning framework** encompasses a sophisticated suite of algorithms tailored to optimize Single and Multi-agent Systems. Noteworthy among these frameworks is EARS, which offers genetic programming algorithms finely tuned for Multi-agent optimization tasks.

**Evaluation Environments** contain a diverse set of problems that replicate real-world challenges encountered in Single and Multi-agent Systems.

Functioning as a pivotal intermediary, the **Web API** serves to effectively bridge the ML frameworks and Evaluation Environments. Its primary objective is to translate individual representations of solutions within the ML framework into formats (such as programs or trees) that can be interpreted and evaluated by the Evaluation Environment. Subsequently, it relays pertinent information concerning the success of these solutions back to the ML framework.

![Platform General Idea](/docs/images/platform_general_idea.png)

# Next Step
[Platform Setup](https://github.com/UM-LPM/GeneralTrainingEnvironmentForMAS/blob/platform_refactor/docs/GenIATraP_platform_setup.md)
