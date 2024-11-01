using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class Sensor<T> : MonoBehaviour {

    [Header("Base sensor configuration")]
    [SerializeField] public LayerMask LayerMask;
    [SerializeField] public string[] DetectableTags;
    [SerializeField] public Color BaseSensorColor;
    [SerializeField] public Color HitSensorColor;
    [SerializeField] public bool DrawOnlyHitSensors;
    [SerializeField] public bool DrawGizmos;

    [field: SerializeField]
    public string Name { get; set; }

    public T SensorPerceiveOutputs { get; protected set; }

    public Sensor(string name) {
        Name = name;
    }

    public abstract T PerceiveAll();
    public abstract T PerceiveSingle(int xPos = -1, int yPos = -1, int zPos = -1);

    public abstract T PerceiveRange(int startIndex = -1, int endIndex = -1);

    public abstract void Init();

    private void Awake()
    {
        Init();
    }
}
