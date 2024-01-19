using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Serialization;
using UnityEngine;

public abstract class RayPerceptionSensorComponentBase : SensorComponent {

    [SerializeField, FormerlySerializedAs("sensorName")]
    string m_SensorName = "RayPerceptionSensor";

    public string SensorName {
        get { return m_SensorName; }
        set { m_SensorName = value; }
    }

    [SerializeField, FormerlySerializedAs("detectableTags")]
    [Tooltip("List of tags in the scene to compare against.")]
    List<string> m_DetectableTags;

    public List<string> DetectableTags {
        get { return m_DetectableTags; }
        set { m_DetectableTags = value; }
    }

    [SerializeField, FormerlySerializedAs("raysPerDirection")]
    [Range(0, 50)]
    [Tooltip("Number of rays to the left and right of center.")]
    int m_RaysPerDirection = 3;

    public int RaysPerDirection {
        get { return m_RaysPerDirection; }
        // Note: can't change at runtime
        set { m_RaysPerDirection = value; }
    }

    [SerializeField, FormerlySerializedAs("maxRayDegrees")]
    [Range(0, 180)]
    [Tooltip("Cone size for rays. Using 90 degrees will cast rays to the left and right. " +
        "Greater than 90 degrees will go backwards.")]
    float m_MaxRayDegrees = 70;

    public float MaxRayDegrees {
        get => m_MaxRayDegrees;
        set { m_MaxRayDegrees = value; UpdateSensor(); }
    }

    [SerializeField, FormerlySerializedAs("sphereCastRadius")]
    [Range(0f, 10f)]
    [Tooltip("Radius of sphere to cast. Set to zero for raycasts.")]
    float m_SphereCastRadius = 0.5f;

    public float SphereCastRadius {
        get => m_SphereCastRadius;
        set { m_SphereCastRadius = value; UpdateSensor(); }
    }

    [SerializeField, FormerlySerializedAs("rayLength")]
    [Range(1, 1000)]
    [Tooltip("Length of the rays to cast.")]
    float m_RayLength = 20f;

    public float RayLength {
        get => m_RayLength;
        set { m_RayLength = value; UpdateSensor(); }
    }

    const int k_PhysicsDefaultLayers = -5;
    [SerializeField, FormerlySerializedAs("rayLayerMask")]
    [Tooltip("Controls which layers the rays can hit.")]
    LayerMask m_RayLayerMask;

    public LayerMask RayLayerMask {
        get => m_RayLayerMask;
        set { m_RayLayerMask = value; UpdateSensor(); }
    }

    [SerializeField, FormerlySerializedAs("observationStacks")]
    [Range(1, 50)]
    [Tooltip("Number of raycast results that will be stacked before being fed to the neural network.")]
    int m_ObservationStacks = 1;

    public int ObservationStacks {
        get { return m_ObservationStacks; }
        set { m_ObservationStacks = value; }
    }

    [SerializeField]
    [Header("Debug Gizmos", order = 999)]
    internal Color rayHitColor = Color.red;

    [SerializeField]
    internal Color rayMissColor = Color.white;

    [NonSerialized]
    RayPerceptionSensor m_RaySensor;

    public RayPerceptionSensor RaySensor {
        get => m_RaySensor;
    }

    public RayPerceptionInput GetRayPerceptionInput() {
        var rayAngles = GetRayAngles(RaysPerDirection, MaxRayDegrees);

        var rayPerceptionInput = new RayPerceptionInput();
        rayPerceptionInput.RayLength = RayLength;
        rayPerceptionInput.DetectableTags = DetectableTags;
        rayPerceptionInput.Angles = rayAngles;
        rayPerceptionInput.StartOffset = GetStartVerticalOffset();
        rayPerceptionInput.EndOffset = GetEndVerticalOffset();
        rayPerceptionInput.CastRadius = SphereCastRadius;
        rayPerceptionInput.Transform = transform;
        rayPerceptionInput.CastType = GetCastType();
        rayPerceptionInput.LayerMask = RayLayerMask;

        return rayPerceptionInput;
    }

    public override ISensor[] CreateSensors() {
        var rayPerceptionInput = GetRayPerceptionInput();

        m_RaySensor = new RayPerceptionSensor(SensorName, rayPerceptionInput);

        /*if (ObservationStacks != 1) {
            var stackingSensor = new StackingSensor(RaySensor, ObservationStacks);
            return new ISensor[] { stackingSensor };
        }*/

        return new ISensor[] { RaySensor };
    }

    internal static float[] GetRayAngles(int raysPerDirection, float maxRayDegrees) {
        // Example:
        // { 90, 90 - delta, 90 + delta, 90 - 2*delta, 90 + 2*delta }
        var anglesOut = new float[2 * raysPerDirection + 1];
        var delta = maxRayDegrees / raysPerDirection;
        anglesOut[0] = 90f;
        for (var i = 0; i < raysPerDirection; i++) {
            anglesOut[2 * i + 1] = 90 - (i + 1) * delta;
            anglesOut[2 * i + 2] = 90 + (i + 1) * delta;
        }
        return anglesOut;
    }

    internal void UpdateSensor() {
        if (m_RaySensor != null) {
            var rayInput = GetRayPerceptionInput();
            m_RaySensor.SetRayPerceptionInput(rayInput);
        }
    }

    public abstract RayPerceptionCastType GetCastType();

    public virtual float GetStartVerticalOffset() {
        return 0f;
    }

    public virtual float GetEndVerticalOffset() {
        return 0f;
    }

    internal int SensorObservationAge() {
        if (m_RaySensor != null) {
            return Time.frameCount - m_RaySensor.DebugLastFrameCount;
        }

        return 0;
    }

    void OnDrawGizmosSelected() {
        if (m_RaySensor?.RayPerceptionOutput?.RayOutputs != null) {
            // If we have cached debug info from the sensor, draw that.
            // Draw "old" observations in a lighter color.
            // Since the agent may not step every frame, this helps de-emphasize "stale" hit information.
            var alpha = Mathf.Pow(.5f, SensorObservationAge());

            foreach (var rayInfo in m_RaySensor.RayPerceptionOutput.RayOutputs) {
                DrawRaycastGizmos(rayInfo, alpha);
            }
        }
        else {
            var rayInput = GetRayPerceptionInput();
            // We don't actually need the tags here, since they don't affect the display of the rays.
            // Additionally, the user might be in the middle of typing the tag name when this is called,
            // and there's no way to turn off the "Tag ... is not defined" error logs.
            // So just don't use any tags here.
            rayInput.DetectableTags = null;
            for (var rayIndex = 0; rayIndex < rayInput.Angles.Count; rayIndex++) {
                var rayOutput = RayPerceptionSensor.PerceiveSingleRay(rayInput, rayIndex);
                DrawRaycastGizmos(rayOutput);
            }
        }
    }

    void DrawRaycastGizmos(RayPerceptionOutput.RayOutput rayOutput, float alpha = 1.0f) {
        var startPositionWorld = rayOutput.StartPositionWorld;
        var endPositionWorld = rayOutput.EndPositionWorld;
        var rayDirection = endPositionWorld - startPositionWorld;
        rayDirection *= rayOutput.HitFraction;

        // hit fraction ^2 will shift "far" hits closer to the hit color
        var lerpT = rayOutput.HitFraction * rayOutput.HitFraction;
        var color = Color.Lerp(rayHitColor, rayMissColor, lerpT);
        color.a *= alpha;
        Gizmos.color = color;
        Gizmos.DrawRay(startPositionWorld, rayDirection);

        // Draw the hit point as a sphere. If using rays to cast (0 radius), use a small sphere.
        if (rayOutput.HasHit) {
            var hitRadius = Mathf.Max(rayOutput.ScaledCastRadius, .05f);
            Gizmos.DrawWireSphere(startPositionWorld + rayDirection, hitRadius);
        }
    }
}