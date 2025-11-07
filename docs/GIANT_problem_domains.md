# GIANT Problem Domains

The platform currently includes four implemented problem domains: **RoboStrike**, **Soccer**, **Collector**, and **BombClash**. These domains provide a mix of single-agent and multi-agent environments, allowing for diverse agent interactions and challenges. Each agent within these environments can be controlled using different types of controllers. The AI Controller supports various decision-making models, including **custom scripts**, **Behavior Trees (BTs)**, **Neural Networks (NNs)**, and **Finite State Machines (FSMs)**. Additionally, the Manual Controller allows for direct user input, functioning as a custom script that processes player commands and updates the agent’s ActionBuffer accordingly. This flexible control system enables both autonomous AI-driven gameplay and human-in-the-loop experimentation.

## RoboStrike Problem Domain
[**RoboStrike**](/docs/GIANT_robostrike_problem_domain.md) is a **multi-agent** combat simulation where agents are represented as tanks and placed in a strategic battle arena. Agents must navigate obstacles, track opponents, and use efficient attack and evasion strategies to survive and eliminate opponents.

<center><img src="/docs/images/robostrikeProblemDomain/robostrike_problem_domain_main.png" alt="Robostrike" width="600"/></center>


## Soccer Problem Domain
[**Soccer**](/docs/GIANT_soccer_problem_domain.md): A **multi-agent** soccer environment where agents compete to score goals by strategically positioning themselves, passing, and shooting. The challenge lies in teamwork, spatial awareness, and decision-making under dynamic conditions.

<center><img src="/docs/images/soccerProblemDomain/soccer_problem_domain_main.png" alt="Soccer" width="600"/></center>

## Collector Problem Domain
[**Collector**](/docs/GIANT_collector_problem_domain.md) is a **single-agent** environment where agents navigate a map to collect scattered resources while avoiding obstacles. The goal is to optimize movement efficiency and maximize resource collection within a given time frame.

<center><img src="/docs/images/collectorProblemDomain/collector_problem_domain_main.png" alt="Collector" width="600"/></center>

## BombClash Problem Domain
[**BombClash**](/docs/GIANT_bombclash_problem_domain.md) is a **multi-agent** environment inspired by the classic Bomberman game, where agents strategically place bombs to destroy obstacles and eliminate opponents while avoiding explosions. Success requires careful planning, opponent prediction, and tactical movement.

<center><img src="/docs/images/bombClashProblemDomain/bombClash_problem_domain_main.png" alt="BombClash" width="600"/></center>

## DodgeBall Problem Domain
[**DodgeBall**](/docs/GIANT_dodgeball_problem_domain.md) is a **multi-agent** environment, where agents try to eliminate opponents by throwing balls at them while dodging incoming throws.

<center><img src="/docs/images/dodgeballProblemDomain/dodgeball_problem_domain_main.png" alt="DodgeBall" width="600"/></center>

## Pong Problem Domain

[**Pong**](/docs/GIANT_pong_problem_domain.md) is a **multi-agent** environment, where players control paddles on opposite sides of the screen, bouncing a ball back and forth in a test of reflexes, timing, and precision to score points against their opponent.

<center><img src="/docs/images/pongProblemDomain/pong_problem_domain_main.png" alt="Pong" width="600"/></center>

[**BoxTact**](/docs/GIANT_boxtact_problem_domain.md) is an environment inspired by the classic Sokoban puzzle game, where players navigate a maze-like grid, strategically pushing boxes onto designated targets while planning each move carefully to avoid getting trapped.

<center><img src="/docs/images/boxtactProblemDomain/boxtact_problem_domain_main.png" alt="BoxTact" width="600"/></center>
