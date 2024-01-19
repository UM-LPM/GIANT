using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Serialization;
using UnityEngine;

[AddComponentMenu("Sensors/Ray Perception Sensor 3D", (int)MenuGroup.Sensors)]
public class RayPerceptionSensorComponent3D : RayPerceptionSensorComponentBase {

    [SerializeField, FormerlySerializedAs("startVerticalOffset")]
    [Range(-10f, 10f)]
    [Tooltip("Ray start is offset up or down by this amount.")]
    float m_StartVerticalOffset;

    public float StartVerticalOffset {
        get => m_StartVerticalOffset;
        set { m_StartVerticalOffset = value; UpdateSensor(); }
    }

    [SerializeField, FormerlySerializedAs("endVerticalOffset")]
    [Range(-10f, 10f)]
    [Tooltip("Ray end is offset up or down by this amount.")]
    float m_EndVerticalOffset;

    public float EndVerticalOffset {
        get => m_EndVerticalOffset;
        set { m_EndVerticalOffset = value; UpdateSensor(); }
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
