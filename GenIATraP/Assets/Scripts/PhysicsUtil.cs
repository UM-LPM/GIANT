using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PhysicsUtil
{
    public static bool PhysicsOverlapObject(GameType gameType, GameObject caller, Vector3 position, float radius, Vector3 halfExtends, Quaternion rotation, PhysicsOverlapType overlapType, bool ignoreTriggerGameObjs, int layer, int defaultLayer)
    {
        switch (overlapType)
        {
            case PhysicsOverlapType.OverlapBox:
                return PhysicsOverlapBox(gameType, caller, position, rotation, halfExtends, ignoreTriggerGameObjs, layer, defaultLayer);
            case PhysicsOverlapType.OverlapSphere:
                return PhysicsOverlapSphere(gameType, caller, position, radius, ignoreTriggerGameObjs, layer, defaultLayer);
            case PhysicsOverlapType.OverlapCapsule:
                return PhysicsOverlapCapsule(gameType, caller, position, radius, rotation, ignoreTriggerGameObjs, layer, defaultLayer);
            default:
                return false;
        }
    }

    public static bool PhysicsOverlapBox(GameType gameType, GameObject caller, Vector3 position, Quaternion rotation, Vector3 halfExtends, bool ignoreTriggerGameObjs, int layer, int defaultLayer)
    {
        Component[] colliders = null;
        if (gameType == GameType._3D)
        {
            colliders = Physics.OverlapBox(position, halfExtends, rotation, LayerMask.GetMask(LayerMask.LayerToName(layer)) + defaultLayer);
            if (ignoreTriggerGameObjs)
                colliders = colliders.Where(col => !(col as Collider).isTrigger).ToArray();

            if (colliders.Length > 1 || (colliders.Length == 1 && caller != colliders[0].gameObject))
            {
                return true;
            }
        }
        else
        {
            colliders = Physics2D.OverlapBoxAll(position, halfExtends, rotation.eulerAngles.z, LayerMask.GetMask(LayerMask.LayerToName(layer)) + defaultLayer);
            if (ignoreTriggerGameObjs)
                colliders = colliders.Where(col => !(col as Collider2D).isTrigger).ToArray();
        }

        if (colliders.Length > 1 || (colliders.Length == 1 && caller != colliders[0].gameObject))
        {
            return true;
        }

        return false;
    }

    public static bool PhysicsOverlapSphere(GameType gameType, GameObject caller, Vector3 position, float radius, bool ignoreTriggerGameObjs, int layer, int defaultLayer)
    {
        Component[] colliders = null;
        if (gameType == GameType._3D)
        {
            colliders = Physics.OverlapSphere(position, radius, LayerMask.GetMask(LayerMask.LayerToName(layer)) + defaultLayer);
            if (ignoreTriggerGameObjs)
                colliders = colliders.Where(col => !(col as Collider).isTrigger).ToArray();
        }
        else
        {
            colliders = Physics2D.OverlapCircleAll(position, radius, LayerMask.GetMask(LayerMask.LayerToName(layer)) + defaultLayer);

            if (ignoreTriggerGameObjs)
                colliders = colliders.Where(col => !(col as Collider2D).isTrigger).ToArray();

        }

        if (colliders.Length > 1 || (colliders.Length == 1 && caller != colliders[0].gameObject))
        {
            return true;
        }

        return false;
    }

    public static bool PhysicsOverlapCapsule(GameType gameType, GameObject caller, Vector3 position, float radius, Quaternion rotation, bool ignoreTriggerGameObjs, int layer, int defaultLayer)
    {
        throw new System.NotImplementedException();
    }

    public static T PhysicsOverlapTargetObject<T>(GameType gameType, GameObject caller, Vector3 position, float radius, Vector3 halfExtends, Quaternion rotation, PhysicsOverlapType overlapType, bool ignoreTriggerGameObjs, int layer, int defaultLayer) where T: Component
    {
        switch (overlapType)
        {
            case PhysicsOverlapType.OverlapBox:
                return PhysicsOverlapBoxTargetObject<T>(gameType, caller, position, rotation, halfExtends, ignoreTriggerGameObjs, layer, defaultLayer);
            case PhysicsOverlapType.OverlapSphere:
                return PhysicsOverlapSphereTargetObject<T>(gameType, caller, position, radius, ignoreTriggerGameObjs, layer, defaultLayer);
            case PhysicsOverlapType.OverlapCapsule:
                return PhysicsOverlapCapsuleTargetObject<T>(gameType, caller, position, radius, rotation, ignoreTriggerGameObjs, layer, defaultLayer);
            default:
                return null;
        }
    }

    public static T PhysicsOverlapBoxTargetObject<T>(GameType gameType, GameObject caller, Vector3 position, Quaternion rotation, Vector3 halfExtends, bool ignoreTriggerGameObjs, int layer, int defaultLayer) where T : Component
    {
        Component[] colliders = null;
        if (gameType == GameType._3D)
        {
            colliders = Physics.OverlapBox(position, halfExtends, rotation, LayerMask.GetMask(LayerMask.LayerToName(layer)) + defaultLayer);
            if (ignoreTriggerGameObjs)
                colliders = colliders.Where(col => !(col as Collider).isTrigger).ToArray();
        }
        else
        {
            colliders = Physics2D.OverlapBoxAll(position, halfExtends, rotation.eulerAngles.z, LayerMask.GetMask(LayerMask.LayerToName(layer)) + defaultLayer);
            if (ignoreTriggerGameObjs)
                colliders = colliders.Where(col => !(col as Collider2D).isTrigger).ToArray();

        }

        foreach (var collider in colliders)
        {
            if (caller != collider.gameObject)
            {
                T component = collider.GetComponent<T>();
                if (component != null)
                    return component;
            }
        }

        return null;
    }

    public static T PhysicsOverlapSphereTargetObject<T>(GameType gameType, GameObject caller, Vector3 position, float radius, bool ignoreTriggerGameObjs, int layer, int defaultLayer) where T : Component
    {
        Component[] colliders = null;
        if (gameType == GameType._3D)
        {
            colliders = Physics.OverlapSphere(position, radius, LayerMask.GetMask(LayerMask.LayerToName(layer)) + defaultLayer);
            if (ignoreTriggerGameObjs)
                colliders = colliders.Where(col => !(col as Collider).isTrigger).ToArray();
        }
        else
        {
            colliders = Physics2D.OverlapCircleAll(position, radius, LayerMask.GetMask(LayerMask.LayerToName(layer)) + defaultLayer);

            if (ignoreTriggerGameObjs)
                colliders = colliders.Where(col => !(col as Collider2D).isTrigger).ToArray();
        }

        foreach (var collider in colliders)
        {
            if (caller != collider.gameObject)
            {
                T component = collider.GetComponent<T>();
                if (component != null)
                    return component;
            }
        }

        return null;
    }

    public static T PhysicsOverlapCapsuleTargetObject<T>(GameType gameType, GameObject caller, Vector3 position, float radius, Quaternion rotation, bool ignoreTriggerGameObjs, int layer, int defaultLayer) where T : Component
    {
        throw new System.NotImplementedException();
    }


    public static List<T> PhysicsOverlapTargetObjects<T>(GameType gameType, GameObject caller, Vector3 position, float radius, Vector3 halfExtends, Quaternion rotation, PhysicsOverlapType overlapType, bool ignoreTriggerGameObjs, int layer, int defaultLayer) where T : Component
    {
        switch (overlapType)
        {
            case PhysicsOverlapType.OverlapBox:
                return PhysicsOverlapBoxTargetObjects<T>(gameType, caller, position, rotation, halfExtends, ignoreTriggerGameObjs, layer, defaultLayer);
            case PhysicsOverlapType.OverlapSphere:
                return PhysicsOverlapSphereTargetObjects<T>(gameType, caller, position, radius, ignoreTriggerGameObjs, layer, defaultLayer);
            case PhysicsOverlapType.OverlapCapsule:
                return PhysicsOverlapCapsuleTargetObjects<T>(gameType, caller, position, radius, rotation, ignoreTriggerGameObjs, layer, defaultLayer);
            default:
                return null;
        }
    }

    public static List<T> PhysicsOverlapBoxTargetObjects<T>(GameType gameType, GameObject caller, Vector3 position, Quaternion rotation, Vector3 halfExtends, bool ignoreTriggerGameObjs, int layer, int defaultLayer) where T : Component
    {
        Component[] colliders = null;
        if (gameType == GameType._3D)
        {
            colliders = Physics.OverlapBox(position, halfExtends, rotation, LayerMask.GetMask(LayerMask.LayerToName(layer)) + defaultLayer);
            if (ignoreTriggerGameObjs)
                colliders = colliders.Where(col => !(col as Collider).isTrigger).ToArray();
        }
        else
        {
            colliders = Physics2D.OverlapBoxAll(position, halfExtends, rotation.eulerAngles.z, LayerMask.GetMask(LayerMask.LayerToName(layer)) + defaultLayer);
            if (ignoreTriggerGameObjs)
                colliders = colliders.Where(col => !(col as Collider2D).isTrigger).ToArray();

        }

        return colliders.Select(col => col.GetComponent<T>()).Where(component => component != null).ToList();
    }

    public static List<T> PhysicsOverlapSphereTargetObjects<T>(GameType gameType, GameObject caller, Vector3 position, float radius, bool ignoreTriggerGameObjs, int layer, int defaultLayer) where T : Component
    {
        Component[] colliders = null;
        if (gameType == GameType._3D)
        {
            colliders = Physics.OverlapSphere(position, radius, LayerMask.GetMask(LayerMask.LayerToName(layer)) + defaultLayer);
            if (ignoreTriggerGameObjs)
                colliders = colliders.Where(col => !(col as Collider).isTrigger).ToArray();
        }
        else
        {
            colliders = Physics2D.OverlapCircleAll(position, radius, LayerMask.GetMask(LayerMask.LayerToName(layer)) + defaultLayer);

            if (ignoreTriggerGameObjs)
                colliders = colliders.Where(col => !(col as Collider2D).isTrigger).ToArray();
        }

        return colliders.Select(col => col.GetComponent<T>()).Where(component => component != null).ToList();
    }

    public static List<T> PhysicsOverlapCapsuleTargetObjects<T>(GameType gameType, GameObject caller, Vector3 position, float radius, Quaternion rotation, bool ignoreTriggerGameObjs, int layer, int defaultLayer) where T : Component
    {
        throw new System.NotImplementedException();
    }
}

public enum PhysicsOverlapType
{
    OverlapBox,
    OverlapSphere,
    OverlapCapsule
}