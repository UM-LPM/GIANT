using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class AntEnvironmentController1 : EnvironmentControllerBase
{
    [Header("Ant Configuration")]
    [SerializeField] float AntMoveSpeed = 1.0f;
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
    [SerializeField] int numHives = 5;
    [SerializeField] public GameObject PheromonePrefab;
    [SerializeField] int AgentStartHealth = 400;

    public override void UpdateAgents()
    {
        // Update Agents that are being evaluated
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
                    Vector3 agentPosition = agent.transform.position;
                    var foodItem=Instantiate(FoodPrefab, agentPosition, Quaternion.identity);
                    FoodItems.Add(foodItem);

                }
                else if (agent.hasFood == false && Input.GetKeyDown(agent.dropPickUpKey) )
                {
                    Vector3 agentPosition = agent.transform.position;

                    foreach (GameObject food in FoodItems)
                    {
                        if (Vector3.Distance(agentPosition, food.transform.position) < 1.0f)
                        {
                            if (!agent.hasFood)
                            {
                                agent.hasFood = true;
                                Destroy(food);
                                break;
                            }
                        }
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
                      
                    agent.transform.Translate(new Vector3(agent.MoveDirection.x, agent.MoveDirection.y, 0));
                    agent.NextAgentUpdateTime = CurrentSimulationTime + AgentUpdateinterval;
                      
                    }
              
            }
        }
    }
}