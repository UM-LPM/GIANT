using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RayPerceptionOutput {
    /// <summary>
    /// Contains the data generated from a single ray of a ray perception sensor.
    /// </summary>
    public struct RayOutput {
        public bool HasHit;
        public bool HitTaggedObject;
        public int HitTagIndex;
        public float HitFraction;
        public GameObject HitGameObject;
        public Vector3 StartPositionWorld;
        public Vector3 EndPositionWorld;
        public float ScaledRayLength {
            get {
                var rayDirection = EndPositionWorld - StartPositionWorld;
                return rayDirection.magnitude;
            }
        }
        public float ScaledCastRadius;

        /// <summary>
        /// Writes the ray output information to a subset of the float array.  Each element in the rayAngles array
        /// determines a sublist of data to the observation. The sublist contains the observation data for a single cast.
        /// The list is composed of the following:
        /// 1. A one-hot encoding for detectable tags. For example, if DetectableTags.Length = n, the
        ///    first n elements of the sublist will be a one-hot encoding of the detectableTag that was hit, or
        ///    all zeroes otherwise.
        /// 2. The 'numDetectableTags' element of the sublist will be 1 if the ray missed everything, or 0 if it hit
        ///    something (detectable or not).
        /// 3. The 'numDetectableTags+1' element of the sublist will contain the normalized distance to the object
        ///    hit, or 1.0 if nothing was hit.
        /// </summary>
        /// <param name="numDetectableTags"></param>
        /// <param name="rayIndex"></param>
        /// <param name="buffer">Output buffer. The size must be equal to (numDetectableTags+2) * RayOutputs.Length</param>
        public void ToFloatArray(int numDetectableTags, int rayIndex, float[] buffer) {
            var bufferOffset = (numDetectableTags + 2) * rayIndex;
            if (HitTaggedObject) {
                buffer[bufferOffset + HitTagIndex] = 1f;
            }
            buffer[bufferOffset + numDetectableTags] = HasHit ? 0f : 1f;
            buffer[bufferOffset + numDetectableTags + 1] = HitFraction;
        }
    }

    /// <summary>
    /// RayOutput for each ray that was cast.
    /// </summary>
    public RayOutput[] RayOutputs;
}
