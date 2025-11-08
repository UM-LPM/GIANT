# BoxTact Problem Domain

The **BoxTact** problem domain is a single-agent puzzle-solving environment inspired by the classic Sokoban game. The agent operates in a 2D grid-based world and must strategically push boxes onto their designated target positions. The objective is to move all boxes to their correct locations using the fewest possible moves. This domain is ideal for evaluating agents that exhibit spatial reasoning, planning, and decision-making in discrete environments.

<center><img src="/docs/images/boxtactProblemDomain/boxtact_problem_domain_main.png?raw=true" alt="BoxTact" width="500"/></center>


## Objective
The primary objective for the agent in the BoxTact domain is to move all boxes to their respective target areas on the grid. The agent can move in four directions (up, down, left, right) and push boxes when standing adjacent to them. A box moves only if the space behind it is free. The simulation ends when all boxes are correctly positioned on their targets or when a predefined number of moves is reached. Agents must plan sequences of actions to efficiently navigate and manipulate their surroundings.

## Environment
The environment is a bounded 2D grid, consisting of cells representing empty space, walls, boxes, target areas, and the agent itself. The agent moves cell-by-cell, pushing boxes into adjacent free spaces. Levels can vary in complexity based on the number of boxes, obstacles, and grid size, which are configurable via the settings file.

### Agent
The agent is capable of moving in four cardinal directions and pushing boxes that are directly in front of it. However, the agent cannot pull boxes, requiring it to plan each move carefully to avoid getting boxes stuck. The agent’s success depends on its ability to anticipate the consequences of its actions and to optimize movement efficiency.

<center><img src="/docs/images/boxtactProblemDomain/boxtact_problem_domain_agents.png?raw=true" alt="BoxTact Agent" width="100"/></center>

### Boxes and Targets
Boxes are movable objects that must be pushed onto target cells. A box placed on a target cell is considered “solved.” When all boxes are correctly positioned, the level is completed. The number of boxes and targets is configurable, allowing for varying levels of difficulty. Agents are rewarded for successfully moving boxes and placing them in the correct positions.

<center><img src=/docs/images/boxtactProblemDomain/boxtact_problem_domain_box.png?raw=true" alt="BoxTact Boxes" width="100"/> <img src="https://github.com/UM-LPM/GeneralTrainingEnvironmentForMAS/blob/main/docs/images/boxtactProblemDomain/boxtact_problem_domain_box_target.png?raw=true" alt="BoxTact Targets" width="100"/></center>

### Environment Perception
Agents perceive their surroundings using a Grid Sensor, which captures a 5×5 cell area centered around the agent. Each cell in the sensor’s grid encodes information about nearby objects, such as walls, boxes, targets, and empty cells. This allows the agent to plan moves based on local spatial information rather than global knowledge. The sensor updates in real time as the agent moves through the environment.

<center><img src="/docs/images/boxtactProblemDomain/boxtact_domain_object_detection.png?raw=true" alt="BoxTact Perception" width="400"/></center>

## Fitness Evaluation
Fitness evaluation in the BoxTact problem domain focuses on efficiency, progress, and goal completion. It encourages agents to solve the puzzle while minimizing unnecessary movement. Key fitness components include:
- BoxesOnTarget: A large reward for each box successfully placed on a target cell at the end of the simulation.
- BoxesMoved: A smaller reward for moving boxes, encouraging exploration and interaction with the environment.
- MovesMade: A penalty proportional to the number of moves taken, promoting efficient planning and minimal action sequences.
