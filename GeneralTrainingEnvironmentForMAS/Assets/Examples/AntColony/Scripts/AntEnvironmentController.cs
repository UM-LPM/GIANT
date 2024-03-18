using System;
using System.Collections.Generic;
using UnityEngine;

public class AntEnvironmentController : EnvironmentControllerBase
{
    [Header("Ant Configuration")]
    [SerializeField] float AntMoveSpeed = 1.0f;
    [SerializeField] float AntRotationSpeed = 90.0f;
    [Header("Ant configuration General")]
    [SerializeField] GameObject FoodPrefab;
    [SerializeField] GameObject HivePrefab;

    [SerializeField] int numFoodItems = 10;
    [SerializeField] int numHives = 5;
    [SerializeField]  public GameObject PheromonePrefab;
    [SerializeField] int AgentStartHealth = 400;


    protected override void DefineAdditionalDataOnStart()
    {
        SpawnFood();
        SpawnHives();
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
        //TODO
    }

    public void IfCarryingFood(AntAgentComponent ant)
    {
        if (ant.hasFood && Vector3.Distance(ant.transform.position, HivePrefab.transform.position) < 1.0f)
        {
            ant.hasFood = false;
            //update fitness with AgentBringFood
            //MoveRandomly
        }
        else
        {
           //moveToHve
        }
    }
    void UpdateAgentsWithBTs(AgentComponent[] agents)
    {
        ActionBuffer actionBuffer;
        foreach (RobocodeAgentComponent agent in agents)
        {
            if (agent.gameObject.activeSelf)
            {
                actionBuffer = new ActionBuffer(null, new int[] { 0, 0, 0, 0, 0,0,0 }); // Forward, Side, Rotate, DropPheromone,MoveRandomDirection,MoveToHive,PickUpFood

                agent.BehaviourTree.UpdateTree(actionBuffer);
              // 
            }
        }

    }
  
    public void MoveToAdjacentPheromone(AntAgentComponent ant)
    {
        //TODO: if the ant sees the pheromone with sesnor, moveForward, otherwise ??
            
    }
    public void MoveToAdjacentFood(AntAgentComponent ant)
    {
        //TODO: if the ant sees the food with sesnor, moveForward, otherwise ??

    }
    public void IfFoodHere(AntAgentComponent ant)
    {
        //TODO: if the ant is on the location where the food is, pick up, otherwise move ??

    }
    void SpawnFood()
    {
        for (int i = 0; i < numFoodItems; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPoint();
            Instantiate(FoodPrefab, spawnPos, Quaternion.identity);
        }
    }

    void SpawnHives()
    {
        for (int i = 0; i < numHives; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPoint();
            Instantiate(HivePrefab, spawnPos, Quaternion.identity);
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
