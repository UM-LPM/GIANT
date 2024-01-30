using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Sensor<T> : MonoBehaviour {

    [field: SerializeField]
    public string Name { get; set; }

    public Sensor(string name) {
        Name = name;
    }

    public abstract T Perceive();
}
