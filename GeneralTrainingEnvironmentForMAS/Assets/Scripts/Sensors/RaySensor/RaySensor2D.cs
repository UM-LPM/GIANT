using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaySensor2D : RaySensorBase {

    public RaySensor2D() : base("Ray Sensor 2D") {

    }

    public override RayPerceptionCastType GetCastType() {
        return RayPerceptionCastType.Cast2D;
    }
}
