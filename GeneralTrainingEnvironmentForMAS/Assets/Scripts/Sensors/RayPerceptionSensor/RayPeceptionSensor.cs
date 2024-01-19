using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using UnityEngine;

public class RayPerceptionSensor : ISensor, IBuiltInSensor {
    float[] m_Observations;
    string m_Name;

    RayPerceptionInput m_RayPerceptionInput;
    RayPerceptionOutput m_RayPerceptionOutput;

    /// <summary>
    /// Time.frameCount at the last time Update() was called. This is only used for display in gizmos.
    /// </summary>
    int m_DebugLastFrameCount;

    internal int DebugLastFrameCount {
        get { return m_DebugLastFrameCount; }
    }

    /// <summary>
    /// Creates the RayPerceptionSensor.
    /// </summary>
    /// <param name="name">The name of the sensor.</param>
    /// <param name="rayInput">The inputs for the sensor.</param>
    public RayPerceptionSensor(string name, RayPerceptionInput rayInput) {
        m_Name = name;
        m_RayPerceptionInput = rayInput;

        SetNumObservations(rayInput.OutputSize());

        m_DebugLastFrameCount = Time.frameCount;
        m_RayPerceptionOutput = new RayPerceptionOutput();
    }

    /// <summary>
    /// The most recent raycast results.
    /// </summary>
    public RayPerceptionOutput RayPerceptionOutput {
        get { return m_RayPerceptionOutput; }
    }

    void SetNumObservations(int numObservations) {
        m_Observations = new float[numObservations];
    }

    internal void SetRayPerceptionInput(RayPerceptionInput rayInput) {
        // Note that change the number of rays or tags doesn't directly call this,
        // but changing them and then changing another field will.
        if (m_RayPerceptionInput.OutputSize() != rayInput.OutputSize()) {
            Debug.Log(
                "Changing the number of tags or rays at runtime is not " +
                "supported and may cause errors in training or inference."
            );
            // Changing the shape will probably break things downstream, but we can at least
            // keep this consistent.
            SetNumObservations(rayInput.OutputSize());
        }
        m_RayPerceptionInput = rayInput;
    }

    public void Update() {
        m_DebugLastFrameCount = Time.frameCount;
        var numRays = m_RayPerceptionInput.Angles.Count;

        if (m_RayPerceptionOutput.RayOutputs == null || m_RayPerceptionOutput.RayOutputs.Length != numRays) {
            m_RayPerceptionOutput.RayOutputs = new RayPerceptionOutput.RayOutput[numRays];
        }

        // For each ray, do the casting and save the results.
        for (var rayIndex = 0; rayIndex < numRays; rayIndex++) {
            m_RayPerceptionOutput.RayOutputs[rayIndex] = PerceiveSingleRay(m_RayPerceptionInput, rayIndex);
        }
    }

    public void Reset() { }

 
    public string GetName() {
        return m_Name;
    }

    public virtual byte[] GetCompressedObservation() {
        return null;
    }


    public BuiltInSensorType GetBuiltInSensorType() {
        return BuiltInSensorType.RayPerceptionSensor;
    }

    /// <summary>
    /// Evaluates the raycasts to be used as part of an observation of an agent.
    /// </summary>
    /// <param name="input">Input defining the rays that will be cast.</param>
    /// <returns>Output struct containing the raycast results.</returns>
    public static RayPerceptionOutput Perceive(RayPerceptionInput input) {
        RayPerceptionOutput output = new RayPerceptionOutput();
        output.RayOutputs = new RayPerceptionOutput.RayOutput[input.Angles.Count];

        for (var rayIndex = 0; rayIndex < input.Angles.Count; rayIndex++) {
            output.RayOutputs[rayIndex] = PerceiveSingleRay(input, rayIndex);
        }

        return output;
    }

    /// <summary>
    /// Evaluate the raycast results of a single ray from the RayPerceptionInput.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="rayIndex"></param>
    /// <returns></returns>
    internal static RayPerceptionOutput.RayOutput PerceiveSingleRay(
        RayPerceptionInput input,
        int rayIndex
    ) {
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

        var rayOutput = new RayPerceptionOutput.RayOutput {
            HasHit = castHit,
            HitFraction = hitFraction,
            HitTaggedObject = false,
            HitTagIndex = -1,
            HitGameObject = hitObject,
            StartPositionWorld = startPositionWorld,
            EndPositionWorld = endPositionWorld,
            ScaledCastRadius = scaledCastRadius
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
                    rayOutput.HitTaggedObject = true;
                    rayOutput.HitTagIndex = i;
                    break;
                }
            }
        }


        return rayOutput;
    }
}
