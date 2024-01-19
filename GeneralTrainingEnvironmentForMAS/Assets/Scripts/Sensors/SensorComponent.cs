using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class SensorComponent : MonoBehaviour {
    /// <summary>
    /// Create the ISensors. This is called by the Agent when it is initialized.
    /// </summary>
    /// <returns>Created ISensor objects.</returns>
    public abstract ISensor[] CreateSensors();
}
