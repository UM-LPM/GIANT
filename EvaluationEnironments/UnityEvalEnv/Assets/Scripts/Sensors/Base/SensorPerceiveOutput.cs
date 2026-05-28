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
    }

    public void Reset()
    {
        HasHit = false;
        HasHitTaggedObject = false;
        HitTagIndex = -1;
        HitFraction = 1f;
        HitGameObjects[0] = null;
    }
}