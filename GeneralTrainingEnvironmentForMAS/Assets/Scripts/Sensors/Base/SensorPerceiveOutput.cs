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
    public Vector2 EndPositionWorld;
}