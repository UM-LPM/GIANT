# AI Controllers

AI Controllers define **how non-player characters (NPCs) and agents** behave in a game environment. Whether controlling **enemy behavior, friendly NPCs, or AI-driven players**, AI controllers are responsible for **decision-making, movement, and interaction**.

Game AI can range from **simple rule-based systems** to **complex machine learning models** that adapt and evolve over time. This page covers:

1. **What AI Controllers Are**
2. **Key Considerations in AI Controller Design**
3. **Major AI Controller Types**
    - Finite State Machines (FSMs)
    - Behavior Trees
    - Utility-Based AI
    - GOAP (Goal-Oriented Action Planning)
    - Neural Networks
    - Custom Scripts & Hybrid Approaches
    - Rule-Based AI
    - Fuzzy Logic AI
    - HTN (Hierarchical Task Networks)
    - Genetic Algorithms (Evolutionary AI)
    - Multi-Agent AI
    - Procedural/Adaptive AI

## 1. What Are AI Controllers?

AI Controllers are **scripts or systems** that control **the actions and decisions** of NPCs in games. They **analyze the game state** and execute actions based on **predefined logic, probabilities, or learned behaviors**.

**Examples of AI Controllers in Games:**

- **Stealth Games:** Guards patrolling a level react dynamically to the player (*Metal Gear Solid*).
- **Strategy Games:** AI-controlled armies manage resources and attack strategically (*Age of Empires*).
- **Racing Games:** Opponent cars adjust speed and tactics to maintain competition (*Mario Kart*).

## 2. Key Considerations in AI Controller Design

Before choosing an AI controller type, consider:

✅ **Complexity** – Does the AI need to make **simple** or **advanced** decisions?

✅ **Predictability vs. Adaptability** – Should AI behavior be **consistent** or **dynamic**?

✅ **Performance** – How computationally expensive is the AI system?

✅ **Scalability** – Can the system handle **many agents** at once?

## 3. Major AI Controller Types

AI controllers can be implemented using various architectures, each suited for different types of decision-making and behaviors. The choice of AI system depends on the complexity of the game, the desired level of adaptability, and computational constraints. Below are the most common AI controller types used in game development:

| AI Controller Type | Best For | Pros | Cons |
| --- | --- | --- | --- |
| **Finite State Machines (FSMs)** | Simple NPCs & predictable AI | Easy to implement, low CPU usage | Hard to scale for complex behaviors |
| **Behavior Trees (BTs)** | Modular, structured AI logic | Reusable, scalable | Can be hard to optimize |
| **Utility-Based AI** | Dynamic decision-making | Adaptive & flexible | Requires fine-tuning utility functions |
| **GOAP (Goal-Oriented Action Planning)** | AI that needs dynamic planning | Efficient for emergent behaviors | Computationally expensive |
| **Neural Networks & Machine Learning** | Adaptive AI & self-learning | AI learns over time | Requires data & high processing power |
| **Rule-Based AI** | Simple AI (dialogues, NPCs) | Easy to debug | Becomes unmanageable with many rules |
| **Fuzzy Logic AI** | AI with gradual decision-making | Handles uncertainty well | More complex than rule-based systems |
| **HTN (Hierarchical Task Networks)** | AI that plans tasks | Ideal for strategy games | Harder to implement |
| **Genetic Algorithms (Evolutionary AI)** | Self-improving AI | Finds optimal solutions over time | Unpredictable results |
| **Multi-Agent AI** | Cooperative/Competitive AI | Works well for team-based AI | High computational overhead |
| **Procedural/Adaptive AI** | AI that reacts to player behavior | Creates more organic AI experiences | Requires extensive balancing |

### 3.1. Finite State Machines (FSMs) – Structured, Rule-Based Decision Making

Finite State Machines (FSMs) are one of the simplest AI control mechanisms, where an agent transitions between predefined states based on specific conditions. Each state governs the agent’s behavior, and transitions occur when the game environment meets certain criteria.

#### Example: Enemy AI in a Shooter

- **Idle** → (Player detected) → **Chase** → (Player in range) → **Attack**
- **Attack** → (Low health) → **Retreat** → (Health recovered) → **Idle**

#### Advantages

✔ Simple to implement and efficient in execution.

✔ Predictable and easy to debug.

#### Limitations

❌ Becomes difficult to scale as the number of states increases.

❌ Lacks flexibility, requiring explicit programming for every possible behavior.

#### Common Use Cases

👾 Classic arcade AI (*Pac-Man ghosts*).

💂 Enemy patrols in stealth games (*Metal Gear Solid*).

### 3.2. Behavior Trees (BTs) – Modular, Hierarchical AI Systems

Behavior Trees provide a more scalable alternative to FSMs by structuring AI decisions in a tree format. Each node in the tree represents a behavior, and branches dictate decision-making logic.

#### Example: AI in a Strategy Game

📌 **Selector Node:** Choose between attacking, defending, or retreating.

📌 **Sequence Node:** If the enemy is close → Check health → Attack if strong, retreat if weak.

#### Advantages

✔ Modular and reusable, allowing for easy expansion.

✔ More scalable than FSMs, making them suitable for complex decision-making.

#### Limitations

❌ Requires careful tree design to avoid inefficient structures.

❌ Can become computationally expensive for large behavior trees.

#### Common Use Cases

🏹 NPC behaviors in *Assassin’s Creed*.

🤖 AI opponents in *Halo* and *F.E.A.R.*.

### 3.3. Utility-Based AI – Adaptive, Dynamic Decision Making

Utility-based AI systems allow agents to evaluate multiple actions and select the one with the highest priority. Instead of following fixed rules, agents dynamically adjust their behavior based on utility scores assigned to different options.

#### Example: AI in a Survival Game

🔥 **Hunger:** 80% → Find food.

💤 **Fatigue:** 60% → Rest.

⚔ **Enemy Nearby:** 90% → Engage in combat or flee.

#### Advantages

✔ Highly flexible, enabling dynamic decision-making.

✔ More adaptive than FSMs or Behavior Trees.

#### Limitations

❌ More complex to design and fine-tune.

❌ Requires defining and balancing utility functions.

#### Common Use Cases

🧟 Enemy behavior in *Left 4 Dead*.

🏙 NPC decision-making in *The Sims*.

### 3.4. Goal-Oriented Action Planning (GOAP) – AI-Driven Action Sequencing

Goal-Oriented Action Planning (GOAP) is a planning-based AI system that dynamically determines the best sequence of actions to achieve a goal. Instead of following predefined states or trees, GOAP allows AI agents to evaluate the available actions and construct a plan based on the current situation.

#### Example: AI in a Stealth Game

🎯 **Goal:** Eliminate the player.

📌 Available Actions: **Find Weapon**, **Move to Player's Location**, **Take Cover**, **Attack**.

📌 **AI dynamically selects and sequences actions** instead of following a fixed path.

#### Advantages

✔ More flexible than FSMs and Behavior Trees, as agents can generate plans dynamically.

✔ Efficient for complex AI behaviors where predefined decision trees would be impractical.

#### **Limitations**

❌ Requires computational overhead for real-time planning.

❌ Complex to implement compared to traditional AI techniques.

#### Common Use Cases

🔫 Enemy AI in *F.E.A.R.* (Monolith Productions).

💀 Tactical NPC AI in *Hitman* series.

### 3.5. Neural Networks & Machine Learning – Self-Learning AI Systems

Neural Networks allow AI agents to learn patterns from data and optimize their decision-making process over time. Unlike traditional AI controllers that rely on pre-defined logic, machine learning-based AI can adapt to changing environments.

#### **Example: Self-Learning AI in a Fighting Game**

👊 AI **observes player strategies** → **Adapts tactics** based on common player moves.

#### Advantages

✔ Can handle complex and unpredictable behaviors.

✔ Adaptive AI can improve its performance over time.

#### **Limitations**

❌ Requires large amounts of data for training.

❌ Computationally expensive compared to rule-based AI.

#### Common Use Cases

🎮 AI bots in *Dota 2* (*OpenAI Five*).

🚗 Self-driving AI in *Gran Turismo Sophy*.

### 3.6. Custom Scripts & Hybrid Approaches – Combining AI Techniques

Many modern games use a hybrid approach, leveraging multiple AI techniques to achieve the best balance of flexibility, adaptability, and performance.

#### Example: AI in a Racing Game

🏎 **FSM:** AI car switches between **Idle, Racing, and Crashing**.

🛤 **Utility AI:** Adjusts driving strategy based on **weather and opponent behavior**.

🧠 **Neural Networks:** AI learns optimal lap times over multiple races.

#### Advantages

✔ Allows for highly sophisticated AI behavior.

✔ Leverages the strengths of multiple AI techniques.

#### Limitations

❌ More complex to implement and balance.

❌ May require additional computational resources.

#### Common Use Cases

🚀 Modern FPS & RTS games (*Halo, Total War*).

🏁 Racing games with adaptive AI (*Forza Horizon*).

### 3.7. Rule-Based Systems – Explicit, Hardcoded Decision-Making

Rule-Based AI uses a set of predefined "if-then" rules to determine the agent’s actions. These systems are straightforward but become complex when handling many conditions.

#### Example: NPC Dialogue System

📌 **If** the player chooses "angry response" → NPC responds aggressively.

📌 **If** the player has a high reputation → NPC provides a discount.

#### Advantages

✔ Highly predictable and easy to implement.

✔ Efficient when the number of rules is small.

#### Limitations

❌ Becomes difficult to maintain when rules scale up.

❌ Lacks adaptability unless combined with other techniques.

#### Common Use Cases

🗣 NPC dialogue and branching quests (*The Witcher 3*).

🤖 Simple AI behaviors in puzzle games (*Tic-Tac-Toe AI*).

### 3.8. Fuzzy Logic Systems – Handling Uncertainty & Approximate Decision-Making

Fuzzy Logic AI controllers deal with uncertainty by allowing for **partial truth values** instead of strict binary (true/false) conditions. Instead of "yes or no" decisions, they work with probabilities and "degrees of truth."

#### Example: Driving AI in a Racing Game

🚗 "If the turn is **sharp**, reduce speed **significantly**."

🚗 "If the turn is **moderate**, reduce speed **slightly**."

#### Advantages

✔ Handles uncertainty better than FSMs or Rule-Based AI.

✔ Useful for AI requiring continuous input variation.

#### Limitations

❌ More complex than traditional rule-based AI.

❌ Requires careful tuning of fuzzy logic rules.

#### Common Use Cases

🏎 Racing game AI (*Gran Turismo AI*).

🎯 Aiming assistance in FPS games (*Dynamic aim assist*).

### 3.9. Planning-Based AI (HTN – Hierarchical Task Networks)** – **Task-Driven AI for Complex Planning

Hierarchical Task Networks (HTN) AI controllers break down high-level goals into a series of smaller, manageable tasks. It is often used in strategy and simulation games.

#### **Example: AI in a Strategy Game**

🏰 **Goal:** Build a fortress.

📌 **Subtasks:** Gather wood → Gather stone → Construct walls → Train guards.

#### Advantages

✔ Enables structured long-term planning.

✔ Well-suited for complex AI decision-making.

#### Limitations

❌ Requires significant computational resources.

❌ Harder to implement compared to FSMs or Behavior Trees.

#### Common Use Cases

♟ AI in strategy games (*Civilization*, *Total War*).

🛠 AI in resource management games (*RimWorld*).

### 3.10. Evolutionary AI & Genetic Algorithms – Self-Optimizing AI Through Evolution

Evolutionary AI controllers use **genetic algorithms** to evolve agents over time. Instead of being explicitly programmed, the AI **learns** by testing different strategies and selecting the most successful ones. Each evolutionary AI controller is a basic structure (Behavior tree, finite state machine, etc.) optimized over time.

#### Example: Evolving AI in a Fighting Game

👊 AI starts with **random strategies** → Competes against itself → Retains the best-performing behaviors → Improves with each generation.

#### Advantages

✔ AI can evolve over time to find optimal strategies.

✔ No need for hand-tuned parameters.

#### Limitations

❌ Requires extensive training time.

❌ Can result in unpredictable or undesired behaviors.

#### Common Use Cases

🧠 AI in research & simulations (*AI that learns to walk*).

👾 Self-learning AI in games (*NEAT AI in Flappy Bird clones*).

### 3.11. Multi-Agent Systems (MAS) AI – Cooperative & Competitive AI Systems

Multi-Agent AI controllers involve **multiple AI agents** working together, either cooperatively or competitively, to achieve goals. They are widely used in **team-based games and simulations.**

#### Example: AI in a Soccer Game

⚽ **Cooperative agents:** AI teammates pass the ball and coordinate attacks.

⚽ **Competitive agents:** AI opponents try to counter the player’s strategy.

#### Advantages

✔ Ideal for games with **multiple AI-controlled entities**.

✔ Allows for emergent behaviors through agent interactions.

#### Limitations

❌ Computationally expensive, especially in large-scale simulations.

❌ Requires careful design to prevent chaotic agent interactions.

#### Common Use Cases

🏀 Team-based sports AI (*FIFA, Rocket League*).

🔫 Squad-based FPS AI (*Rainbow Six Siege, Call of Duty bots*).

### 3.12. Procedural AI & Adaptive AI – AI That Changes Based on Player Behavior

Procedural AI adapts dynamically to **player choices, game progression, or external conditions.** This makes AI behavior feel **less scripted and more organic.**

#### Example: AI Director in a Horror Game

👁 *Left 4 Dead’s AI Director* monitors player performance and **adjusts enemy difficulty dynamically.**

#### Advantages

✔ Enhances player immersion with unpredictable AI.

✔ Can be used to create dynamic difficulty balancing.

#### Limitations

❌ Requires extensive testing to ensure fairness.

❌ Hard to debug due to AI’s non-deterministic nature.

#### Common Use Cases

🧟 Adaptive enemy spawning (*Left 4 Dead*).

😨 Horror game AI (*Alien: Isolation’s Xenomorph AI*).

## Conclusion – How to Choose the Right AI Controller

Each AI controller type has **strengths and weaknesses**, and the best choice depends on **game complexity, agent adaptability, and processing constraints.** By combining multiple AI techniques **(Hybrid AI approaches)**, developers can create **intelligent, responsive, and engaging** game AI.
