using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public abstract T Perceive();
}
