using UnityEngine;

public class AntAgentComponent : AgentComponent
{
    public float detectionRadius = 3f;

    public Rigidbody2D Rigidbody { get; set; }
    public float pheromoneEvaporationRate { get; set; }
    public GameObject targetObject { get; set; }
    public GameObject detectCarriableItem { get; set; }
    public GameObject carriedItemObject { get; set; }
   public  LayerMask detectionLayerMask = 65535;


    [Header("Controlls")]
    [SerializeField] public KeyCode dropPickUpKey = KeyCode.Space;
    [SerializeField] public KeyCode dropPheromoneKey = KeyCode.LeftShift;
    [SerializeField] public KeyCode dropAlarmPheromone = KeyCode.RightShift;
    [SerializeField] public KeyCode InputUp = KeyCode.W;
    [SerializeField] public KeyCode InputDown = KeyCode.S;
    [SerializeField] public KeyCode InputLeft = KeyCode.A;
    [SerializeField] public KeyCode InputRight = KeyCode.D;
    public Vector2 MoveDirection { get; private set; }

    public float NextAgentUpdateTime { get; set; }
    public float stamina;
    [SerializeField] public float maxStamina = 100f;
    [SerializeField] public float restThreshold = 50f;
    [SerializeField] public float recoveryRate = 10f;
    [SerializeField] public float attackRange = 10f;

    public Hive hive { get; set; }
    public PheromoneNodeComponent currentActiveNodePheromone { get; set; }
    public PheromoneTrailComponent activePheromoneTrail { get; set; }

    public PheromoneNodeComponent currentFoodPheromoneNode { get; set; }
    public PheromoneTrailComponent foodPheromoneTrailComponent { get; set; }

    public PheromoneNodeComponent currentWaterPheromoneNode { get; set; }
    public PheromoneTrailComponent waterPheromoneTrailComponent { get; set; }
    public PheromoneNodeComponent currentBoundaryPheromoneNode { get; set; }
    public PheromoneTrailComponent boundaryPheromoneTrailComponent { get; set; }
    public PheromoneNodeComponent currentThreatPheromoneNode { get; set; }
    public PheromoneTrailComponent threatPheromoneTrailComponent { get; set; }
    public void SetDirection(Vector2 newDirection)
    {
        MoveDirection = newDirection;
    }
}
