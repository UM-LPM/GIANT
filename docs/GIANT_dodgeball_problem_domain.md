# DodgeBall Problem domain

The DodgeBall problem domain is a dynamic, multi-agent environment where two teams of agents compete in a fast-paced arena-based match. Agents must collect and throw balls at opponents to score hits while avoiding incoming throws from the opposing team. The goal is to hit opponents as many times as possible while minimizing being hit. This problem domain is ideal for evaluating agents that must exhibit team coordination, spatial awareness, decision-making under uncertainty, and real-time reaction to dynamic environments.

<center><img src="/docs/images/dodgeballProblemDomain/dodgeball_problem_domain_main.png?raw=true" alt="Dodgeball" width="600"/></center>

## Objective
The primary objective for agents in the DodgeBall domain is to collect balls and throw them at opponents to score points. When an agent successfully hits an opponent, it is rewarded, while the opponent is reset to its original spawn position. Similarly, when a ball hits an agent, the ball is also reset to the center of the arena. The environment challenges agents to coordinate with teammates, predict opponent movements, and strategically control the arena to maximize their scoring opportunities while avoiding being hit.

## Environment
The match takes place in a bounded 3D arena divided into two halves, one for each team. The arena floor is a flat surface, and agents can move freely within their designated area. In the center of the arena, several balls are placed at the start of each round. These serve as shared resources that both teams must compete to collect. The arena may include low obstacles or markings that help agents navigate and plan their movement strategies.

### Agent
Each agent represents a player on one of the two teams. Agents are capable of moving, picking up balls, throwing balls, and dodging incoming projectiles. When hit by a ball, an agent is temporarily removed from active play and respawned at its original starting position after a short delay. Agents can only hold one ball at a time, encouraging decision-making about when to throw or conserve the ball.

<center><img src="/docs/images/dodgeballProblemDomain/dodgeball_problem_domain_agents.png?raw=true" alt="Dodgeball Agent" width="200"/></center>

### Ball
Balls are the central interactive objects in the DodgeBall environment. They are initially positioned in the center of the arena. Agents can pick up a ball when within a certain distance and throw it toward an opponent. Once a ball hits an agent or collides with an arena wall, it is automatically reset to its original spawn point. The number of balls and their initial positions are configurable through the environment settings.

<center><img src="/docs/images/dodgeballProblemDomain/dodgeball_problem_domain_ball.png?raw=true" alt="Dodgeball Ball" width="100"/></center>

### Environment Perception
Agents perceive their surroundings using a Ray Perception Sensor, identical to the perception mechanism in the Robostrike domain. A predefined number of rays and angles are configured, allowing agents to detect nearby objects such as balls, opponents, teammates, and arena boundaries. Each ray returns detailed information about detected objects, including type and distance. If no object is detected, the ray returns an empty result. This perception mechanism allows agents to make real-time decisions based on partial and local information.

<center><img src="/docs/images/dodgeballProblemDomain/dodgeball_domain_object_detection.png?raw=true" alt="Dodgeball Object Detection" width="400"/></center>

## Fitness Evaluation
Fitness evaluation in the DodgeBall problem domain is designed to measure how effectively an agent contributes to its team’s success, emphasizing accuracy, survivability, and strategic play. Key fitness components include:
- HitsScored: A large reward for successfully hitting an opponent with a ball, rewarding accuracy and offensive performance.
- HitsTaken: A penalty for being hit by an opponent’s ball, encouraging agents to avoid projectiles and position strategically.
- BallControl: A reward for collecting and holding balls, promoting resource competition and area control.
- ThrowAccuracy: A secondary reward based on the distance and precision of throws, encouraging skillful targeting.
- SurvivalTime: A reward proportional to the time an agent remains active without being hit, emphasizing defensive play.
- TeamSupport: Optional component rewarding agents for assisting teammates (e.g., distracting opponents or creating space for others).
- ArenaControl: A small reward for occupying key areas of the arena or retrieving central balls, encouraging proactive positioning.
