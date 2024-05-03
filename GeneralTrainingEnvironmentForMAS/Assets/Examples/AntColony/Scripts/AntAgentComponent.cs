using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntAgentComponent : AgentComponent
{

    public Rigidbody Rigidbody { get; set; }
    public float Health { get; set; }
    public bool hasFood { get; set; }
    [Header("Controlls")]
    [SerializeField] public KeyCode dropPickUpKey = KeyCode.Space;
    [SerializeField] public KeyCode dropPheromoneKey = KeyCode.LeftShift;
    [SerializeField] public KeyCode InputUp = KeyCode.W;
    [SerializeField] public KeyCode InputDown = KeyCode.S;
    [SerializeField] public KeyCode InputLeft = KeyCode.A;
    [SerializeField] public KeyCode InputRight = KeyCode.D;
    public Vector2 MoveDirection { get; private set; }

    public float NextAgentUpdateTime { get; set; }

    public PheromoneNodeComponent currentPheromoneNode { get; set; }
    public PheromoneTrailComponent pheromoneTrailComponent { get; set; }
    protected override void DefineAdditionalDataOnAwake()
    {
        Rigidbody = GetComponent<Rigidbody>(); 

    }
    public void SetDirection(Vector2 newDirection)
    {
        MoveDirection = newDirection;
    }
    }
