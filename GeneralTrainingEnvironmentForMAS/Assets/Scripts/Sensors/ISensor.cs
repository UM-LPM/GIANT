using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ISensor {

    /// <summary>
    /// Get the name of the sensor. This is used to ensure deterministic sorting of the sensors
    /// on an Agent, so the naming must be consistent across all sensors and agents.
    /// </summary>
    string GetName();
}
