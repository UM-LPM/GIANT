# GIANT - General Intelligent Agent Trainer

---
title: 'GIANT: General Intelligent Agent Trainer'
tags:
  - Machine Learning
  - evolutionary computation
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
date: 21 March 2025
bibliography: paper.bib

# Summary

GIANT is a versatile, open-source platform designed to train intelligent agents using diverse machine learning techniques and evaluate them in game engine-based environments. Its highly modular architecture supports a wide range of optimization methods, with the current implementation leveraging Genetic Programming (GP) [9] within the EARS framework [7]. GIANT aims to provide a standardized benchmark for comparing single- and multi-agent systems, incorporating concepts of self-organization to enable adaptive evaluation, and offers flexibility for integrating custom environments and techniques.
# Statement of need

Numerous frameworks exist for training intelligent agents using a variety of machine learning techniques and evaluation environments [1-5]. However, fairly comparing these solutions remains a persistent challenge. Variations in human implementation, framework performance, and architectural design often introduce inconsistencies that hinder objective analysis [@carton204FairnesInML, Nguyen2019MLSurvey]. Moreover, when diverse AI controller types must be evaluated within a shared problem domain, most existing frameworks lack the flexibility to support such comparisons under unified conditions.

The importance of robust benchmarking methodologies is well-documented in the literature, with calls for standardized experimental protocols to enhance reproducibility and fairness in optimization research [@bartz2020benchmarking, @latorre2020fairness]. To address these gaps, GIANT offers a modular, standardized platform for training and evaluating agents across a range of optimization methods. By ensuring consistent experimental conditions, GIANT enables reproducible, cross-method benchmarks tailored to the needs of AI researchers and developers. The platform supports deterministic execution with a fixed seed for repeatable experiments, while also allowing dynamic environments to test agent adaptability under varying conditions.
# Architecture 

The conceptual design of the platform consists of three core components: the Machine Learning Framework, the Evaluation Environment, and the Web App Interface (see Figure 1). These components work together to provide a flexible and extensible platform for training and evaluating intelligent agents.

The Machine Learning Framework includes a collection of optimization algorithms designed for single-agent or multi-agent systems. This framework provides the foundation for training agents using diverse machine learning techniques, ensuring adaptability across different problem domains.

The Evaluation Environment contains a set of problem domains that simulate real-world challenges. These environments serve as standardized benchmarks for evaluating agent performance, allowing for fair and deterministic comparisons.

The Web API Interface acts as the central integration layer, facilitating communication between the Machine Learning Framework and the Evaluation Environment. Its primary function is to convert individual solutions into a standardized format that can be interpreted and evaluated consistently across different environments.

The modular architecture of GIANT ensures that components remain independent, allowing for the seamless integration or replacement of individual modules without affecting the performance of the overall system. When introducing a new component, only the Web App Interface needs to be adapted to align with its specifications, making the platform highly scalable and adaptable.

<figure>
  <img src="/docs/images/platform_architecture_orig.png" alt="GIANT Architecture">
  <figcaption><strong>Figure 1:</strong> Architecture of GIANT platform</figcaption>
</figure>
# Included problem domains

The platform includes a set of predefined problem domains, ranging from single-agent to multi-agent scenarios, to facilitate the development of custom problem definitions. These problem domains are structured as game-based environments, where agents operate under the control of AI-driven decision-making models.

The platform currently supports the following problem domains: Collector, RoboStrike, Soccer, and BombClash. Within these environments, agents can be controlled using a variety of AI controllers, including custom scripts, Behavior Trees (BTs) [8], Neural Networks (NNs), Finite State Machines (FSMs). Additionally, the platform features a Manual Controller, which allows for direct user input by functioning as a custom script that processes player commands.

- Collector (Figure 2) is a single-agent environment where agents navigate a map to collect scattered resources while avoiding obstacles. The goal is to optimize movement efficiency and maximize resource collection within a given time frame.
	<figure>
	  <img src="/docs/images/collectorProblemDomain/collector_problem_domain_main.png" alt="Collector" width="300">
	  <figcaption><strong>Figure 2:</strong> Collector.</figcaption>
	</figure>
- RoboStrike (Figure 3) is a multi-agent combat simulation where agents are represented as tanks and placed in a strategic battle arena. Agents must navigate obstacles, track opponents, and use efficient attack and evasion strategies to survive and eliminate opponents.
	<figure>
	  <img src="/docs/images/robostrikeProblemDomain/robostrike_problem_domain_main.png" alt="Robostrike" width="300">
	  <figcaption><strong>Figure 3:</strong> RoboStrike problem</figcaption>
	</figure>
- Soccer (Figure 4) is a multi-agent soccer environment where agents compete to score goals by strategically positioning themselves, passing, and shooting. The challenge lies in teamwork, spatial awareness, and decision-making under dynamic conditions.
	<figure>
	  <img src="/docs/images/soccerProblemDomain/soccer_problem_domain_main.png" alt="Soccer" width="300">
	  <figcaption><strong>Figure 4:</strong> Soccer problem</figcaption>
	</figure>
- BombClash (Figure 5) is a multi-agent environment inspired by the classic Bomberman game, where agents strategically place bombs to destroy obstacles and eliminate opponents while avoiding explosions. Success requires careful planning, opponent prediction, and tactical movement.
	<figure>
	  <img src="/docs/images/bombClashProblemDomain/bombClash_problem_domain_main.png" alt="BombClash" width="300">
	  <figcaption><strong>Figure 5:</strong> BombClash problem</figcaption>
	</figure>

Platforms' flexible control system enables both fully autonomous AI-driven gameplay and human-in-the-loop experimentation, making the platform suitable for a wide range of research and development applications in machine learning, artificial intelligence, and game AI.
# Optimization process

The optimization process starts in the EARS framework, which generates an initial population of solutions. This population is sent via a web API to the Unity-based evaluation environment. The API converts individuals into a compatible format and forwards them to the coordinator, Unity’s main HTTP server.

The coordinator distributes individuals across available Unity instances based on configuration settings. Each instance, managed by a communicator (an internal HTTP server), conducts evaluations including agents that are controlled by a single controller (e.g., Behavior Trees, Neural Networks, Finite State Machines, or Manual Controllers). The environment controller updates agents at fixed intervals, ensuring consistent evaluation.

Once the evaluations are completed, the coordinator collects and sends the results back through the web API to EARS, where the optimization continues.
# Scalability and Performance Optimization

Since the evaluation phase is the most time-consuming part of the optimization process, the platform implements two levels of parallelization to enhance efficiency. The first level of parallelization operates within a single Unity instance, where multiple simulations are evaluated simultaneously within the same environment but on isolated simulation spaces, ensuring fair and independent assessments. The second level involves running multiple Unity instances. The number of instances that can run concurrently is constrained only by the processing power of the machine executing the experiment. 

To further improve the performance, GIANT provides render toggling, allowing users to disable visual rendering during evaluations. This reduces GPU load and maximizes computational efficiency, especially when graphical output is unnecessary. Additionally, the platform supports increasing the time scale, which accelerates physics simulations by running the simulation at a higher speed than real-time. This feature ensures faster data collection and shortens training cycles without affecting simulation fidelity.
# References

[1] A. Juliani et al., ‘Unity: A General Platform forIntelligent Agents’. arXiv, May 06, 2020. doi:10.48550/arXiv.1809.02627.

[2] Y. Song, “Arena: A General Evaluation Platform and Building Toolkit for Multi-Agent Intelligence”, _Proceedings of the AAAI Conference on Artificial Intelligence_, vol. 34, no. 5, pp. 7253–7260, 2020, doi: 10.1609/aaai.v34i05.6216. 

[3] M. Towers _et al._, “Gymnasium: A Standard Interface for Reinforcement Learning Environments,” Nov. 08, 2024, _arXiv_: arXiv:2407.17032. doi: [10.48550/arXiv.2407.17032](https://doi.org/10.48550/arXiv.2407.17032).

[4] . Partlan, L. Soto, J. Howe, S. Shrivastava, M. S. El-Nasr,and S. Marsella, ‘EvolvingBehavior: Towards Co-CreativeEvolution of Behavior Trees for Game NPCs’. arXiv, Sep.01, 2022. doi: 10.48550/arXiv.2209.01020. 

[5] Wrona et al., ‘Overview of Software Agent PlatformsAvailable in 2023’, Information, vol. 14, no. 6, p. 348, Jun.2023, doi: 10.3390/info14060348.

[7] N. Veček, M. Mernik, and M. Črepinšek, ‘A chess ratingsystem for evolutionary algorithms: A new method for thecomparison and ranking of evolutionary algorithms’, Infor-mation Sciences, vol. 277, pp. 656–679, Sep. 2014, doi:10.1016/j.ins.2014.02.154.  

[8] Isla, Damian. "Handling complexity in the Halo 2 AI, 2005." _URL: http://www. gamasutra. com/gdc2005/features/20050311/isla _01. shtml [21.1. 2010]_ (2022).

[9] Koza, John & Poli, Riccardo. (2005). Genetic Programming. 10.1007/0-387-28356-0_5.

