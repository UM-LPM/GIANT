using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using UnityEngine;
using AgentOrganizations;

[DisallowMultipleComponent]
public class Util : MonoBehaviour {
    //[SerializeField] bool SeedFromCommunicator = true;
    [SerializeField] int InitialSeed = 316227711;
    [SerializeField] public System.Random Rnd;
    [SerializeField] public System.Random RndBt;
    //[SerializeField] bool RandomSeed = false;

    private void Awake() {
        if (Communicator.Instance != null) {
            if (Communicator.Instance.RandomSeedMode == RandomSeedMode.Fixed) {
                Rnd = new System.Random(Communicator.Instance.InitialSeed);
                RndBt = new System.Random(Communicator.Instance.InitialSeed);
            }
            else if (Communicator.Instance.RandomSeedMode == RandomSeedMode.RandomAll) {
                Rnd = new System.Random(Communicator.Instance.InitialSeed);
                RndBt = new System.Random(Communicator.Instance.InitialSeed);
            }
            else { // RandomSeedMode.RandomPerIndividual
                Rnd = new System.Random();
                RndBt = new System.Random();
            }
        }
        else {
            Rnd = new System.Random(InitialSeed);
            RndBt = new System.Random(InitialSeed);
        }
    }

    public double NextDouble()
    {
        return Rnd.NextDouble();
    }

    public double NextDoubleBt()
    {
        return RndBt.NextDouble();
    }

    public float NextFloat(float min, float max) {
        double val = (Rnd.NextDouble() * (max - min) + min);
        return (float)val;
    }

    public float NextFloatdBt(float min, float max)
    {
        double val = (RndBt.NextDouble() * (max - min) + min);
        return (float)val;
    }

    public int NextInt(int min, int max)
    {
        return Rnd.Next(min, max);
    }

    public int NextIntBt(int min, int max)
    {
        return RndBt.Next(min, max);
    }

    public int NextInt(int max)
    {
        return Rnd.Next(max);
    }

    public int NextIntBt(int max)
    {
        return RndBt.Next(max);
    }
}