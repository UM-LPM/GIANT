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
    [SerializeField] GameObject PheromonePrefab;


    protected override void DefineAdditionalDataOnStart()
    {
    }

    protected override void OnUpdate()
    {
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
                //just an example
                Vector3 moveDirection = ant.transform.forward * AntMoveSpeed * Time.fixedDeltaTime;
                ant.Rigidbody.MovePosition(ant.Rigidbody.position + moveDirection);

            }
        }
    }

    void UpdateAntBehavior()
    {

    }

    public void AntCollision(AntAgentComponent ant1, AntAgentComponent ant2)
    {
    }


    public void FoodFound(AntAgentComponent ant)
    {
    }

    public void NestReached(AntAgentComponent ant)
    {
    }
    public void DropedFood(AntAgentComponent ant)
    {
    }

}
