# GIANT - EARS Overview

**EARS** (Evolutionary Algorithms Rating System) is a free and open-source Java-based Machine Learning (ML) framework developed by the University of Maribor’s Laboratory for Programming Methodologies ([UM-LPM](http://lpm.feri.um.si/en/)). It is designed for ranking, developing, and experimenting with single- and multi-objective evolutionary algorithms. EARS provides a robust platform for researchers, developers, and enthusiasts to benchmark, compare, and extend evolutionary algorithms with ease.
## What is EARS?
EARS is a modular and extensible framework tailored for evolutionary computation. It facilitates the systematic evaluation and comparison of evolutionary algorithms by providing a suite of predefined problems (e.g., Ackley, Rastrigin, Sphere), benchmarking tools, and a flexible architecture for integrating custom algorithms and solutions. Whether you're a researcher studying optimization techniques or a developer building novel algorithms, EARS streamlines the process with its comprehensive toolkit.

**Framework highlights**:
- **Open-Source**: Freely available under an open-source license.
- **Java-Based**: Built with Java for cross-platform compatibility.
- **Single- and Multi-Objective**: Supports both single-objective and multi-objective optimization.
- **Extensible**: Easily extendable with custom problems, algorithms, and solution representations.
## Key Features

EARS offers a rich set of features to support evolutionary algorithm development and experimentation:
1. **Predefined Problems**:  
    - Includes classic unconstrained problems like Ackley, Booth, Griewank, Rastrigin, Schaffer, Schwefel, and Sphere.
    - Multi-objective problems such as DTLZ, ZDT, and WFG are also supported.
    - Problems come with configurable dimensions, constraints, and bounds.
2. **Benchmarking Framework**:  
    - Built-in tools for ranking algorithms based on performance metrics.
    - Supports repeated experiments and statistical analysis.
3. **Algorithm Implementation**:  
    - Provides examples like ES1p1sAlgorithm, TLBOAlgorithm, and RandomWalkAlgorithm.
    - Easy integration of custom algorithms.
4. **Evaluation Control**:  
    - Handles stopping criteria (e.g., maximum evaluations or convergence).
    - Manages solution fitness evaluation with optimization direction (min/max).
5. **Modularity**:  
    - Separates problems, tasks, algorithms, and solutions into distinct, reusable components.
6. **Utility Tools**:  
    - Random number generation, result storage, and debugging utilities.
  
## How Does EARS Work?

EARS operates on a task-based workflow:
1. **Problem Definition**: A Problem object encapsulates the optimization problem (e.g., dimensions, bounds, objective function).
2. **Task Creation**: A Task object combines a problem with stopping criteria and evaluation tracking.
3. **Algorithm Execution**: An Algorithm iterates over solutions, leveraging the Task to evaluate fitness and check termination conditions.
4. **Result Analysis**: Results are collected and can be analysed or ranked using benchmarking tools.
  
The framework ensures consistency by abstracting low-level details (e.g., solution validation, fitness comparison) while allowing flexibility for customization.
## Genetic Programming in EARS
**Genetic Programming (GP)** is a subset of evolutionary algorithms that evolves computer programs, typically represented as tree structures, to solve problems. Unlike traditional evolutionary algorithms that optimize numerical parameters, GP searches for the best program structure (e.g., mathematical expressions, decision or behavior trees) by applying genetic operators like crossover, mutation, and selection. It is widely used for tasks such as symbolic regression, classification, and automated program synthesis.

**EARS** includes support for Genetic Programming, allowing users to define and evolve **tree-based solutions** for problems where the goal is to discover functional relationships or programs rather than numerical optima. The framework provides:
- **Tree Representation**: Solutions are represented as trees with **functions** and **terminals**.
- **Predefined GP Problems**: Includes **symbolic regression benchmarks** (e.g., evolving a function to fit data points) and general **problem for Unity Evaluation Environment**.
- **GP Algorithms**: Multiple **GP Algorithm variants** exist, supporting many standard GP operations (e.g., **subtree crossover**, **subtree mutation**, **subtree encapsulation**, **hall of fame**, etc.) within the extensible algorithm framework.
- **Evaluation Tools**: GUI to track the optimization progress and analyse final results. 
## Official Documentation
For official documentation please refer to the following [**GitHub respository**](https://github.com/UM-LPM/EARS).