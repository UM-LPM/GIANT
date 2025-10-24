using Base;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils
{
    public static class PhysicsUtil
    {
        public static int DefaultColliderArraySize = 32;

        // Reusable buffers to avoid allocations.
        private static readonly Collider[] ColliderBuffer3D = new Collider[DefaultColliderArraySize];
        private static readonly Collider2D[] ColliderBuffer2D = new Collider2D[DefaultColliderArraySize];
        private static readonly RaycastHit2D[] RaycastHit2DBuffer = new RaycastHit2D[DefaultColliderArraySize];

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
            int layerMask = 1 << layer;

            if (gameType == GameType._3D)
            {
                // PhysicsScene.OverlapBox returns number of hits and writes into provided buffer
                int count = physicsScene.OverlapBox(position, halfExtends, ColliderBuffer3D, rotation, layerMask,
                    ignoreTriggerGameObjs ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);

                for (int i = 0; i < count; i++)
                {
                    var c = ColliderBuffer3D[i];
                    if (c != null && c.gameObject != caller) return true;
                }

                return false;
            }
            else
            {
                var filter = new ContactFilter2D
                {
                    layerMask = layerMask,
                    useTriggers = !ignoreTriggerGameObjs
                };
                
                int count = physicsScene2D.OverlapBox(position, halfExtends, rotation.eulerAngles.z, filter, ColliderBuffer2D);

                for (int i = 0; i < count; i++)
                {
                    var c = ColliderBuffer2D[i];
                    if (c != null && c.gameObject != caller) return true;
                }

                return false;
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
            int layerMask = 1 << layer;

            if (gameType == GameType._3D)
            {
                int count = physicsScene.OverlapSphere(position, radius, ColliderBuffer3D, layerMask,
                    ignoreTriggerGameObjs ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);

                for (int i = 0; i < count; i++)
                {
                    var c = ColliderBuffer3D[i];
                    if (c != null && c.gameObject != caller) return true;
                }

                return false;
            }
            else
            {
                var filter = new ContactFilter2D
                {
                    layerMask = layerMask,
                    useTriggers = !ignoreTriggerGameObjs
                };
                int count = physicsScene2D.OverlapCircle(position, radius, filter, ColliderBuffer2D);

                for (int i = 0; i < count; i++)
                {
                    var c = ColliderBuffer2D[i];
                    if (c != null && c.gameObject != caller) return true;
                }

                return false;
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
            var results = new List<T>(8);
            int layerMask = 1 << layer;

            if (gameType == GameType._3D)
            {
                int count = physicsScene.OverlapSphere(position, radius, ColliderBuffer3D, layerMask,
                    ignoreTriggerGameObjs ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);

                for (int i = 0; i < count; i++)
                {
                    var c = ColliderBuffer3D[i];
                    if (c == null || c.gameObject == caller) continue;
                    var comp = c.GetComponent<T>();
                    if (comp != null) results.Add(comp);
                }
            }
            else
            {
                var filter = new ContactFilter2D
                {
                    layerMask = layerMask,
                    useTriggers = !ignoreTriggerGameObjs
                };
                
                int count = physicsScene2D.OverlapCircle(position, radius, filter, ColliderBuffer2D);
                
                for (int i = 0; i < count; i++)
                {
                    var c = ColliderBuffer2D[i];
                    if (c == null || c.gameObject == caller) continue;
                    var comp = c.GetComponent<T>();
                    if (comp != null) results.Add(comp);
                }
            }

            return results.Count == 0 ? Array.Empty<T>() : results.ToArray();
        }

        public static T[] PhysicsOverlapBox<T>(PhysicsScene physicsScene,
            PhysicsScene2D physicsScene2D,
            GameType gameType,
            GameObject caller,
            Vector3 position,
            Quaternion rotation,
            Vector3 halfExtends,
            bool ignoreTriggerGameObjs,
            int layer)
        {
            var results = new List<T>(8);
            int layerMask = 1 << layer;
            if (gameType == GameType._3D)
            {
                int count = physicsScene.OverlapBox(position, halfExtends, ColliderBuffer3D, rotation, layerMask,
                    ignoreTriggerGameObjs ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
                for (int i = 0; i < count; i++)
                {
                    var c = ColliderBuffer3D[i];
                    if (c == null || c.gameObject == caller) continue;
                    var comp = c.GetComponent<T>();
                    if (comp != null) results.Add(comp);
                }
            }
            else
            {
                var filter = new ContactFilter2D
                {
                    layerMask = layerMask,
                    useTriggers = !ignoreTriggerGameObjs
                };
                
                int count = physicsScene2D.OverlapBox(position, halfExtends, rotation.eulerAngles.z, filter, ColliderBuffer2D);
                
                for (int i = 0; i < count; i++)
                {
                    var c = ColliderBuffer2D[i];
                    if (c == null || c.gameObject == caller) continue;
                    var comp = c.GetComponent<T>();
                    if (comp != null) results.Add(comp);
                }
            }
            return results.Count == 0 ? Array.Empty<T>() : results.ToArray();
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
            throw new NotImplementedException();
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
            PhysicsScene2D physicsScene2D, GameType gameType, GameObject caller, Vector3 position, Quaternion rotation, Vector3 halfExtends, bool ignoreTriggerGameObjs, int layer) where T : Component
        {
            int layerMask = 1 << layer;

            if (gameType == GameType._3D)
            {
                int count = physicsScene.OverlapBox(position, halfExtends, ColliderBuffer3D, rotation, layerMask,
                    ignoreTriggerGameObjs ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);

                for (int i = 0; i < count; i++)
                {
                    var col = ColliderBuffer3D[i];
                    if (col == null || col.gameObject == caller) continue;
                    var comp = col.GetComponent<T>();
                    if (comp != null) return comp;
                }
            }
            else
            {
                var filter = new ContactFilter2D { layerMask = layerMask, useTriggers = !ignoreTriggerGameObjs };
                int count = physicsScene2D.OverlapBox(position, halfExtends, rotation.eulerAngles.z, filter, ColliderBuffer2D);

                for (int i = 0; i < count; i++)
                {
                    var col = ColliderBuffer2D[i];
                    if (col == null || col.gameObject == caller) continue;
                    var comp = col.GetComponent<T>();
                    if (comp != null) return comp;
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
            int layerMask = 1 << layer;

            if (gameType == GameType._3D)
            {
                int count = physicsScene.OverlapSphere(position, radius, ColliderBuffer3D, layerMask,
                    ignoreTriggerGameObjs ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);

                for (int i = 0; i < count; i++)
                {
                    var col = ColliderBuffer3D[i];
                    if (col == null || col.gameObject == caller) continue;
                    var comp = col.GetComponent<T>();
                    if (comp != null) return comp;
                }
            }
            else
            {
                var filter = new ContactFilter2D { layerMask = layerMask, useTriggers = !ignoreTriggerGameObjs };
                int count = physicsScene2D.OverlapCircle(position, radius, filter, ColliderBuffer2D);

                for (int i = 0; i < count; i++)
                {
                    var col = ColliderBuffer2D[i];
                    if (col == null || col.gameObject == caller) continue;
                    var comp = col.GetComponent<T>();
                    if (comp != null) return comp;
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
            throw new NotImplementedException();
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
                    return new List<T>();
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
            var results = new List<T>(4);
            int layerMask = 1 << layer;

            if (gameType == GameType._3D)
            {
                int count = physicsScene.OverlapBox(position, halfExtends, ColliderBuffer3D, rotation, layerMask,
                    ignoreTriggerGameObjs ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);

                for (int i = 0; i < count; i++)
                {
                    var col = ColliderBuffer3D[i];
                    if (col == null || col.gameObject == caller) continue;
                    var comp = col.GetComponent<T>();
                    if (comp != null) results.Add(comp);
                }
            }
            else
            {
                var filter = new ContactFilter2D { layerMask = layerMask, useTriggers = !ignoreTriggerGameObjs };
                int count = physicsScene2D.OverlapBox(position, halfExtends, rotation.eulerAngles.z, filter, ColliderBuffer2D);

                for (int i = 0; i < count; i++)
                {
                    var col = ColliderBuffer2D[i];
                    if (col == null || col.gameObject == caller) continue;
                    var comp = col.GetComponent<T>();
                    if (comp != null) results.Add(comp);
                }
            }

            return results;
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
            var results = new List<T>(4);
            int layerMask = 1 << layer;

            if (gameType == GameType._3D)
            {
                int count = physicsScene.OverlapSphere(position, radius, ColliderBuffer3D, layerMask,
                    ignoreTriggerGameObjs ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);

                for (int i = 0; i < count; i++)
                {
                    var col = ColliderBuffer3D[i];
                    if (col == null || col.gameObject == caller) continue;
                    var comp = col.GetComponent<T>();
                    if (comp != null) results.Add(comp);
                }
            }
            else
            {
                var filter = new ContactFilter2D { layerMask = layerMask, useTriggers = !ignoreTriggerGameObjs };
                int count = physicsScene2D.OverlapCircle(position, radius, filter, ColliderBuffer2D);

                for (int i = 0; i < count; i++)
                {
                    var col = ColliderBuffer2D[i];
                    if (col == null || col.gameObject == caller) continue;
                    var comp = col.GetComponent<T>();
                    if (comp != null) results.Add(comp);
                }
            }

            return results;
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
            throw new NotImplementedException();
        }

        public static RaycastHit2D[] PhysicsCircleCast2D(
            PhysicsScene2D physicsScene2D,
            GameObject caller,
            Vector3 position,
            float radius,
            Vector2 direction,
            float distance,
            bool ignoreTriggerGameObjs,
            int layer)
        {
            var results = new List<RaycastHit2D>();
            var filter = new ContactFilter2D { layerMask = 1 << layer, useTriggers = !ignoreTriggerGameObjs };

            int count = physicsScene2D.CircleCast(position, radius, direction, distance, filter, RaycastHit2DBuffer);

            for (int i = 0; i < count; i++)
            {
                var hit = RaycastHit2DBuffer[i];
                if (hit.collider == null || hit.collider.gameObject == caller) continue;
                results.Add(hit);
            }

            return results.Count == 0 ? Array.Empty<RaycastHit2D>() : results.ToArray();
        }

        public static Collider2D[] PhysicsOverlapBox2D(
            PhysicsScene2D physicsScene2D,
            GameObject caller,
            Vector2 position,
            Quaternion rotation,
            Vector2 halfExtends,
            bool ignoreTriggerGameObjs,
            int layer)
        {
            var filter = new ContactFilter2D { layerMask = 1 << layer, useTriggers = !ignoreTriggerGameObjs };

            int count = physicsScene2D.OverlapBox(position, halfExtends, rotation.eulerAngles.z, filter, ColliderBuffer2D);

            var hits = new List<Collider2D>();
            for (int i = 0; i < count; i++)
            {
                var c = ColliderBuffer2D[i];
                if (c != null && c.gameObject != caller)
                {
                    hits.Add(c);
                }
            }
            return hits.ToArray();
        }

        public static Collider[] PhysicsOverlapBox3D(
            PhysicsScene physicsScene,
            GameObject caller,
            Vector3 position,
            Quaternion rotation,
            Vector3 halfExtends,
            bool ignoreTriggerGameObjs,
            int layer)
        {
            int layerMask = 1 << layer;

            int count = physicsScene.OverlapBox(position, halfExtends, ColliderBuffer3D, rotation, layerMask,
                ignoreTriggerGameObjs ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);

            var hits = new List<Collider>();
            for (int i = 0; i < count; i++)
            {
                var c = ColliderBuffer3D[i];
                if (c != null && c.gameObject != caller)
                {
                    hits.Add(c);
                }
            }
            return hits.ToArray();
        }
    }

    public enum PhysicsOverlapType
    {
        OverlapBox,
        OverlapSphere,
        OverlapCapsule
    }
}
