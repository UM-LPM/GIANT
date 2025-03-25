using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SensorPerceiveOutput {
    public bool HasHit;
    public bool HasHitTaggedObject;
    public int HitTagIndex;
    public float HitFraction;
    public GameObject[] HitGameObjects;
    public Vector3 StartPositionWorld;
    public Vector3 EndPositionWorld;

    public SensorPerceiveOutput()
    {
        HitGameObjects = new GameObject[1];
        StartPositionWorld = Vector3.zero;
        EndPositionWorld = Vector3.zero;
    }
}