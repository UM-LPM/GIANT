using System.Linq;
using UnityEngine;

public static class PhysicsUtil
{
    public static bool PhysicsOverlap(GameType gameType, GameObject caller, Vector3 position, float radius, Vector3 halfExtends, Quaternion newRotation, PhysicsOverlapType overlapType, bool ignoreTriggerGameObjs, int layer, int defaultLayer)
    {
        switch (overlapType)
        {
            case PhysicsOverlapType.OverlapBox:
                return PhysicsOverlapBox(gameType, caller, position, newRotation, halfExtends, ignoreTriggerGameObjs, layer, defaultLayer);
            case PhysicsOverlapType.OverlapSphere:
                return PhysicsOverlapSphere(gameType, caller, position, radius, ignoreTriggerGameObjs, layer, defaultLayer);
            case PhysicsOverlapType.OverlapCapsule:
                return PhysicsOverlapCapsule(gameType, caller, position, radius, newRotation, ignoreTriggerGameObjs, layer, defaultLayer);
            default:
                return false;
        }
    }

    public static bool PhysicsOverlapBox(GameType gameType, GameObject caller, Vector3 position, Quaternion newRotation, Vector3 halfExtends, bool ignoreTriggerGameObjs, int layer, int defaultLayer)
    {
        if (gameType == GameType._3D)
        {
            Collider[] colliders = Physics.OverlapBox(position, halfExtends, newRotation, LayerMask.GetMask(LayerMask.LayerToName(layer)) + defaultLayer);
            if (ignoreTriggerGameObjs)
                colliders = colliders.Where(col => !col.isTrigger).ToArray();

            if (colliders.Length > 1 || (colliders.Length == 1 && caller != colliders[0].gameObject))
            {
                return true;
            }
        }
        else
        {
            Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(position, halfExtends, newRotation.eulerAngles.z, LayerMask.GetMask(LayerMask.LayerToName(layer)) + defaultLayer);
            if (ignoreTriggerGameObjs)
                collider2Ds = collider2Ds.Where(col => !col.isTrigger).ToArray();

            if (collider2Ds.Length > 1 || (collider2Ds.Length == 1 && caller != collider2Ds[0].gameObject))
            {
                return true;
            }
        }

        return false;
    }

    public static bool PhysicsOverlapSphere(GameType gameType, GameObject caller, Vector3 position, float radius, bool ignoreTriggerGameObjs, int layer, int defaultLayer)
    {
        if (gameType == GameType._3D)
        {
            Collider[] colliders = Physics.OverlapSphere(position, radius, LayerMask.GetMask(LayerMask.LayerToName(layer)) + defaultLayer);
            if (ignoreTriggerGameObjs)
                colliders = colliders.Where(col => !col.isTrigger).ToArray();

            if (colliders.Length > 1 || (colliders.Length == 1 && caller != colliders[0].gameObject))
            {
                return true;
            }
        }
        else
        {
            Collider2D[] collider2Ds = Physics2D.OverlapCircleAll(position, radius, LayerMask.GetMask(LayerMask.LayerToName(layer)) + defaultLayer);

            if (ignoreTriggerGameObjs)
                collider2Ds = collider2Ds.Where(col => !col.isTrigger).ToArray();

            if (collider2Ds.Length > 1 || (collider2Ds.Length == 1 && caller != collider2Ds[0].gameObject))
            {
                return true;
            }
        }

        return false;
    }

    public static bool PhysicsOverlapCapsule(GameType gameType, GameObject caller, Vector3 position, float radius, Quaternion newRotation, bool ignoreTriggerGameObjs, int layer, int defaultLayer)
    {
        // TODO Implement

        return false;
    }
}

public enum PhysicsOverlapType
{
    OverlapBox,
    OverlapSphere,
    OverlapCapsule
}