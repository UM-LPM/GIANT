using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaySensor3D : RaySensorBase 
{
    [Range(-10f, 10f)]
    [Tooltip("Ray start is offset up or down by this amount.")]
    [SerializeField] public float StartVerticalOffset;

    [Range(-10f, 10f)]
    [Tooltip("Ray end is offset up or down by this amount.")]
    [SerializeField] public float EndVerticalOffset;

    public RaySensor3D() : base("Ray Sensor 3D") {

    }

    public override RayPerceptionCastType GetCastType() {
        return RayPerceptionCastType.Cast3D;
    }

    public override float GetStartVerticalOffset() {
        return StartVerticalOffset;
    }

    public override float GetEndVerticalOffset() {
        return EndVerticalOffset;
    }
}
