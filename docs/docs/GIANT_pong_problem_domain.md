# Pong Problem Domain

The **Pong problem domain** is a competitive multi-agent environment inspired by the classic arcade game. In this environment, two or four agents control paddles positioned around a 2D arena and attempt to prevent the ball from entering their area while directing it into the opponents’ goals. The objective is to react quickly, predict ball trajectories, and maintain control of the match. This domain provides an ideal testbed for evaluating agents’ reflexes, predictive control, and spatial reasoning in a continuous 2D environment.

<center><img src="/docs/images/pongProblemDomain/pong_problem_domain_main.png?raw=true" alt="Pong" width="600"/></center>

## Objective
The main objective for agents in the Pong domain is to defend their area while scoring points by directing the ball into opponents’ areas. When an agent fails to block the ball and it enters its area, a point is awarded to the opposing player, and the ball is reset to the center of the arena. Agents must anticipate ball movement, position efficiently, and react quickly to continuously changing trajectories.

## Environment
The environment is a flat 2D rectangular arena with defined boundaries. The ball moves freely within the play area and bounces off the arena walls and paddles according to the laws of reflection. Depending on the chosen configuration, the game can be played in two-player or four-player mode. Each agent has an area behind its paddle, which must be defended at all times. The environment’s dimensions, ball speed, and bounce behavior are configurable through the settings file.


### Agent
Each agent represents a paddle that can move along a single axis — vertically (in 2-player mode) or along its designated boundary (in 4-player mode). Agents must track the ball’s position, predict its future trajectory, and position the paddle to intercept it.

<center><img src="/docs/images/pongProblemDomain/pong_problem_domain_agents.png?raw=true" alt="Pong Agent" width="50"/></center>


### Ball
The ball is the primary interactive object in the environment. It starts in the center of the arena and moves with a constant speed. The direction changes whenever it collides with a paddle or a wall, following reflection rules. When a point is scored, the ball is reset, and play resumes. The ball’s speed, start bounce angle, and reset conditions are configurable parameters.

<center><img src="/docs/images/pongProblemDomain/pong_problem_domain_ball.png?raw=true" alt="Pong Ball" width="100"/></center>

### Environment Perception
Agents perceive their surroundings using a Ray Perception Sensor. A configurable number of rays are emitted from each agent, allowing detection of nearby walls, the ball, teammates, and opponents. When a ray detects an object, it returns information about the object type and distance. If no object is detected, the ray returns an empty result. This enables agents to make real-time decisions based on partial perception of the 2D environment.

<center><img src="/docs/images/pongProblemDomain/pong_domain_object_detection.png?raw=true" alt="Pong Object Detection" width="400"/></center>

## Fitness Evaluation
Fitness evaluation in the Pong problem domain focuses on precision and strategic control. It rewards agents that effectively defend their area and successfully score against opponents. Key fitness components include:
- PointsScored: A large reward for successfully directing the ball into an opponent’s area, rewarding offensive success.
- PointsConceded: A penalty for allowing the ball to enter the agent’s area, promoting defensive stability.
- BallControl: A reward for keeping the ball in play or influencing its direction effectively.
