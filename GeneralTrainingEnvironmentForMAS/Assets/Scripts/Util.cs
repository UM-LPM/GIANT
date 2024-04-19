using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Util : MonoBehaviour {
    [SerializeField] bool SeedFromCommunicator = true;
    [SerializeField] int InitialSeed = 316227711;
    [SerializeField] public System.Random Rnd;
    [SerializeField] bool RandomSeed = false;

    private void Awake() {
        if (SeedFromCommunicator) {
            InitialSeed = Communicator.Instance.InitialSeed;
            Rnd = new System.Random(InitialSeed);
        }
        else {
            if (RandomSeed)
                Rnd = new System.Random();
            else
                Rnd = new System.Random(InitialSeed);
        }
    }

    public float NextFloat(float min, float max) {
        double val = (Rnd.NextDouble() * (max - min) + min);
        return (float)val;
    }
}