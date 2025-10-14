using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Serialization;
using UnityEngine;
using UnityEngine.Windows;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting.Antlr3.Runtime.Collections;

public abstract class RaySensorBase : Sensor<SensorPerceiveOutput[]> {

    [Header("Ray sensor configuration")]

    [Range(0, 50)]
    [Tooltip("Number of rays to the left and right of center.")]
    [SerializeField] public int RaysPerDirection;

    [Range(0, 180)]
    [Tooltip("Cone size for rays. Using 90 degrees will cast rays to the left and right. " +
        "Greater than 90 degrees will go backwards.")]
    [SerializeField] public float MaxRayDegrees;


    [Range(0f, 10f)]
    [Tooltip("Radius of sphere to cast. Set to zero for raycasts.")]
    [SerializeField] public float SphereCastRadius;

    [Range(1, 1000)]
    [Tooltip("Length of the rays to cast.")]
    [SerializeField] float RayLength;

    RayPerceptionInput input;
    SensorPerceiveOutput[] rayOutputs;

    // Private variables
    float unscaledRayLength;
    float unscaledCastRadius;
    Vector3 startPositionWorld;
    Vector3 endPositionWorld;
    (Vector3 StartPositionWorld, Vector3 EndPositionWorld) extents;

    bool castHit;
    float hitFraction;
    GameObject hitObject;
    Vector3 rayDirection;
    float scaledRayLength;
    float scaledCastRadius;

    RaycastHit rayHit;
    RaycastHit2D rayHit2D;

    int numTags;
    bool tagsEqual;
    string detectableTag;

    RayPerceptionInput rayPerceptionInput;

    public RaySensorBase(string name) : base(name) {
    }

    public void SetLayerMask(int layerMask)
    {
        LayerMask = layerMask;
        input.LayerMask = LayerMask;
    }

    NativeArray<RaycastHit> results;
    NativeArray<SpherecastCommand> commands;
    JobHandle jobHandle;
    QueryParameters queryParameters;

    public override SensorPerceiveOutput[] PerceiveAll()
    {
        ResetRayOutputs();

        // Option 1: Perceive all rays in a single thread
        for (var rayIndex = 0; rayIndex < input.Angles.Count; rayIndex++) {
            rayOutputs[rayIndex] = PerceiveSingleRay(input, rayIndex);
        }

        return rayOutputs;
    }

    public override SensorPerceiveOutput[] PerceiveSingle(int xPos = -1, int yPos = -1, int zPos = -1)
    {
        ResetRayOutputs();

        if (xPos == -1)
            throw new ArgumentException("RaySensorBase.Perceive(): xPos must be set to a valid value");

        rayOutputs[xPos] = PerceiveSingleRay(input, xPos);

        return rayOutputs;
    }

    public override SensorPerceiveOutput[] PerceiveRange(int startIndex = -1, int endIndex = -1, int step = 1)
    {
        ResetRayOutputs();

        if (startIndex == -1 || endIndex == -1 || endIndex > input.Angles.Count) {
            throw new ArgumentException("RaySensorBase.Perceive(): startIndex and endIndex must be set to valid values");
        }

        // Option 1: Perceive all rays in a single thread
        for (var rayIndex = startIndex; rayIndex < endIndex; rayIndex+=step)
        {
            rayOutputs[rayIndex] = PerceiveSingleRay(input, rayIndex);
        }

        return rayOutputs;
    }

    public virtual SensorPerceiveOutput PerceiveSingleRay(RayPerceptionInput input, int rayIndex) {
        unscaledRayLength = input.RayLength;
        unscaledCastRadius = input.CastRadius;

        extents = input.RayExtents(rayIndex);
        startPositionWorld = extents.StartPositionWorld;
        endPositionWorld = extents.EndPositionWorld;

        rayDirection = endPositionWorld - startPositionWorld;
        // If there is non-unity scale, |rayDirection| will be different from rayLength.
        // We want to use this transformed ray length for determining cast length, hit fraction etc.
        // We also it to scale up or down the sphere or circle radii
        scaledRayLength = rayDirection.magnitude;
        // Avoid 0/0 if unscaledRayLength is 0
        scaledCastRadius = unscaledRayLength > 0 ?
            unscaledCastRadius * scaledRayLength / unscaledRayLength :
            unscaledCastRadius;

        // Do the cast and assign the hit information for each detectable tag.
        castHit = false;
        hitFraction = 1.0f;
        hitObject = null;


        if (input.CastType == RayPerceptionCastType.Cast3D) {
            if (scaledCastRadius > 0f) {
                castHit = PhysicsScene.SphereCast(startPositionWorld, scaledCastRadius, rayDirection, out rayHit,
                    scaledRayLength, input.LayerMask);
            }
            else {
                castHit = PhysicsScene.Raycast(startPositionWorld, rayDirection, out rayHit,
                    scaledRayLength, input.LayerMask);
            }

            // If scaledRayLength is 0, we still could have a hit with sphere casts (maybe?).
            // To avoid 0/0, set the fraction to 0.
            hitFraction = castHit ? (scaledRayLength > 0 ? rayHit.distance / scaledRayLength : 0.0f) : 1.0f;
            hitObject = castHit ? rayHit.collider.gameObject : null;
        }
        else {
            if (scaledCastRadius > 0f) {
                rayHit2D = PhysicsScene2D.CircleCast(startPositionWorld, scaledCastRadius, rayDirection,
                    scaledRayLength, input.LayerMask);
            }
            else {
                rayHit2D = PhysicsScene2D.Raycast(startPositionWorld, rayDirection, scaledRayLength, input.LayerMask);
            }

            castHit = rayHit2D;
            hitFraction = castHit ? rayHit2D.fraction : 1.0f;
            hitObject = castHit ? rayHit2D.collider.gameObject : null;
        }

        if(rayOutputs[rayIndex] == null)
            rayOutputs[rayIndex] = new SensorPerceiveOutput();

        rayOutputs[rayIndex].HasHit = castHit;
        rayOutputs[rayIndex].HitFraction = hitFraction;
        rayOutputs[rayIndex].HasHitTaggedObject = false;
        rayOutputs[rayIndex].HitTagIndex = -1;
        rayOutputs[rayIndex].HitGameObjects[0] = hitObject;
        rayOutputs[rayIndex].StartPositionWorld = startPositionWorld;
        rayOutputs[rayIndex].EndPositionWorld = endPositionWorld;
        
        // TODO Remove this
        /*rayOutput = new SensorPerceiveOutput {
            HasHit = castHit,
            HitFraction = hitFraction,
            HasHitTaggedObject = false,
            HitTagIndex = -1,
            HitGameObjects = new GameObject[] { hitObject },
            StartPositionWorld = startPositionWorld,
            EndPositionWorld = endPositionWorld,
        };*/

        if (castHit) {
            // Find the index of the tag of the object that was hit.
            numTags = input.DetectableTags?.Count ?? 0;
            for (var i = 0; i < numTags; i++) {
                tagsEqual = false;
                try {
                    detectableTag = input.DetectableTags[i];
                    if (!string.IsNullOrEmpty(detectableTag)) {
                        tagsEqual = hitObject.CompareTag(detectableTag);
                    }
                }
                catch (UnityException) {
                    // If the tag is null, empty, or not a valid tag, just ignore it.
                }

                if (tagsEqual) {
                    rayOutputs[rayIndex].HasHitTaggedObject = true;
                    rayOutputs[rayIndex].HitTagIndex = i;
                    break;
                }
            }
        }


        return rayOutputs[rayIndex];
    }

    public override void Init()
    {
        input = GetRayPerceptionInput();
        rayOutputs = new SensorPerceiveOutput[input.Angles.Count];
    }

    public void ResetRayOutputs()
    {
        for (int i = 0; i < rayOutputs.Length; i++)
        {
            rayOutputs[i] = null;
        }
    }



    public RayPerceptionInput GetRayPerceptionInput() {
        var rayAngles = GetRayAngles(RaysPerDirection, MaxRayDegrees);

        rayPerceptionInput = new RayPerceptionInput();
        rayPerceptionInput.RayLength = RayLength;
        rayPerceptionInput.DetectableTags = DetectableTags;
        rayPerceptionInput.Angles = rayAngles;
        rayPerceptionInput.StartOffset = GetStartVerticalOffset();
        rayPerceptionInput.EndOffset = GetEndVerticalOffset();
        rayPerceptionInput.CastRadius = SphereCastRadius;
        rayPerceptionInput.Transform = transform;
        rayPerceptionInput.CastType = GetCastType();
        rayPerceptionInput.LayerMask = LayerMask;

        return rayPerceptionInput;
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

    public abstract RayPerceptionCastType GetCastType();

    public virtual float GetStartVerticalOffset() {
        return 0f;
    }

    public virtual float GetEndVerticalOffset() {
        return 0f;
    }

    void OnDrawGizmosSelected() {
        if (DrawGizmos) {
            if (rayOutputs == null)
            {
                Init();
            }

            var rayInput = GetRayPerceptionInput();
            rayInput.DetectableTags = null;
            for (var rayIndex = 0; rayIndex < rayInput.Angles.Count; rayIndex++) {
                var rayOutput = PerceiveSingleRay(rayInput, rayIndex);
                DrawRaycastGizmos(rayOutput);
            }
        }
    }

    void DrawRaycastGizmos(SensorPerceiveOutput rayOutput, float alpha = 1.0f) {
        if ((DrawOnlyHitSensors && rayOutput.HasHit) || !DrawOnlyHitSensors) {
            var startPositionWorld = rayOutput.StartPositionWorld;
            var endPositionWorld = rayOutput.EndPositionWorld;
            var rayDirection = endPositionWorld - startPositionWorld;
            rayDirection *= rayOutput.HitFraction;

            // hit fraction ^2 will shift "far" hits closer to the hit color
            var lerpT = rayOutput.HitFraction * rayOutput.HitFraction;
            var color = Color.Lerp(HitSensorColor, BaseSensorColor, lerpT);
            color.a *= alpha;
            Gizmos.color = color;
            Gizmos.DrawRay(startPositionWorld, rayDirection);

            // Draw the hit point as a sphere. If using rays to cast (0 radius), use a small sphere.
            if (rayOutput.HasHit) {
                var hitRadius = Mathf.Max(SphereCastRadius, .05f);
                Gizmos.DrawWireSphere(startPositionWorld + rayDirection, hitRadius);
            }
        }
    }
}