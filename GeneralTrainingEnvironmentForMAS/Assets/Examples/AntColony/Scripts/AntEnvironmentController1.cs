using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
public class AntEnvironmentController1 : EnvironmentControllerBase
{
    [Header("Ant Configuration")]
    [SerializeField] float AntMoveSpeed = 2.0f;
    [SerializeField] public float AntRotationSpeed = 90.0f;
    [Header("Ant configuration General")]
    [SerializeField] GameObject FoodPrefab;
    [SerializeField] GameObject HivePrefab;
    [SerializeField] public float AgentUpdateinterval = 0.1f;

    private List<GameObject> foodItems = new List<GameObject>();
    public List<GameObject> FoodItems => foodItems;

    private List<GameObject> hiveItems = new List<GameObject>();
    public List<GameObject> HiveItems => hiveItems;
    [SerializeField] int numFoodItems = 10;
    [SerializeField] int numHives = 1;
    [SerializeField] int numOfAnts = 10;
    [SerializeField] public float pheromoneEvaporationRate = 1.0f;
    [SerializeField] public GameObject PheromonePrefab;
    [SerializeField] int AgentStartHealth = 400;
    protected override void DefineAdditionalDataOnPreStart()
    {
        SpawnHives();
        SpawnFood();

        for (int i = 0; i < numOfAnts; i++)
        {
            GameObject agent = Instantiate(AgentPrefab, hiveItems[0].transform.position, Quaternion.identity, this.gameObject.transform);

        }
    }
 
    protected override void DefineAdditionalDataOnPostStart() {
        foreach (AntAgentComponent agent in Agents)
      {
           agent.Rigidbody = agent.GetComponent<Rigidbody>();
           agent.Health = AgentStartHealth;
            agent.pheromoneEvaporationRate = pheromoneEvaporationRate;
      }

      foreach (AntAgentComponent agent in AgentsPredefinedBehaviour)
      {
          agent.Rigidbody = agent.GetComponent<Rigidbody>();
          Instantiate(agent.gameObject, hiveItems[0].transform, this.gameObject.transform);

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

    void SpawnHives()
    {
       
            Vector2 spawnPos = GetRandomSpawnPoint();
            GameObject hiveItem = Instantiate(HivePrefab, spawnPos, Quaternion.identity, this.gameObject.transform);
            hiveItems.Add(hiveItem);
       
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
            MoveAgents(Agents);
        
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
        UpdatePheromoneTrails();
    }
    private void UpdatePheromoneTrails()
    {
       var pheromoneTrails = FindObjectsOfType<PheromoneTrailComponent>();
        foreach(var pheromoneTrail in pheromoneTrails)
        {
            pheromoneTrail.UpdatePheromones();
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
                else if (agent.hasFood == true && Input.GetKeyDown(agent.dropPickUpKey))
                {
                    agent.hasFood = false;
                    Vector2 agentPosition = agent.transform.position;
                    var foodItem=Instantiate(FoodPrefab, agentPosition, Quaternion.identity);
                    FoodItems.Add(foodItem);

                }
                else if (agent.hasFood == false && Input.GetKeyDown(agent.dropPickUpKey) )
                {
                    Vector2 agentPosition = agent.transform.position;

                    foreach (GameObject food in FoodItems)
                    {
                        if (Vector2.Distance(agentPosition, food.transform.position) < 2.0f)
                        {
                            if (!agent.hasFood)
                            {
                                agent.hasFood = true;
                                Destroy(food);
                                break;
                            }
                        }
                    }

                }else if(Input.GetKey(agent.dropPheromoneKey)){
                    Vector2 agentPosition = agent.transform.position;
                    var pheromone = Instantiate(PheromonePrefab, agentPosition, Quaternion.identity, this.gameObject.transform);
                    if(agent.pheromoneTrailComponent != null)
                    {
                        agent.pheromoneTrailComponent.AddPheromone(agentPosition,100, agent.pheromoneEvaporationRate);
                    }
                    else
                    {
                        GameObject pheromoneTrailObject = new GameObject("PheromoneTrail");
                        PheromoneTrailComponent pheromoneTrailComponent = pheromoneTrailObject.AddComponent<PheromoneTrailComponent>();
                        agent.pheromoneTrailComponent = pheromoneTrailComponent;
                        agent.pheromoneTrailComponent.AddPheromone(agentPosition,100, agent.pheromoneEvaporationRate);
                    }
                }
                else
                {
                    agent.SetDirection(Vector2.zero);
                }
            }
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
                        agent.Health--;
                    }
                    
                    agent.NextAgentUpdateTime = CurrentSimulationTime + AgentUpdateinterval;
                      
                    }
              
            }
        }
    }
}