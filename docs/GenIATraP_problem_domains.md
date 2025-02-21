# GenIATraP Problem Domains

The platform currently includes four implemented problem domains: **RoboStrike**, **Soccer**, **Collector**, and **Bomberman**. These domains provide a mix of single-agent and multi-agent environments, allowing for diverse agent interactions and challenges. Each agent within these environments can be controlled using different types of controllers. The AI Controller supports various decision-making models, including **custom scripts**, **Behavior Trees (BTs)**, **Neural Networks (NNs)**, and **Finite State Machines (FSMs)**. Additionally, the Manual Controller allows for direct user input, functioning as a custom script that processes player commands and updates the agent’s ActionBuffer accordingly. This flexible control system enables both autonomous AI-driven gameplay and human-in-the-loop experimentation.

## RoboStrike Problem Domain
[**RoboStrike**](/docs/GenIATraP_robostrike_problem_domain.md) is a multi-agent combat simulation where agents control tanks in a strategic battle arena. Agents must navigate obstacles, track opponents, and use efficient attack and evasion strategies to survive and eliminate opponents.

## Soccer Problem Domain
[**Soccer**](/docs/GenIATraP_soccer_problem_domain.md): A multi-agent soccer environment where agents compete to score goals by strategically positioning themselves, passing, and shooting. The challenge lies in teamwork, spatial awareness, and decision-making under dynamic conditions.

## Collector Problem Domain
[**Collector**](/docs/GenIATraP_collector_problem_domain.md) is a single-agent environment where agents navigate a map to collect scattered resources while avoiding obstacles. The goal is to optimize movement efficiency and maximize resource collection within a given time frame.

## Bomberman Problem Domain
[**Bomberman**](/docs/GenIATraP_bomberman_problem_domain.md) is a multi-agent environment inspired by the classic Bomberman game, where agents strategically place bombs to destroy obstacles and eliminate opponents while avoiding explosions. Success requires careful planning, opponent prediction, and tactical movement.
