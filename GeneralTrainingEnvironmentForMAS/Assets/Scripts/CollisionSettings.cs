
using UnityEngine;

public class CollisionSettings: MonoBehaviour
{
    [SerializeField] bool ReuseCollisionCallbacks = false;

    private void Start()
    {
        Physics.reuseCollisionCallbacks = ReuseCollisionCallbacks;
    }
}