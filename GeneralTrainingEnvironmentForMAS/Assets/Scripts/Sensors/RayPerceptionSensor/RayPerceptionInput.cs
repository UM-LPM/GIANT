using System;
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

    /// <summary>
    /// Get the cast start and end points for the given ray index/
    /// </summary>
    /// <param name="rayIndex"></param>
    /// <returns>A tuple of the start and end positions in world space.</returns>
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
            startPositionLocal = new Vector2();
            endPositionLocal = PolarToCartesian2D(RayLength, angle);
        }

        var startPositionWorld = Transform.TransformPoint(startPositionLocal);
        var endPositionWorld = Transform.TransformPoint(endPositionLocal);

        return (StartPositionWorld: startPositionWorld, EndPositionWorld: endPositionWorld);
    }

    /// <summary>
    /// Converts polar coordinate to cartesian coordinate.
    /// </summary>
    static internal Vector3 PolarToCartesian3D(float radius, float angleDegrees) {
        var x = radius * Mathf.Cos(Mathf.Deg2Rad * angleDegrees);
        var z = radius * Mathf.Sin(Mathf.Deg2Rad * angleDegrees);
        return new Vector3(x, 0f, z);
    }

    /// <summary>
    /// Converts polar coordinate to cartesian coordinate.
    /// </summary>
    static internal Vector2 PolarToCartesian2D(float radius, float angleDegrees) {
        var x = radius * Mathf.Cos(Mathf.Deg2Rad * angleDegrees);
        var y = radius * Mathf.Sin(Mathf.Deg2Rad * angleDegrees);
        return new Vector2(x, y);
    }
}
