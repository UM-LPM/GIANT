using Base;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Utils
{
    public static class PhysicsUtil
    {
        public static int DefaultColliderArraySize = 32;

        public static bool PhysicsOverlapObject(
            PhysicsScene physicsScene,
            PhysicsScene2D physicsScene2D,
            GameType gameType,
            GameObject caller,
            Vector3 position,
            float radius,
            Vector3 halfExtends,
            Quaternion rotation,
            PhysicsOverlapType overlapType,
            bool ignoreTriggerGameObjs,
            int layer)
        {
            switch (overlapType)
            {
                case PhysicsOverlapType.OverlapBox:
                    return PhysicsOverlapBox(physicsScene, physicsScene2D, gameType, caller, position, rotation, halfExtends, ignoreTriggerGameObjs, layer);
                case PhysicsOverlapType.OverlapSphere:
                    return PhysicsOverlapSphere(physicsScene, physicsScene2D, gameType, caller, position, radius, ignoreTriggerGameObjs, layer);
                case PhysicsOverlapType.OverlapCapsule:
                    return PhysicsOverlapCapsule(physicsScene, physicsScene2D, gameType, caller, position, radius, rotation, ignoreTriggerGameObjs, layer);
                default:
                    return false;
            }
        }

        public static bool PhysicsOverlapBox(
            PhysicsScene physicsScene,
            PhysicsScene2D physicsScene2D,
            GameType gameType,
            GameObject caller,
            Vector3 position,
            Quaternion rotation,
            Vector3 halfExtends,
            bool ignoreTriggerGameObjs,
            int layer)
        {
            if (gameType == GameType._3D)
            {
                Collider[] colliders = new Collider[DefaultColliderArraySize];
                physicsScene.OverlapBox(position, halfExtends, colliders, rotation, LayerMask.GetMask(LayerMask.LayerToName(layer)), ignoreTriggerGameObjs ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
                return HasCollision(caller, colliders);
            }
            else
            {
                Collider2D[] colliders = new Collider2D[DefaultColliderArraySize];
                ContactFilter2D contactFilter = new ContactFilter2D() {
                    layerMask = 1 << layer,
                    useTriggers = !ignoreTriggerGameObjs,
                };
                physicsScene2D.OverlapBox(position, halfExtends, rotation.eulerAngles.z, contactFilter, colliders);
                return HasCollision(caller, colliders);
            }
        }

        public static bool PhysicsOverlapSphere(
            PhysicsScene physicsScene,
            PhysicsScene2D physicsScene2D,
            GameType gameType,
            GameObject caller,
            Vector3 position,
            float radius,
            bool ignoreTriggerGameObjs,
            int layer)
        {
            if (gameType == GameType._3D)
            {
                Collider[] colliders = new Collider[DefaultColliderArraySize];
                physicsScene.OverlapSphere(position, radius, colliders, LayerMask.GetMask(LayerMask.LayerToName(layer)), ignoreTriggerGameObjs ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
                return HasCollision(caller, colliders);
            }
            else
            {
                Collider2D[] colliders = new Collider2D[DefaultColliderArraySize];
                ContactFilter2D contactFilter = new ContactFilter2D()
                {
                    layerMask = 1 << layer,
                    useTriggers = !ignoreTriggerGameObjs,
                };
                physicsScene2D.OverlapCircle(position, radius, contactFilter, colliders);
                return HasCollision(caller, colliders);
            }
        }

        public static T[] PhysicsOverlapSphere<T>(
            PhysicsScene physicsScene,
            PhysicsScene2D physicsScene2D,
            GameType gameType,
            GameObject caller,
            Vector3 position,
            float radius,
            bool ignoreTriggerGameObjs,
            int layer)
        {
            List<T> ts = new List<T>();
            Component[] components = new Component[]{};
            if (gameType == GameType._3D)
            {
                Collider[] colliders = new Collider[DefaultColliderArraySize];
                physicsScene.OverlapSphere(position, radius, colliders, LayerMask.GetMask(LayerMask.LayerToName(layer)), ignoreTriggerGameObjs ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
                components = colliders;
            }
            else
            {
                Collider2D[] colliders = new Collider2D[DefaultColliderArraySize];
                ContactFilter2D contactFilter = new ContactFilter2D()
                {
                    layerMask = 1 << layer,
                    useTriggers = !ignoreTriggerGameObjs,
                };
                physicsScene2D.OverlapCircle(position, radius, contactFilter, colliders);
                components = colliders;
            }

            if (components.Length > 1 || (components.Length == 1 && caller != components[0].gameObject))
            {
                foreach (var collider in components)
                {
                    if (collider != null && caller != collider.gameObject)
                    {
                        T component = collider.GetComponent<T>();
                        if (component != null)
                            ts.Add(component);
                    }
                }
            }

            return ts.ToArray();
        }

        public static bool PhysicsOverlapCapsule(
            PhysicsScene physicsScene,
            PhysicsScene2D physicsScene2D,
            GameType gameType,
            GameObject caller,
            Vector3 position,
            float radius,
            Quaternion rotation,
            bool ignoreTriggerGameObjs,
            int layer)
        {
            throw new System.NotImplementedException();
        }

        public static T PhysicsOverlapTargetObject<T>(
            PhysicsScene physicsScene,
            PhysicsScene2D physicsScene2D,
            GameType gameType,
            GameObject caller,
            Vector3 position,
            float radius,
            Vector3 halfExtends,
            Quaternion rotation,
            PhysicsOverlapType overlapType,
            bool ignoreTriggerGameObjs,
            int layer) where T : Component
        {
            switch (overlapType)
            {
                case PhysicsOverlapType.OverlapBox:
                    return PhysicsOverlapBoxTargetObject<T>(physicsScene, physicsScene2D, gameType, caller, position, rotation, halfExtends, ignoreTriggerGameObjs, layer);
                case PhysicsOverlapType.OverlapSphere:
                    return PhysicsOverlapSphereTargetObject<T>(physicsScene, physicsScene2D, gameType, caller, position, radius, ignoreTriggerGameObjs, layer);
                case PhysicsOverlapType.OverlapCapsule:
                    return PhysicsOverlapCapsuleTargetObject<T>(physicsScene, physicsScene2D, gameType, caller, position, radius, rotation, ignoreTriggerGameObjs, layer);
                default:
                    return null;
            }
        }

        public static T PhysicsOverlapBoxTargetObject<T>(
            PhysicsScene physicsScene,
            PhysicsScene2D physicsScene2D,GameType gameType, GameObject caller, Vector3 position, Quaternion rotation, Vector3 halfExtends, bool ignoreTriggerGameObjs, int layer) where T : Component
        {
            Component[] components = new Component[]{};
            if (gameType == GameType._3D)
            {
                Collider[] colliders = new Collider[DefaultColliderArraySize];
                physicsScene.OverlapBox(position, halfExtends, colliders, rotation, LayerMask.GetMask(LayerMask.LayerToName(layer)), ignoreTriggerGameObjs ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
                components = colliders;
            }
            else
            {
                Collider2D[] colliders = new Collider2D[DefaultColliderArraySize];
                ContactFilter2D contactFilter = new ContactFilter2D()
                {
                    layerMask = 1 << layer,
                    useTriggers = !ignoreTriggerGameObjs,
                };

                physicsScene2D.OverlapBox(position, halfExtends, rotation.eulerAngles.z, contactFilter, colliders);
                components = colliders;
            }

            foreach (var collider in components)
            {
                if (collider != null && caller != collider.gameObject)
                {
                    T component = collider.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }

            return null;
        }

        public static T PhysicsOverlapSphereTargetObject<T>(
            PhysicsScene physicsScene,
            PhysicsScene2D physicsScene2D,
            GameType gameType,
            GameObject caller,
            Vector3 position,
            float radius,
            bool ignoreTriggerGameObjs,
            int layer) where T : Component
        {
            Component[] components = new Component[]{};
            if (gameType == GameType._3D)
            {
                Collider[] colliders = new Collider[DefaultColliderArraySize];
                physicsScene.OverlapSphere(position, radius, colliders, LayerMask.GetMask(LayerMask.LayerToName(layer)), ignoreTriggerGameObjs ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
                components = colliders;
            }
            else
            {
                Collider2D[] colliders = new Collider2D[DefaultColliderArraySize];
                ContactFilter2D contactFilter = new ContactFilter2D()
                {
                    layerMask = 1 << layer,
                    useTriggers = !ignoreTriggerGameObjs,
                };

                physicsScene2D.OverlapCircle(position, radius, contactFilter, colliders);
                components = colliders;
            }

            foreach (var collider in components)
            {
                if (collider != null && caller != collider.gameObject)
                {
                    T component = collider.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }

            return null;
        }

        public static T PhysicsOverlapCapsuleTargetObject<T>(
            PhysicsScene physicsScene,
            PhysicsScene2D physicsScene2D,
            GameType gameType,
            GameObject caller,
            Vector3 position,
            float radius,
            Quaternion rotation,
            bool ignoreTriggerGameObjs,
            int layer) where T : Component
        {
            throw new System.NotImplementedException();
        }


        public static List<T> PhysicsOverlapTargetObjects<T>(
            PhysicsScene physicsScene,
            PhysicsScene2D physicsScene2D,
            GameType gameType,
            GameObject caller,
            Vector3 position,
            float radius,
            Vector3 halfExtends,
            Quaternion rotation,
            PhysicsOverlapType overlapType,
            bool ignoreTriggerGameObjs, int layer) where T : Component
        {
            switch (overlapType)
            {
                case PhysicsOverlapType.OverlapBox:
                    return PhysicsOverlapBoxTargetObjects<T>(physicsScene, physicsScene2D, gameType, caller, position, rotation, halfExtends, ignoreTriggerGameObjs, layer);
                case PhysicsOverlapType.OverlapSphere:
                    return PhysicsOverlapSphereTargetObjects<T>(physicsScene, physicsScene2D, gameType, caller, position, radius, ignoreTriggerGameObjs, layer);
                case PhysicsOverlapType.OverlapCapsule:
                    return PhysicsOverlapCapsuleTargetObjects<T>(physicsScene, physicsScene2D, gameType, caller, position, radius, rotation, ignoreTriggerGameObjs, layer);
                default:
                    return null;
            }
        }

        public static List<T> PhysicsOverlapBoxTargetObjects<T>(
            PhysicsScene physicsScene,
            PhysicsScene2D physicsScene2D,
            GameType gameType,
            GameObject caller,
            Vector3 position,
            Quaternion rotation,
            Vector3 halfExtends,
            bool ignoreTriggerGameObjs,
            int layer) where T : Component
        {
            Component[] components = new Component[]{};
            if (gameType == GameType._3D)
            {
                Collider[] colliders = new Collider[DefaultColliderArraySize];
                physicsScene.OverlapBox(position, halfExtends, colliders, rotation, LayerMask.GetMask(LayerMask.LayerToName(layer)), ignoreTriggerGameObjs ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
                components = colliders;
            }
            else
            {
                Collider2D[] colliders = new Collider2D[DefaultColliderArraySize];
                ContactFilter2D contactFilter = new ContactFilter2D()
                {
                    layerMask = 1 << layer,
                    useTriggers = !ignoreTriggerGameObjs,
                };

                physicsScene2D.OverlapBox(position, halfExtends, rotation.eulerAngles.z, contactFilter, colliders);
                components = colliders;

            }

            return components.NotNull().Select(col => col.GetComponent<T>()).Where(component => component != null && caller != component.gameObject).ToList();
        }

        public static List<T> PhysicsOverlapSphereTargetObjects<T>(
            PhysicsScene physicsScene,
            PhysicsScene2D physicsScene2D,
            GameType gameType,
            GameObject caller,
            Vector3 position,
            float radius,
            bool ignoreTriggerGameObjs,
            int layer) where T : Component
        {
            Component[] components = new Component[]{};
            if (gameType == GameType._3D)
            {
                Collider[] colliders = new Collider[DefaultColliderArraySize];
                physicsScene.OverlapSphere(position, radius, colliders, LayerMask.GetMask(LayerMask.LayerToName(layer)), ignoreTriggerGameObjs ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
                components = colliders;
            }
            else
            {
                Collider2D[] colliders = new Collider2D[DefaultColliderArraySize];
                ContactFilter2D contactFilter = new ContactFilter2D()
                {
                    layerMask = 1 << layer,
                    useTriggers = !ignoreTriggerGameObjs,
                };

                physicsScene2D.OverlapCircle(position, radius, contactFilter, colliders);
                components = colliders;
            }

            return components.NotNull().Select(col => col.GetComponent<T>()).Where(component => component != null && caller != component.gameObject).ToList();
        }

        public static List<T> PhysicsOverlapCapsuleTargetObjects<T>(
            PhysicsScene physicsScene,
            PhysicsScene2D physicsScene2D,
            GameType gameType,
            GameObject caller,
            Vector3 position,
            float radius,
            Quaternion rotation,
            bool ignoreTriggerGameObjs,
            int layer) where T : Component
        {
            throw new System.NotImplementedException();
        }


        private static bool HasCollision(GameObject caller, Component[] colliders)
        {
            return colliders.Any(c => c != null && c.gameObject != caller);
        }
    }

    public enum PhysicsOverlapType
    {
        OverlapBox,
        OverlapSphere,
        OverlapCapsule
    }
}