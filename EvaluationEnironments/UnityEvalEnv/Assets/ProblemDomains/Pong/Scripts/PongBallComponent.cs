using UnityEngine;
using Utils;

namespace Problems.Pong
{
    public class PongBallComponent : MonoBehaviour
    {
        public float Radius { get; private set; }
        public int NumOfSpawns { get; set; }

        private Vector2 velocity;
        private CircleCollider2D circleCollider;

        private PongEnvironmentController PongEnvironmentController;

        Vector2 normal;
        Vector2 reflectedVelocity;
        Vector2 newPosition;

        Vector2 currentPosition;
        Vector2 displacement;
        float distance;

        RaycastHit2D[] hits;

        private void Awake()
        {
            PongEnvironmentController = GetComponentInParent<PongEnvironmentController>();
            circleCollider = GetComponent<CircleCollider2D>();
        }

        void Start()
        {
            if (circleCollider == null)
            {
                Debug.LogError("CircleCollider2D component not found!", this);
                enabled = false;
                return;
            }

            Radius = circleCollider.radius * transform.localScale.x;
        }

        public void OnStep()
        {
            // Check collisions
            CheckForObjectCollisions();

            // Move
            Move();
        }
        void CheckForObjectCollisions()
        {
            var pongAgent = PhysicsUtil.PhysicsOverlapSphereTargetObject<PongAgentComponent>(
                PongEnvironmentController.PhysicsScene,
                PongEnvironmentController.PhysicsScene2D,
                PongEnvironmentController.GameType,
                gameObject,
            transform.position,
                Radius + PongEnvironmentController.BallCollisionCheckRadius,
                true,
                gameObject.layer
            );

            if (pongAgent != null)
            {
                pongAgent.PongBallBounce(this);
                //velocity = Vector2.Reflect(velocity, normal);
            }
        }

        public void Move()
        {
            currentPosition = transform.position;
            displacement = velocity * Time.fixedDeltaTime;
            distance = displacement.magnitude;

            if (distance > 0f)
            {
                hits = PhysicsUtil.PhysicsCircleCast2D(
                PongEnvironmentController.PhysicsScene2D,
                gameObject,
                currentPosition,
                Radius,
                velocity.normalized,
                distance,
                true,
                gameObject.layer);

                if (hits.Length > 0)
                {
                    // Handle collision (take first hit)
                    if (hits[0].collider != null && hits[0].collider.gameObject != gameObject)
                    {
                        HandleCollision(hits[0]);
                    }
                }
                else
                {
                    transform.position = currentPosition + displacement;
                }
            }
        }

        void HandleCollision(RaycastHit2D hit)
        {
            normal = hit.normal;

            // Reflect velocity with bounce
            reflectedVelocity = Vector2.Reflect(velocity, normal);
            velocity = reflectedVelocity;

            // Offset the ball slightly away from the surface
            newPosition = hit.point + normal * (Radius + 0.001f);
            transform.position = newPosition;
        }

        public void SetVelocity(Vector2 newVelocity)
        {
            velocity = newVelocity;
        }

        public void Respawn(Vector2 spawnPos, Quaternion rotation)
        {
            transform.position = spawnPos;
            transform.rotation = rotation;
            velocity = Vector2.zero;
            NumOfSpawns++;
        }
    }
}