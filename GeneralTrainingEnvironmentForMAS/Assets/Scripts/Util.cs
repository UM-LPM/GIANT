using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Util : MonoBehaviour {
    public int initialSeed = 316227711;
    public System.Random rnd;
    public bool randomSeed = false;

    private void Awake() {
        if (randomSeed)
            rnd = new System.Random();
        else
            rnd = new System.Random(initialSeed);
    }

    public float NextFloat(float min, float max) {
        double val = (rnd.NextDouble() * (max - min) + min);
        return (float)val;
    }
}