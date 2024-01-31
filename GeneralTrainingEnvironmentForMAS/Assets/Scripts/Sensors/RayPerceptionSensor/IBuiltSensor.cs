using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal interface IBuiltInSensor {
    /// <summary>
    /// Return the corresponding BuiltInSensorType for the sensor.
    /// </summary>
    /// <returns>A BuiltInSensorType corresponding to the sensor.</returns>
    BuiltInSensorType GetBuiltInSensorType();
}