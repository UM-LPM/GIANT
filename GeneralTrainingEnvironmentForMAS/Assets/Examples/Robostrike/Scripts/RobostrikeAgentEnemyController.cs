using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;


public enum Goals
{
    FindHealth,
    FindAmmo,
    FindShield,
    ShootAgent,
    Wander
}

public enum DetectableObjects
{
    Agent,
    Obstacle,
    Wall,
    Missile,
    HealthBox,
    ShieldBox,
    AmmoBox
}

public class RobostrikeAgentEnemyContoller: MonoBehaviour
{
    private RobostrikeEnvironmentController EnvironmentController;
    private RobostrikeAgentComponent AgentComponent;
    private NavMeshAgent NavMeshAgent;

    private RaySensorBase RaySensor;
    private SensorPerceiveOutput[] SensorPerceiveOutputs;
    private GameObject DetectedObject;

    private Goals LastGoal;
    private Goals ActiveGoal;

    private void Awake()
    {
        EnvironmentController = GetComponentInParent<RobostrikeEnvironmentController>();
        AgentComponent = GetComponent<RobostrikeAgentComponent>();
        NavMeshAgent = GetComponent<NavMeshAgent>();

        LastGoal = Goals.Wander;
        ActiveGoal = Goals.Wander;
    }

    private void Start()
    {

    }

    private void FixedUpdate()
    {
        Debug.Log("Agent goal: " + ActiveGoal.ToString());

        NavMeshAgent.isStopped = false;

        // Based on the current state of the agent, decide which goal to pursue
        if(AgentComponent.HealthComponent.Health < 2)
        {
            LastGoal = ActiveGoal;
            ActiveGoal = Goals.FindHealth;
            
            if(LastGoal != ActiveGoal)
            {
                NavMeshAgent.SetDestination(GetRandomPosition());
            }
        }
        else if(AgentComponent.AmmoComponent.Ammo < 2)
        {
            LastGoal = ActiveGoal;
            ActiveGoal = Goals.FindAmmo;

            if (LastGoal != ActiveGoal)
            {
                NavMeshAgent.SetDestination(GetRandomPosition());
            }
        }
        else if(ObjectDetected(DetectableObjects.Agent))
        {
            LastGoal = ActiveGoal;
            ActiveGoal = Goals.ShootAgent;
        }
        else if(AgentComponent.ShieldComponent.Shield < 2)
        {
            LastGoal = ActiveGoal;
            ActiveGoal = Goals.FindShield;

            if (LastGoal != ActiveGoal)
            {
                NavMeshAgent.SetDestination(GetRandomPosition());
            }
        }
        else
        {
            LastGoal = ActiveGoal;
            ActiveGoal = Goals.Wander;

            if (LastGoal != ActiveGoal)
            {
                NavMeshAgent.SetDestination(GetRandomPosition());
            }
        }

        // Execute the action to perceieve the current goal
        switch(ActiveGoal)
        {
            case Goals.FindHealth:
                FindHealth();
                break;
            case Goals.FindAmmo:
                FindAmmo();
                break;
            case Goals.ShootAgent:
                ShootAgent();
                break;
            case Goals.FindShield:
                FindShield();
                break;
            case Goals.Wander:
                Wander();
                break;
        }
    }

    public Vector3 GetRandomPosition()
    {
        Vector3 randomPosition = new Vector3(EnvironmentController.Util.NextFloat(-13.5f, 13.5f), 0.5f, EnvironmentController.Util.NextFloat(-13.5f, 13.5f));
        return randomPosition;
    }

    public bool ObjectDetected(DetectableObjects detectableObject)
    {
        if (RaySensor == null)
        {
            RaySensor = AgentComponent.gameObject.GetComponentInChildren<RaySensorBase>();
            RaySensor.SetLayerMask((1 << RaySensor.gameObject.layer) + 1); // base layer + default
        }

        //SensorPerceiveOutputs = RaySensor.PerceiveSingle(xPos: 0);
        SensorPerceiveOutputs = RaySensor.PerceiveAll();

        foreach(SensorPerceiveOutput sensorPerceiveOutput in SensorPerceiveOutputs)
        {
            if(sensorPerceiveOutput != null && sensorPerceiveOutput.HasHit && sensorPerceiveOutput.HitGameObjects[0].name.Contains(detectableObject.ToString()))
            {
                DetectedObject = sensorPerceiveOutput.HitGameObjects[0];
                return true;
            }
        }

        return false;
    }

    public void FindHealth()
    {
        if (ObjectDetected(DetectableObjects.HealthBox) && DetectedObject != null && NavMeshAgent.destination != DetectedObject.transform.position)
        {
            NavMeshAgent.SetDestination(DetectedObject.transform.position);
        }
        else
        {
            Wander();
        }
    }

    public void FindAmmo()
    {
        if (ObjectDetected(DetectableObjects.AmmoBox) && DetectedObject != null && NavMeshAgent.destination != DetectedObject.transform.position)
        {
            NavMeshAgent.SetDestination(DetectedObject.transform.position);
        }
        else
        {
            Wander();
        }
    }

    public void FindShield()
    {
        if (ObjectDetected(DetectableObjects.ShieldBox) && DetectedObject != null && NavMeshAgent.destination != DetectedObject.transform.position)
        {
            NavMeshAgent.SetDestination(DetectedObject.transform.position);
        }
        else
        {
            Wander();
        }
    }

    public void ShootAgent()
    {
        if(DetectedObject != null)
        {
            NavMeshAgent.SetDestination(DetectedObject.transform.position);
        }

        if(NavMeshAgent.remainingDistance < 4.0f)
        {
            NavMeshAgent.isStopped = true;
            // Slowly Rotate the agent towards the detected object
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(DetectedObject.transform.position - transform.position), 0.2f);
        }
        else
        {
            NavMeshAgent.isStopped = false;
        }

        if (AgentComponent.AmmoComponent.Ammo > 0)
        {
            for (int i = 0; i < 3; i++)
            {
                if (SensorPerceiveOutputs[i].HasHit && SensorPerceiveOutputs[i].HitGameObjects[0].name.Contains(DetectableObjects.Agent.ToString())){
                    EnvironmentController.AgentsShoot(new RobostrikeAgentComponent[] { AgentComponent });
                }
            }
        }
    }

    public void Wander()
    {
        if (NavMeshAgent.destination == null || (NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance))
        {
            NavMeshAgent.SetDestination(GetRandomPosition());
        }
    }

}