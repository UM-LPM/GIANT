﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum RayPerceptionCastType {

    Cast2D,
    Cast3D,
}

public struct RayPerceptionInput {

    public float RayLength;
    public IReadOnlyList<string> DetectableTags;
    public IReadOnlyList<float> Angles;
    public float StartOffset;
    public float EndOffset;
    public float CastRadius;
    public Transform Transform;
    public RayPerceptionCastType CastType;
    public int LayerMask;

    public int OutputSize() {
        return ((DetectableTags?.Count ?? 0) + 2) * (Angles?.Count ?? 0);
    }


    public (Vector3 StartPositionWorld, Vector3 EndPositionWorld) RayExtents(int rayIndex) {
        var angle = Angles[rayIndex];
        Vector3 startPositionLocal, endPositionLocal;
        if (CastType == RayPerceptionCastType.Cast3D) {
            startPositionLocal = new Vector3(0, StartOffset, 0);
            endPositionLocal = PolarToCartesian3D(RayLength, angle);
            endPositionLocal.y += EndOffset;
        }
        else {
            // Vector2s here get converted to Vector3s (and back to Vector2s for casting)
            startPositionLocal = new Vector2() + (Vector2.up * StartOffset);
            endPositionLocal = PolarToCartesian2D(RayLength, angle) + (Vector2.up * StartOffset);
        }

        var startPositionWorld = Transform.TransformPoint(startPositionLocal);
        var endPositionWorld = Transform.TransformPoint(endPositionLocal);

        return (StartPositionWorld: startPositionWorld, EndPositionWorld: endPositionWorld);
    }

    static internal Vector3 PolarToCartesian3D(float radius, float angleDegrees) {
        var x = radius * Mathf.Cos(Mathf.Deg2Rad * angleDegrees);
        var z = radius * Mathf.Sin(Mathf.Deg2Rad * angleDegrees);
        return new Vector3(x, 0f, z);
    }

    static internal Vector2 PolarToCartesian2D(float radius, float angleDegrees) {
        var x = radius * Mathf.Cos(Mathf.Deg2Rad * angleDegrees);
        var y = radius * Mathf.Sin(Mathf.Deg2Rad * angleDegrees);
        return new Vector2(x, y);
    }
}
