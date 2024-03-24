using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AntEnvironmentController : EnvironmentControllerBase
{
    [Header("Ant Configuration")]
    [SerializeField] float AntMoveSpeed = 1.0f;
    [SerializeField] public  float AntRotationSpeed = 90.0f;
    [Header("Ant configuration General")]
    [SerializeField] GameObject FoodPrefab;
    [SerializeField] GameObject HivePrefab;
    private List<GameObject> foodItems = new List<GameObject>();
    public List<GameObject> FoodItems => foodItems;

    private List<GameObject> hiveItems = new List<GameObject>();
    public List<GameObject> HiveItems => hiveItems;
    [SerializeField] int numFoodItems = 10;
    [SerializeField] int numHives = 5;
    [SerializeField]  public GameObject PheromonePrefab;
    [SerializeField] int AgentStartHealth = 400;


    protected override void DefineAdditionalDataOnStart()
    {
        SpawnHives();
        SpawnFood();
    }

    protected override void OnUpdate()
    {
        //TODO:
    }

    public override void UpdateAgents()
    {
        MoveAnts();
        UpdateAntBehavior();
    }

    void MoveAnts()
    {
        foreach (AntAgentComponent ant in Agents)
        {
            if (ant.gameObject.activeSelf)
            {

                Vector3 moveDirection = ant.transform.forward * AntMoveSpeed * Time.fixedDeltaTime;
                ant.Rigidbody.MovePosition(ant.Rigidbody.position + moveDirection);
            }
        }
    }

    void UpdateAntBehavior()
    {
        {
            UpdateAgentsWithBTs(AgentsPredefinedBehaviour);
        }

        //Todo:
    }

    public void IfCarryingFood(AntAgentComponent ant)
    {
        if (ant.hasFood && Vector3.Distance(ant.transform.position, HivePrefab.transform.position) < 1.0f)
        {
            ant.hasFood = false;
            //update fitness with AgentBringFood- it should be more then 1.
            //MoveRandomly
        }
        else
        {
           //moveToHve
        }
    }
    void MoveAntAgent(AntAgentComponent agent, ActionBuffer actionBuffer)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;
        var rotateTurrentDir = Vector3.zero;

        var forwardAxis = actionBuffer.DiscreteActions[0];
        var rightAxis = actionBuffer.DiscreteActions[1];
        var rotateAxis = actionBuffer.DiscreteActions[2];
        var rotateTurrentAxis = actionBuffer.DiscreteActions[3];

        switch (forwardAxis)
        {
            case 1:
                dirToGo = agent.transform.forward * 1f ;
                break;
            case 2:
                dirToGo = agent.transform.forward * -1f;
                break;
        }

        switch (rotateAxis)
        {
            case 1:
                rotateDir = agent.transform.up * -1f;
                break;
            case 2:
                rotateDir = agent.transform.up * 1f;
                break;
        }

        switch (rotateTurrentAxis)
        {
            case 1:
                rotateTurrentDir = agent.transform.up * -1f;
                break;
            case 2:
                rotateTurrentDir = agent.transform.up * 1f;
                break;
        }

        agent.Rigidbody.MovePosition(agent.Rigidbody.position + (dirToGo * AntMoveSpeed * Time.fixedDeltaTime));
        Quaternion turnRotation = Quaternion.Euler(0.0f, rotateDir.y * Time.fixedDeltaTime * AntRotationSpeed, 0.0f);
        agent.Rigidbody.MoveRotation(agent.Rigidbody.rotation * turnRotation);

    }
        void UpdateAgentsWithBTs(AgentComponent[] agents)
                {
                    ActionBuffer actionBuffer;
                    foreach (AntAgentComponent agent in agents)
                    {
                        if (agent.gameObject.activeSelf)
                        {
                            actionBuffer = new ActionBuffer(null, new int[] { 0, 0, 0, 0, 0,0,0 }); // Forward, Side, Rotate, DropPheromone,MoveToHive,PickUpFood

                            agent.BehaviourTree.UpdateTree(actionBuffer);
                         
                            MoveAntAgent(agent, actionBuffer);

                        if (actionBuffer.DiscreteActions[3] == 1)
                            {
                                DropPheromone(agent);
                            }
                            if (actionBuffer.DiscreteActions[4] == 1)
                            {
                                MoveToHive(agent);
                            }
                            if (actionBuffer.DiscreteActions[5] == 1)
                            {
                                PickUpFood(agent);
                            }

                        }
                    }

                }

  
    public void MoveToAdjacentPheromoneElse(AntAgentComponent ant)
    {
            
    }
    public void MoveToAdjacentFood(AntAgentComponent ant)
    {
        //TODO: if the ant sees the food with sesnor, moveForward, otherwise ??

    }
    public void IfFoodHere(AntAgentComponent ant)
    {
        //TODO: if the ant is on the location where the food is, pick up, otherwise move ??

    }
    void PickUpFood(AntAgentComponent agent)
    {
        AntAgentComponent antComponent = agent.GetComponent<AntAgentComponent>();

        Vector3 agentPosition = agent.transform.position;

        foreach (GameObject food in FoodItems)
        {
            if (Vector3.Distance(agentPosition, food.transform.position) < 1.0f)
            {
                if (!antComponent.hasFood)
                {
                    antComponent.hasFood = true;
                    Destroy(food);
                    break; 
                }
            }
        }
    }

    void MoveToHive(AntAgentComponent agent)
    {

        Vector3 agentPosition = agent.transform.position;
        Vector3 hivePosition = HiveItems[0].transform.position;

        Vector3 directionToHive = (hivePosition - agentPosition).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToHive);
        agent.transform.rotation = Quaternion.RotateTowards(agent.transform.rotation, targetRotation, Time.deltaTime * AntRotationSpeed);

        Vector3 moveDirection = agent.transform.forward * AntMoveSpeed * Time.fixedDeltaTime;
        agent.Rigidbody.MovePosition(agent.Rigidbody.position + moveDirection);
    }

    void DropPheromone(AntAgentComponent agent)
    {
        Vector3 agentPosition = agent.transform.position;

        Instantiate(PheromonePrefab, agentPosition, Quaternion.identity);
    }

    void SpawnFood()
    {
        float minDistance = 5.0f;

        float minAngle = 30.0f; 
        for (int i = 0; i < numFoodItems; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPoint();
            float angle;
            do
            {
                spawnPos = GetRandomSpawnPoint();
                Vector3 direction = (spawnPos - hiveItems[0].transform.position).normalized;

                 angle = Vector3.Angle(hiveItems[0].transform.forward, direction);

            } while (Vector3.Distance(spawnPos, hiveItems[0].transform.position) < minDistance || angle < minAngle);


            GameObject foodItem = Instantiate(FoodPrefab, spawnPos, Quaternion.identity);
            foodItems.Add(foodItem);
        }
    }

    void SpawnHives()
    {
        for (int i = 0; i < numHives; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPoint();
           GameObject hiveItem= Instantiate(HivePrefab, spawnPos, Quaternion.identity);
            hiveItems.Add(hiveItem);
        }
    }

    Vector3 GetRandomSpawnPoint()
    {
        return new Vector3
        {
            x = Util.NextFloat(-(ArenaSize.x / 2) + ArenaOffset, (ArenaSize.x / 2) - ArenaOffset),
            y = Util.NextFloat(-(ArenaSize.x / 2) + ArenaOffset, (ArenaSize.x / 2) - ArenaOffset),
            z = 0,
        };
    }

}
