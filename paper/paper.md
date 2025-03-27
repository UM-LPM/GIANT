# GIANT - General Intelligent Agent Trainer

---
title: 'GIANT: General Intelligent Agent Trainer'
tags:
  - Machine Learning
  - Evolutionary Computation
  - game engines
  - behavior trees
  - games
  - platform
  - Multi-Agent Systems
  - Self-Organizing Systems
authors:
  - name: Marko Šmid
    orcid: 0009-0001-9099-6416
    equal-contrib: true
    affiliation: 1
  - name: Miha Ravber
    orcid: 0000-0003-4908-4631
    equal-contrib: true
    affiliation: 1
affiliations:
 - name: University of Maribor, Faculty of Electrical Engineering and Computer Science
   index: 1
date: 27 March 2025
bibliography: paper.bib

# Summary

GIANT is a versatile, open-source platform designed to train intelligent agents using diverse machine learning techniques and evaluate them in game engine-based environments. Its highly modular architecture supports a wide range of optimization methods, with the current implementation leveraging Genetic Programming (GP) [@koza2005GP] within the EARS framework [@ears2019]. GIANT aims to provide a standardized benchmark for comparing single- and multi-agent systems, incorporating concepts of self-organization to enable adaptive evaluation, and offers flexibility for integrating custom environments and techniques.

# Statement of need

Numerous frameworks exist for training intelligent agents using a variety of machine learning techniques and evaluation environments[@juliani2020MlAgents;@Song2020Arena;@towers2024gymnasium;@partlan2022EvolvingBehavior;@bellemare2013Ale;@wrona2023SoftwareAgentPlatforms]. However, fairly comparing these solutions remains a persistent challenge. Variations in human implementation, framework performance, and architectural design often introduce inconsistencies that hinder objective analysis [@carton204FairnesInML;@Nguyen2019MLSurvey]. Moreover, when diverse AI controller types must be evaluated within a shared problem domain, most existing frameworks lack the flexibility to support such comparisons under unified conditions.

The importance of robust benchmarking methodologies is well-documented in the literature, with calls for standardized experimental protocols to enhance reproducibility and fairness in optimization research [@bartz2020benchmarking;@latorre2020fairness]. To address these gaps, GIANT offers a modular, standardized platform for training and evaluation across a range of optimization methods. By ensuring consistent experimental conditions, GIANT enables reproducible, cross-method benchmarks tailored to the needs of AI researchers and developers. The platform supports deterministic execution with a fixed seed for repeatable experiments, while also allowing dynamic environments to test agent adaptability under varying conditions.

# Architecture 

The conceptual design of the platform consists of three core components: the Machine Learning Framework, the Evaluation Environment, and the Web App Interface (see \autoref{fig:platformArchitecture}). These components work together to provide a flexible and extensible platform for training and evaluating intelligent agents.

The Machine Learning Framework includes a collection of optimization algorithms designed for single-agent or multi-agent systems. This framework provides the foundation for training agents using diverse machine learning techniques, ensuring adaptability across different problem domains.

The Evaluation Environment contains a set of problem domains that simulate real-world challenges. These environments serve as standardized benchmarks for evaluating agent performance, allowing for fair and deterministic comparisons.

The Web API Interface acts as the central integration layer, facilitating communication between the Machine Learning Framework and the Evaluation Environment. Its primary function is to convert individual solutions into a standardized format that can be interpreted and evaluated across different environments.

The modular architecture of GIANT ensures that components remain independent, allowing for the seamless integration or replacement of individual modules without affecting the performance of the overall system. When introducing a new component, only the Web App Interface needs to be adapted to align with its specifications, making the platform highly scalable and adaptable.

![Architecture of GIANT platform.\label{fig:platformArchitecture}](/docs/images/platform_architecture_orig.png)

# Included problem domains

The platform includes a set of predefined problem domains, ranging from single-agent to multi-agent scenarios, to facilitate the development of custom problem domains. These problem domains are structured as game-based environments, where agents operate under the control of AI-driven decision-making models.

The platform currently supports the following problem domains: Collector, RoboStrike, Soccer, and BombClash. Within these environments, agents can be controlled using a variety of AI controllers, including custom scripts, Behavior Trees (BTs) [@isla2022bts] and Artificial Neural Networks (ANNs) [@richards1998ANNs]. Additionally, the platform features a Manual Controller, which allows for direct user input by functioning as a custom script that processes player commands.

- Collector (see \autoref{fig:collector}) is a single-agent environment, inspired by ML-Agents' Food Collector [@juliani2020MlAgents], where agents navigate a map to collect scattered resources while avoiding obstacles. The goal is to optimize movement efficiency and maximize resource collection within a given time frame.
	
  ![Collector problem domain.\label{fig:collector}](/docs/images/collectorProblemDomain/collector_problem_domain_main.png){ width=40% }

- RoboStrike (see \autoref{fig:robostrike}) is a multi-agent combat simulation inspired by Robocode [@nelson2024Robocode], where agents are represented as tanks and placed in a strategic battle arena. Agents must navigate obstacles, track opponents, and employ efficient attack and evasion strategies to survive and eliminate opponents.

  ![RoboStrike problem domain.\label{fig:robostrike}](/docs/images/robostrikeProblemDomain/robostrike_problem_domain_main.png){ width=40% }

- Soccer (see \autoref{fig:soccer}) is a replica of ML-Agents' Soccer Twos [@juliani2020MlAgents], a multi-agent environment where agents compete to score goals by strategically positioning themselves, passing, and shooting.

  ![Soccer problem domain.\label{fig:soccer}](/docs/images/soccerProblemDomain/soccer_problem_domain_main.png){ width=40% }

- BombClash (see \autoref{fig:bombclash}) is a multi-agent environment inspired by the Bomberland multi-agent AI competition [@coderOne2021Bomberland], where agents strategically place bombs to destroy obstacles, eliminate opponents, and evade explosions. Success requires careful planning, opponent prediction, and tactical movement.
	
  ![BombClash problem domain.\label{fig:bombclash}](/docs/images/bombClashProblemDomain/bombClash_problem_domain_main.png){ width=40% }

Platforms' flexible control system enables both fully autonomous AI-driven gameplay and human-in-the-loop experimentation, making the platform suitable for a wide range of research and development applications in machine learning, artificial intelligence, and game AI.

# Optimization process

The optimization process starts in the EARS framework, which generates an initial population of solutions. This population is sent via a web API to the Unity-based evaluation environment. The API converts individuals into a compatible format and forwards them to the coordinator, Unity’s main HTTP server.

The coordinator distributes individuals across available Unity instances based on configuration settings. Each instance, managed by a communicator (an internal HTTP server), conducts evaluations including agents that are controlled by a single controller (e.g., Behavior Trees, Neural Networks, or Manual Controllers). The environment controller updates agents at fixed intervals, ensuring consistent evaluation.

Once the evaluations are completed, the coordinator collects and sends the results back through the web API to EARS, where the optimization continues.

# Scalability and Performance Optimization

Since the evaluation phase is the most time-consuming part of the optimization process [@li2022ExpensiveOptimization], the platform implements two levels of parallelization to enhance efficiency. The first level of parallelization operates within a single Unity instance, where multiple simulations are evaluated simultaneously within the same environment but on isolated simulation spaces, ensuring fair and independent evaluation. The second level involves running multiple Unity instances. The number of instances that can run concurrently is constrained only by the processing power of the machine executing the experiment. 

To further improve the performance, GIANT provides render toggling, allowing users to disable visual rendering during evaluations. This reduces GPU load and maximizes computational efficiency, especially when graphical output is unnecessary. Additionally, the platform supports increasing the time scale, which accelerates physics simulations by running the simulation at a higher speed than real-time. This feature ensures faster data collection and shortens training cycles without affecting simulation fidelity.

# References