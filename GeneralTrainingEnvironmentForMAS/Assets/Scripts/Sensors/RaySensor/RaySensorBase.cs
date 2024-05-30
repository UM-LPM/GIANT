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

    public RaySensorBase(string name) : base(name) { }

    public override SensorPerceiveOutput[] Perceive() {
        RayPerceptionInput input = GetRayPerceptionInput();

        SensorPerceiveOutput[] rayOutputs = new SensorPerceiveOutput[input.Angles.Count];

        // Option 1: Perceive all rays in a single thread
        for (var rayIndex = 0; rayIndex < input.Angles.Count; rayIndex++) {
            rayOutputs[rayIndex] = PerceiveSingleRay(input, rayIndex);
        }

        // Option 2: Perceive all rays in a batch job
        /*var results = new NativeArray<RaycastHit>(input.Angles.Count, Allocator.TempJob);
        var commands = new NativeArray<SpherecastCommand>(input.Angles.Count, Allocator.TempJob);

        for (var rayIndex = 0; rayIndex < input.Angles.Count; rayIndex++)
        {
            var unscaledRayLength = input.RayLength;
            var unscaledCastRadius = input.CastRadius;

            var extents = input.RayExtents(rayIndex);
            var startPositionWorld = extents.StartPositionWorld;
            var endPositionWorld = extents.EndPositionWorld;

            var rayDirection = endPositionWorld - startPositionWorld;
            var scaledRayLength = rayDirection.magnitude;
            // Avoid 0/0 if unscaledRayLength is 0
            var scaledCastRadius = unscaledRayLength > 0 ?
                unscaledCastRadius * scaledRayLength / unscaledRayLength :
                unscaledCastRadius;

            QueryParameters queryParameters = new QueryParameters
            {
                layerMask = input.LayerMask,
                hitTriggers = QueryTriggerInteraction.Ignore,
            };

            commands[rayIndex] = new SpherecastCommand(startPositionWorld, scaledCastRadius, rayDirection, queryParameters, scaledRayLength);
        }

        // Execute batch 
        JobHandle jobHandle = SpherecastCommand.ScheduleBatch(commands, results, 1, 1, default(JobHandle));
        
        // Wait for the batch to complete
        jobHandle.Complete();

        // Process the results
        for (var rayIndex = 0; rayIndex < input.Angles.Count; rayIndex++)
        {
            var rayHit = results[rayIndex];
            var castHit = rayHit.collider != null;
            var hitFraction = castHit ? (rayHit.distance / input.RayLength) : 1.0f;
            var hitObject = castHit ? rayHit.collider.gameObject : null;

            SensorPerceiveOutput rayOutput = new SensorPerceiveOutput
            {
                HasHit = castHit,
                HitFraction = hitFraction,
                HasHitTaggedObject = false,
                HitTagIndex = -1,
                HitGameObjects = new GameObject[] { hitObject },
                StartPositionWorld = commands[rayIndex].origin,
                EndPositionWorld = commands[rayIndex].origin + commands[rayIndex].direction * hitFraction * input.RayLength,
                //ScaledCastRadius = scaledCastRadius
            };

            if (castHit)
            {
                // Find the index of the tag of the object that was hit.
                var numTags = input.DetectableTags?.Count ?? 0;
                for (var i = 0; i < numTags; i++)
                {
                    var tagsEqual = false;
                    try
                    {
                        var tag = input.DetectableTags[i];
                        if (!string.IsNullOrEmpty(tag))
                        {
                            tagsEqual = hitObject.CompareTag(tag);
                        }
                    }
                    catch (UnityException)
                    {
                        // If the tag is null, empty, or not a valid tag, just ignore it.
                    }

                    if (tagsEqual)
                    {
                        rayOutput.HasHitTaggedObject = true;
                        rayOutput.HitTagIndex = i;
                        break;
                    }
                }
            }

            rayOutputs[rayIndex] = rayOutput;
        }  
        
        results.Dispose();
        commands.Dispose();*/

        return rayOutputs;
    }

    public virtual SensorPerceiveOutput PerceiveSingleRay(RayPerceptionInput input, int rayIndex) {
        var unscaledRayLength = input.RayLength;
        var unscaledCastRadius = input.CastRadius;

        var extents = input.RayExtents(rayIndex);
        var startPositionWorld = extents.StartPositionWorld;
        var endPositionWorld = extents.EndPositionWorld;

        var rayDirection = endPositionWorld - startPositionWorld;
        // If there is non-unity scale, |rayDirection| will be different from rayLength.
        // We want to use this transformed ray length for determining cast length, hit fraction etc.
        // We also it to scale up or down the sphere or circle radii
        var scaledRayLength = rayDirection.magnitude;
        // Avoid 0/0 if unscaledRayLength is 0
        var scaledCastRadius = unscaledRayLength > 0 ?
            unscaledCastRadius * scaledRayLength / unscaledRayLength :
            unscaledCastRadius;

        // Do the cast and assign the hit information for each detectable tag.
        var castHit = false;
        var hitFraction = 1.0f;
        GameObject hitObject = null;

        if (input.CastType == RayPerceptionCastType.Cast3D) {
            RaycastHit rayHit;
            if (scaledCastRadius > 0f) {
                castHit = Physics.SphereCast(startPositionWorld, scaledCastRadius, rayDirection, out rayHit,
                    scaledRayLength, input.LayerMask);
            }
            else {
                castHit = Physics.Raycast(startPositionWorld, rayDirection, out rayHit,
                    scaledRayLength, input.LayerMask);
            }

            // If scaledRayLength is 0, we still could have a hit with sphere casts (maybe?).
            // To avoid 0/0, set the fraction to 0.
            hitFraction = castHit ? (scaledRayLength > 0 ? rayHit.distance / scaledRayLength : 0.0f) : 1.0f;
            hitObject = castHit ? rayHit.collider.gameObject : null;
        }
        else {
            RaycastHit2D rayHit;
            if (scaledCastRadius > 0f) {
                rayHit = Physics2D.CircleCast(startPositionWorld, scaledCastRadius, rayDirection,
                    scaledRayLength, input.LayerMask);
            }
            else {
                rayHit = Physics2D.Raycast(startPositionWorld, rayDirection, scaledRayLength, input.LayerMask);
            }

            castHit = rayHit;
            hitFraction = castHit ? rayHit.fraction : 1.0f;
            hitObject = castHit ? rayHit.collider.gameObject : null;
        }

        SensorPerceiveOutput rayOutput = new SensorPerceiveOutput {
            HasHit = castHit,
            HitFraction = hitFraction,
            HasHitTaggedObject = false,
            HitTagIndex = -1,
            HitGameObjects = new GameObject[] { hitObject },
            StartPositionWorld = startPositionWorld,
            EndPositionWorld = endPositionWorld,
            //ScaledCastRadius = scaledCastRadius
        };

        if (castHit) {
            // Find the index of the tag of the object that was hit.
            var numTags = input.DetectableTags?.Count ?? 0;
            for (var i = 0; i < numTags; i++) {
                var tagsEqual = false;
                try {
                    var tag = input.DetectableTags[i];
                    if (!string.IsNullOrEmpty(tag)) {
                        tagsEqual = hitObject.CompareTag(tag);
                    }
                }
                catch (UnityException) {
                    // If the tag is null, empty, or not a valid tag, just ignore it.
                }

                if (tagsEqual) {
                    rayOutput.HasHitTaggedObject = true;
                    rayOutput.HitTagIndex = i;
                    break;
                }
            }
        }


        return rayOutput;
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