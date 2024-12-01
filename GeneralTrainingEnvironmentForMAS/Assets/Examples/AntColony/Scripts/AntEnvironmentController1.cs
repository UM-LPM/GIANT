using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

public class AntEnvironmentController1 : EnvironmentControllerBase
{
    [Header("Ant Configuration")]
    [SerializeField] float AntMoveSpeed = 2.0f;
    [SerializeField] public float AntRotationSpeed = 90.0f;
    [Header("Ant configuration General")]
    [SerializeField] GameObject FoodPrefab;
    [SerializeField] GameObject HivePrefab;
    [SerializeField] GameObject WaterPrefab;
    [SerializeField] public float AgentUpdateinterval = 0.1f;

    private List<GameObject> foodItems = new List<GameObject>();
    public List<GameObject> FoodItems => foodItems;

    private List<GameObject> waterItems = new List<GameObject>();
    public List<GameObject> WaterItems => waterItems;

    public List<GameObject> gatheredFoodItems = new List<GameObject>();
    public List<GameObject> gatheredWaterItems = new List<GameObject>();

    private List<Hive> hiveItems = new List<Hive>();
    public List<Hive> HiveItems => hiveItems;
    [SerializeField] int numFoodItems = 10;
    [SerializeField] int numWaterItems = 1;
    [SerializeField] int numHives = 1;
    [SerializeField] int numOfAnts = 0;
    [SerializeField] public float pheromoneEvaporationRate = 10.0f;
    [SerializeField] public GameObject PheromonePrefab;
    [SerializeField] int agentStartStamina = 400;
    [SerializeField] public float recoveryRate = 10f;
    [SerializeField] public float boundaryTreshold = 30f;


    protected override void DefineAdditionalDataOnPreStart()
    {

        for (int i = 0; i < numOfAnts; i++)
        {
            GameObject agent = Instantiate(AgentPrefab, hiveItems[0].transform.position, Quaternion.identity, this.gameObject.transform);
        }
    }

    protected override void DefineAdditionalDataOnPostStart()
    {
        foreach (Hive hive in this.GetComponentsInChildren<Hive>())
        { hiveItems.Add(hive); }
        foreach (AntAgentComponent agent in Agents)
        {
            agent.Rigidbody = agent.GetComponent<Rigidbody2D>();
            agent.stamina = agentStartStamina;
            agent.pheromoneEvaporationRate = pheromoneEvaporationRate;
            agent.hive = HiveItems[0].GetComponent<Hive>();
        }
        foreach (AntAgentComponent agent in AgentsPredefinedBehaviour)
        {
            agent.stamina = agentStartStamina;
            agent.pheromoneEvaporationRate = pheromoneEvaporationRate;
            agent.hive = this.GetComponentsInChildren<Hive>()[0];
            agent.Rigidbody = agent.GetComponent<Rigidbody2D>();
        }

    }
    void SpawnFood()
    {
        float minDistance = 5.0f;

        float minAngle = 30.0f;
        for (int i = 0; i < numFoodItems; i++)
        {
            Vector2 spawnPos = this.GetRandomSpawnPointFood(minDistance);
            float angle;
            GameObject foodItem = Instantiate(FoodPrefab, spawnPos, Quaternion.identity, this.gameObject.transform);
            foodItems.Add(foodItem);
        }
    }
    public Vector2 GetRandomSpawnPoint()
    {
        if (ArenaSize != null && ArenaSize != Vector3.zero)
        {
            return new Vector2
            {
                x = Util.NextFloat(-(ArenaSize.x / 2) + ArenaOffset, (ArenaSize.x / 2) - ArenaOffset),
                y = Util.NextFloat(-(ArenaSize.y / 2) + ArenaOffset, (ArenaSize.y / 2) - ArenaOffset),
            };
        }
        else
        {
            return new Vector2(0, 0);
        }
    }

    public Vector2 GetRandomSpawnPointFood(float minDistanceFromHive)
    {
        Vector2 hivePosition = hiveItems[0].transform.position;

        if (ArenaSize != null && ArenaSize != Vector3.zero)
        {
            Vector2 spawnPos;
            float distanceToHive;
            float angleToHive;
            do
            {
                spawnPos = new Vector2
                {
                    x = Util.NextFloat(-(ArenaSize.x / 2) + ArenaOffset, (ArenaSize.x / 2) - ArenaOffset),
                    y = Util.NextFloat(-(ArenaSize.y / 2) + ArenaOffset, (ArenaSize.y / 2) - ArenaOffset),
                };
                distanceToHive = Vector2.Distance(spawnPos, hivePosition);
                angleToHive = Vector2.Angle(hivePosition, spawnPos);

            } while (distanceToHive < minDistanceFromHive || Mathf.Approximately(angleToHive, 0f) || Mathf.Approximately(angleToHive, 90f) || Mathf.Approximately(angleToHive, 180f));

            return spawnPos;
        }
        else
        {
            return Vector2.zero;
        }
    }
    public override void UpdateAgents()
    {
        if (ManualAgentControl)
        {
            MoveAgents(Agents);
        }
        else
        {
            UpdateAgentsWithBTs(Agents);

        }
        if (ManualAgentPredefinedBehaviourControl)
            MoveAgents(AgentsPredefinedBehaviour);
        else
            UpdateAgentsWithBTs(AgentsPredefinedBehaviour);
    }
    void UpdateAgentsWithBTs(AgentComponent[] agents)
    {
        UpdatePheromoneTrails();

        ActionBuffer actionBuffer;
        foreach (AntAgentComponent agent in agents)
        {
            if (agent.gameObject.activeSelf)
            {
                actionBuffer = new ActionBuffer(null, new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                agent.BehaviourTree.UpdateTree(actionBuffer);
                MoveAgent(agent, actionBuffer);

                if (actionBuffer.DiscreteActions[3] == 1)
                {
                    PickUpCarriableItem(agent, CarriableItemType.Food);
                }
                if (actionBuffer.DiscreteActions[4] == 1)
                {
                    agent.targetObject = agent.hive.GameObject();
                }
                if (actionBuffer.DiscreteActions[5] == 1)
                {
                    MaintainHive(agent);
                }
                if (actionBuffer.DiscreteActions[6] == 1)
                {
                    DropCarriableItem(agent, CarriableItemType.Food);
                }
                if (actionBuffer.DiscreteActions[7] == 1)
                {
                    Attack(agent, actionBuffer);
                }
                if (actionBuffer.DiscreteActions[8] == 1)
                {
                    ReleasePheromone(agent, agent.transform.position, PheromoneType.Boundary, 100, pheromoneEvaporationRate);
                }
                if (actionBuffer.DiscreteActions[9] == 1)
                {
                    ReleasePheromone(agent, agent.transform.position, PheromoneType.Food, 100, pheromoneEvaporationRate);
                }
                if (actionBuffer.DiscreteActions[10] == 1)
                {
                    ReleasePheromone(agent, agent.transform.position, PheromoneType.Threat, 100, pheromoneEvaporationRate);
                }
                if (actionBuffer.DiscreteActions[11] == 1)
                {
                    ReleasePheromone(agent, agent.transform.position, PheromoneType.Water, 100, pheromoneEvaporationRate);
                }
                if (actionBuffer.DiscreteActions[12] == 1)
                {
                    RestAndRecover(agent);
                }
                if (actionBuffer.DiscreteActions[13] == 1)
                {
                    MoveToNextPheromone(agent);
                }
                if (actionBuffer.DiscreteActions[14] == 1)
                {
                    ReinforcePheromone(agent);
                }
                if (actionBuffer.DiscreteActions[15] == 1)
                {
                    ReinforcePheromone(agent);
                }
                if (actionBuffer.DiscreteActions[16] == 1)
                {
                    ReinforcePheromone(agent);
                }
                if (actionBuffer.DiscreteActions[17] == 1)
                {
                    ReinforcePheromone(agent);
                }
                if (actionBuffer.DiscreteActions[18] == 1)
                {
                    MoveToPreviousPheromone(agent);
                }
                if (actionBuffer.DiscreteActions[19] == 1)
                {
                    DropCarriableItem(agent, CarriableItemType.Water);
                }
                if (actionBuffer.DiscreteActions[20] == 1)
                {
                    PickUpCarriableItem(agent, CarriableItemType.Water);
                }
                if (actionBuffer.DiscreteActions[21] == 1)
                {
                    RemoveActivePheromoneTrail(agent);
                }
            }
        }
    }

    protected override void OnUpdate()
    {
        if (ManualAgentControl)
        {
            OnGameInput(Agents);
        }
        if (ManualAgentPredefinedBehaviourControl)
        {
            OnGameInput(AgentsPredefinedBehaviour);
        }
    }
    private void UpdatePheromoneTrails()
    {
        var pheromoneTrails = FindObjectsOfType<PheromoneTrailComponent>();
        foreach (var pheromoneTrail in pheromoneTrails)
        {
            pheromoneTrail.UpdatePheromones();
        }
    }
    public void RemoveActivePheromoneTrail(AntAgentComponent ant)
    {
        if (ant.activePheromoneTrail != null)
        {
            ant.activePheromoneTrail = null;
            ant.currentActiveNodePheromone = null;
            ant.foodPheromoneTrailComponent = null;
            ant.waterPheromoneTrailComponent = null;
        }

    }
    public bool DetectPheromone(Vector3 position, PheromoneType type, float detectionRadius)
    {
        RaySensor2D raySensor = GetComponent<RaySensor2D>();

        if (raySensor != null)
        {
            RayPerceptionInput perceptionInput = new RayPerceptionInput
            {
                RayLength = detectionRadius,
                DetectableTags = new List<string> { type.ToString() },
                Angles = new List<float> { 0 },
                CastRadius = 0.5f,
                Transform = transform,
                CastType = RayPerceptionCastType.Cast2D,
                LayerMask = LayerMask.GetMask("Pheromones")
            };

            SensorPerceiveOutput[] perceptionOutputs = raySensor.Perceive();

            foreach (var output in perceptionOutputs)
            {
                if (output.HasHit && output.HitGameObjects != null)
                {
                    foreach (var hitObject in output.HitGameObjects)
                    {
                        if (hitObject != null && hitObject.CompareTag(type.ToString()))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    public void ReleasePheromone(AntAgentComponent ant, Vector2 position, PheromoneType type, float strength, float evaporationRate)
    {
        PheromoneTrailComponent currentPheromoneTrail;
        if (type == PheromoneType.Water)
        {
            currentPheromoneTrail = ant.waterPheromoneTrailComponent;
            if (currentPheromoneTrail == null)
            {
                currentPheromoneTrail = ant.gameObject.AddComponent<PheromoneTrailComponent>();
                currentPheromoneTrail.pheromoneType = PheromoneType.Water;
                ant.activePheromoneTrail = currentPheromoneTrail;
                ant.waterPheromoneTrailComponent = currentPheromoneTrail;
            }

        }
        else if (type == PheromoneType.Food)
        {
            currentPheromoneTrail = ant.foodPheromoneTrailComponent;
            if (currentPheromoneTrail == null)
            {
                currentPheromoneTrail = ant.gameObject.AddComponent<PheromoneTrailComponent>();
                currentPheromoneTrail.pheromoneType = PheromoneType.Food;
                ant.activePheromoneTrail = currentPheromoneTrail;
                ant.foodPheromoneTrailComponent = currentPheromoneTrail;
            }

        }
        else if (type == PheromoneType.Threat)
        {
            currentPheromoneTrail = ant.threatPheromoneTrailComponent;
            if (currentPheromoneTrail == null)
            {
                currentPheromoneTrail = ant.gameObject.AddComponent<PheromoneTrailComponent>();
                currentPheromoneTrail.pheromoneType = PheromoneType.Threat;
                ant.activePheromoneTrail = currentPheromoneTrail;
                ant.threatPheromoneTrailComponent = currentPheromoneTrail;
            }

        }
        else
        {
            currentPheromoneTrail = ant.boundaryPheromoneTrailComponent;
            if (currentPheromoneTrail == null)
            {
                currentPheromoneTrail = ant.gameObject.AddComponent<PheromoneTrailComponent>();
                currentPheromoneTrail.pheromoneType = PheromoneType.Boundary;
                ant.activePheromoneTrail = currentPheromoneTrail;
                ant.boundaryPheromoneTrailComponent = currentPheromoneTrail;
            }

        }
        if (currentPheromoneTrail != null)
        {
            currentPheromoneTrail.AddPheromone(position, strength, evaporationRate);
            ant.currentActiveNodePheromone = currentPheromoneTrail.lastNode;
        }
        ant.AgentFitness.Fitness.UpdateFitness(AntColonyFitness.FitnessValues[AntColonyFitness.FitnessKeys.AgentPheromoneRelease.ToString()], AntColonyFitness.FitnessKeys.AgentPheromoneRelease.ToString());

    }
    void PickUpCarriableItem(AntAgentComponent antAgent, CarriableItemType itemType)
    {
        if (antAgent.detectCarriableItem != null && itemType == CarriableItemType.Water)
        {
            Vector2 targetPosition = antAgent.detectCarriableItem.transform.position;
            Vector2 direction = (targetPosition - (Vector2)antAgent.transform.position).normalized;
            antAgent.transform.position = Vector2.MoveTowards(antAgent.transform.position, targetPosition, AntMoveSpeed * Time.deltaTime);

            if (Vector2.Distance(antAgent.transform.position, targetPosition) < 0.1f)
            {
                var pickedUpItemRepresentation = Instantiate(WaterPrefab, antAgent.transform);
                antAgent.carriedItemObject = pickedUpItemRepresentation;
                antAgent.detectCarriableItem.SetActive(false);
                if (antAgent.carriedItemObject != null)
                {
                    antAgent.targetObject = antAgent.hive.GameObject();
                    if (antAgent.carriedItemObject.TryGetComponent<Collider2D>(out Collider2D collider))
                    {
                        collider.enabled = false;
                    }
                }

            }
        }
        else if (antAgent.detectCarriableItem != null && itemType == CarriableItemType.Food)
        {
            Vector2 targetPosition = antAgent.detectCarriableItem.transform.position;
            Vector2 direction = (targetPosition - (Vector2)antAgent.transform.position).normalized;

            // Move the ant towards the food
            antAgent.transform.position = Vector2.MoveTowards(antAgent.transform.position, targetPosition, AntMoveSpeed * Time.deltaTime);

            // Check if the ant is close enough to the food
            if (Vector2.Distance(antAgent.transform.position, targetPosition) < 0.1f)
            {
                var pickedUpItemRepresentation = Instantiate(FoodPrefab, antAgent.transform);
                antAgent.carriedItemObject = pickedUpItemRepresentation;
                antAgent.detectCarriableItem.SetActive(false);
                if (antAgent.carriedItemObject != null)
                {
                    antAgent.targetObject = antAgent.hive.GameObject();
                    if (antAgent.carriedItemObject.TryGetComponent<Collider2D>(out Collider2D collider))
                    {
                        collider.enabled = false;
                    }
                }
            }
        }
    }

    void MoveToHive(AntAgentComponent antAgent)
    {
        Vector2 hivePosition = antAgent.hive.transform.position;
        Vector2 antPosition = antAgent.transform.position;

        Vector2 direction = (hivePosition - antPosition).normalized;
        antAgent.MoveDirection.Set(direction.x, direction.y);

        // Calculate the target rotation angle
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        antAgent.Rigidbody.rotation = targetAngle;
        float currentAngle = antAgent.transform.eulerAngles.z;

        float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, AntRotationSpeed * Time.fixedDeltaTime);
        antAgent.transform.rotation = Quaternion.Euler(0, 0, newAngle);

        RaycastHit2D hit = Physics2D.Raycast(antPosition, direction, AntMoveSpeed * Time.fixedDeltaTime);

        if (hit.collider == null || (!hit.collider.CompareTag("food") && !hit.collider.CompareTag("food")))
        {
            antAgent.transform.position = Vector2.MoveTowards(antPosition, hivePosition, AntMoveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            Vector2 avoidDirection = Vector2.Perpendicular(direction);
            avoidDirection *= (Random.value > 0.5f ? 1 : -1);
            antAgent.transform.position = Vector2.MoveTowards(antPosition, antPosition + avoidDirection, AntMoveSpeed * Time.fixedDeltaTime);
        }

    }
    void DropCarriableItem(AntAgentComponent antAgent, CarriableItemType itemType)
    {
        if (itemType == CarriableItemType.Water && antAgent.carriedItemObject != null)
        {
            Vector2 agentPosition = antAgent.transform.position;
            gatheredWaterItems.Add(antAgent.carriedItemObject);
            Destroy(antAgent.carriedItemObject);
            antAgent.carriedItemObject = null;



        }
        else if (itemType == CarriableItemType.Food && antAgent.carriedItemObject != null)
        {
            Vector2 agentPosition = antAgent.transform.position;
            gatheredFoodItems.Add(antAgent.carriedItemObject);
            Destroy(antAgent.carriedItemObject);
            antAgent.carriedItemObject = null;


        }
        antAgent.AgentFitness.Fitness.UpdateFitness(AntColonyFitness.FitnessValues[AntColonyFitness.FitnessKeys.AgentObjectGathered.ToString()], AntColonyFitness.FitnessKeys.AgentObjectGathered.ToString());

    }
    void ReinforcePheromone(AntAgentComponent antAgent)
    {
        antAgent.currentActiveNodePheromone.intensity += 10;
        antAgent.AgentFitness.Fitness.UpdateFitness(AntColonyFitness.FitnessValues[AntColonyFitness.FitnessKeys.AgentPheromoneReinforce.ToString()], AntColonyFitness.FitnessKeys.AgentPheromoneReinforce.ToString());

    }
    void RestAndRecover(AntAgentComponent antAgent)
    {
        antAgent.stamina += Time.deltaTime * recoveryRate;
        if (antAgent.stamina > antAgent.maxStamina)
        {
            antAgent.stamina = antAgent.maxStamina;
        }
    }
    public void Attack(AntAgentComponent agent, ActionBuffer actionBuffer)
    {
        Vector2 position = agent.transform.position;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, agent.attackRange);

        foreach (Collider2D collider in hitColliders)
        {
            Threat threat = collider.GetComponent<Threat>();
            if (threat != null)
            {
                threat.health -= 10;
                break;
            }
        }
    }

    public void MoveToNextPheromone(AntAgentComponent agent)
    {
        PheromoneTrailComponent trailComponent = agent.activePheromoneTrail;

        if (trailComponent != null)
        {
            PheromoneNodeComponent currentNode = agent.currentActiveNodePheromone;

            if (currentNode != null && currentNode.next != null)
            {
                agent.transform.position = Vector3.MoveTowards(agent.transform.position, currentNode.next.position, AntMoveSpeed * Time.deltaTime);
                if (Vector3.Distance(agent.transform.position, currentNode.next.position) < 0.1f)
                {
                    agent.currentActiveNodePheromone = currentNode.next;
                }
            }
        }
    }
    public void MoveToPreviousPheromone(AntAgentComponent agent)
    {
        PheromoneNodeComponent currentNode = agent.currentActiveNodePheromone;

        if (currentNode != null && currentNode.previous != null)
            if (currentNode.position == currentNode.previous.position)
            {
                agent.currentActiveNodePheromone = currentNode.previous;
            }
            else
            {
                float distance = Vector3.Distance(agent.transform.position, currentNode.previous.position);
                if (distance > 0)
                {
                    agent.transform.position = Vector3.MoveTowards(agent.transform.position, currentNode.previous.position, AntMoveSpeed * Time.deltaTime);

                    if (Vector3.Distance(agent.transform.position, currentNode.previous.position) < 0.4f)
                    {
                        agent.currentActiveNodePheromone = currentNode.previous;
                    }
                }
            }

    }

    public void MaintainHive(AntAgentComponent agent)
    {
        if (agent.hive != null)
        {
            agent.hive.Repair(10);
        }
    }
    void OnGameInput(AgentComponent[] agents)
    {
        foreach (AntAgentComponent agent in agents)
        {
            if (agent.gameObject.activeSelf && agent.enabled)
            {

                if (Input.GetKey(agent.InputUp))
                {
                    agent.SetDirection(Vector2.up);
                }
                else if (Input.GetKey(agent.InputDown))
                {
                    agent.SetDirection(Vector2.down);
                }
                else if (Input.GetKey(agent.InputLeft))
                {
                    agent.SetDirection(Vector2.left);
                }
                else if (Input.GetKey(agent.InputRight))
                {
                    agent.SetDirection(Vector2.right);
                }
                else if (agent.carriedItemObject != null && Input.GetKeyDown(agent.dropPickUpKey))
                {
                    agent.carriedItemObject = null;
                    Vector2 agentPosition = agent.transform.position;
                    var foodItem = Instantiate(FoodPrefab, agentPosition, Quaternion.identity);
                    FoodItems.Add(foodItem);

                }
                else if (agent.carriedItemObject != null && Input.GetKeyDown(agent.dropPickUpKey))
                {
                    Vector2 agentPosition = agent.transform.position;

                    foreach (GameObject food in FoodItems)
                    {
                        if (Vector2.Distance(agentPosition, food.transform.position) < 2.0f)
                        {
                            if (agent.carriedItemObject == null)
                            {
                                agent.carriedItemObject = food;
                                Destroy(food);
                                break;
                            }
                        }
                    }

                }
                else if (Input.GetKey(agent.dropPheromoneKey))
                {
                    Vector2 agentPosition = agent.transform.position;
                    var pheromone = Instantiate(PheromonePrefab, agentPosition, Quaternion.identity, this.gameObject.transform);
                    if (agent.foodPheromoneTrailComponent != null)
                    {
                        agent.foodPheromoneTrailComponent.AddPheromone(agentPosition, 100, agent.pheromoneEvaporationRate);
                    }
                    else
                    {
                        GameObject pheromoneTrailObject = new GameObject("PheromoneTrail");
                        PheromoneTrailComponent pheromoneTrailComponent = pheromoneTrailObject.AddComponent<PheromoneTrailComponent>();
                        agent.foodPheromoneTrailComponent = pheromoneTrailComponent;
                        agent.foodPheromoneTrailComponent.AddPheromone(agentPosition, 100, agent.pheromoneEvaporationRate);
                    }
                }
                else
                {
                    agent.SetDirection(Vector2.zero);
                }
            }
        }
    }


    void MoveAgent(AntAgentComponent agent, ActionBuffer actionBuffer)
    {
        // Check if there's a target object to move towards
        if (agent.targetObject)
        {
            MoveToObject(agent);
        }
        else
        {
            // Standard movement logic when no target is set
            var dirToGo = Vector2.zero;
            float rotateAngle = 0f;

            var forwardAxis = actionBuffer.DiscreteActions[0];
            var rotateAxis = actionBuffer.DiscreteActions[2];

            switch (forwardAxis)
            {
                case 1:
                    dirToGo = agent.transform.up * AntMoveSpeed;
                    break;
                case 2:
                    dirToGo = -agent.transform.up * AntMoveSpeed;
                    break;
            }

            switch (rotateAxis)
            {
                case 1:
                    rotateAngle = -AntRotationSpeed * Time.fixedDeltaTime;
                    break;
                case 2:
                    rotateAngle = AntRotationSpeed * Time.fixedDeltaTime;
                    break;
            }

            // Apply movement
            agent.Rigidbody.MovePosition(agent.Rigidbody.position + (dirToGo * Time.fixedDeltaTime));
            float newRotation = agent.Rigidbody.rotation + rotateAngle;
            agent.Rigidbody.MoveRotation(newRotation);
        }
    }

    void MoveToObject(AntAgentComponent agent)
    {
        // Positions
        Vector2 hivePosition = agent.targetObject.transform.position;
        Vector2 antPosition = agent.transform.position;

        // Direction to target
        Vector2 direction = (hivePosition - antPosition).normalized;

        // Obstacle Detection (Raycast)
        RaycastHit2D hit = Physics2D.Raycast(antPosition, direction, AntMoveSpeed * Time.fixedDeltaTime, agent.detectionLayerMask);

        if (hit.collider == null || hit.collider.gameObject == agent.gameObject || (agent.carriedItemObject != null && hit.collider.gameObject == agent.carriedItemObject)) // No obstacle
        {
            // Move directly towards the hive
            agent.transform.position = Vector2.MoveTowards(antPosition, hivePosition, AntMoveSpeed * Time.fixedDeltaTime);

            // Smooth rotation towards the hive
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float currentAngle = agent.transform.eulerAngles.z;
            float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, AntRotationSpeed * Time.fixedDeltaTime);
            agent.transform.rotation = Quaternion.Euler(0, 0, newAngle);
            UnityEngine.Debug.DrawLine(antPosition, hit.point, Color.blue);

        }
        else // Obstacle detected
        {
            // Avoid obstacle
            Vector2 avoidDirection = Vector2.Perpendicular(direction);
            avoidDirection *= (Random.value > 0.5f ? 1 : -1);
            UnityEngine.Debug.DrawLine(antPosition, hit.point, Color.red);

            Vector2 newTargetPosition = antPosition + avoidDirection * AntMoveSpeed * Time.fixedDeltaTime;

            // Move towards avoid direction
            agent.transform.position = Vector2.MoveTowards(antPosition, newTargetPosition, AntMoveSpeed * Time.fixedDeltaTime);
        }
    }

    public void MoveAgents(AgentComponent[] agents)
    {

        foreach (AntAgentComponent agent in agents)
        {
            if (agent.gameObject.activeSelf && agent.enabled)
            {

                if (agent.NextAgentUpdateTime <= CurrentSimulationTime && agent.MoveDirection != Vector2.zero)
                {

                    Rigidbody2D rb = agent.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 movement = agent.MoveDirection.normalized * AntMoveSpeed * Time.deltaTime;

                        rb.MovePosition(rb.position + movement);
                        // agent.stamina--;
                    }

                    agent.NextAgentUpdateTime = CurrentSimulationTime + AgentUpdateinterval;

                }

            }
        }
    }
}