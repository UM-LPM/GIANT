using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaySensor2D : RaySensorBase {
    [Range(-10f, 10f)]
    [Tooltip("Ray start is offset up or down by this amount.")]
    [SerializeField] public float StartVerticalOffset;

    public RaySensor2D() : base("Ray Sensor 2D") {

    }

    public override RayPerceptionCastType GetCastType() {
        return RayPerceptionCastType.Cast2D;
    }

    public override float GetStartVerticalOffset()
    {
        return StartVerticalOffset;
    }
}
