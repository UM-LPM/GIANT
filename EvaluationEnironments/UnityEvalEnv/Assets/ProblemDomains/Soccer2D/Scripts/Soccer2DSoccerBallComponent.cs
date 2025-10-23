using UnityEngine;
using Utils;

namespace Problems.Soccer2D
{
    public class Soccer2DSoccerBallComponent : MonoBehaviour
    {
        public Vector2 StartPosition { get; set; }
        public Quaternion StartRotation { get; set; }
        public Soccer2DAgentComponent LastTouchedAgent { get; set; }

        public float BallToPurpleGoalDistance { get; set; }
        public float BallToBlueGoalDistance { get; set; }

        Soccer2DEnvironmentController SoccerEnvironmentController;

        public float Radius { get; private set; }

        public int NumOfSpawns { get; set; }

        private Vector2 velocity;
        private CircleCollider2D circleCollider;
        private ContactFilter2D contactFilter2D;

        RaycastHit2D[] hits;
        Soccer2DGoalComponent goal;

        Vector2 normal;
        Vector2 reflectedVelocity;
        Vector2 newPosition;

        Vector2 currentPosition;
        Vector2 displacement;
        float distance;

        private void Awake()
        {
            SoccerEnvironmentController = GetComponentInParent<Soccer2DEnvironmentController>();
            circleCollider = GetComponent<CircleCollider2D>();
            StartPosition = transform.position;
            StartRotation = transform.rotation;
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

            contactFilter2D = new ContactFilter2D();
            contactFilter2D.useTriggers = false;
            contactFilter2D.SetLayerMask(LayerMask.GetMask(LayerMask.LayerToName(SoccerEnvironmentController.gameObject.layer)));
        }

        public void OnStep()
        {
            // Apply damping
            velocity *= SoccerEnvironmentController.BallDampingFactor;

            // Stop ball if slow
            if (velocity.magnitude < SoccerEnvironmentController.BallMinVelocityThreshold)
            {
                velocity = Vector2.zero;
            }

            // Check collisions
            CheckForObjectCollisions();

            // Move
            Move();
        }
        
        void Move()
        {
            currentPosition = transform.position;
            displacement = velocity * Time.fixedDeltaTime;
            distance = displacement.magnitude;

            if(distance > 0f)
            {
                hits = PhysicsUtil.PhysicsCircleCast2D(
                SoccerEnvironmentController.PhysicsScene2D,
                gameObject,
                currentPosition,
                Radius,
                velocity.normalized,
                distance,
                true,
                gameObject.layer);

                if(hits.Length > 0)
                {
                    // If goal was not hit, handle normal collision (take first hit)
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
            reflectedVelocity = Vector2.Reflect(velocity, normal) * SoccerEnvironmentController.BallBounceFactor;
            velocity = reflectedVelocity;

            // Offset the ball slightly away from the surface
            newPosition = hit.point + normal * (Radius + 0.001f);
            transform.position = newPosition;
        }

        void CheckForObjectCollisions()
        {
            var soccerAgent = PhysicsUtil.PhysicsOverlapSphereTargetObject<Soccer2DAgentComponent>(
                SoccerEnvironmentController.PhysicsScene,
                SoccerEnvironmentController.PhysicsScene2D,
                SoccerEnvironmentController.GameType,
                gameObject,
                transform.position,
                Radius + SoccerEnvironmentController.BallCollisionCheckRadius,
                true,
                gameObject.layer
            );

            if (soccerAgent != null)
            {
                soccerAgent.KickSoccerBall(this);
            }
        }

        public Vector2 GetVelocity()
        {
            return velocity;
        }

        public void AddForce(Vector2 force)
        {
            velocity += force;
            if (velocity.magnitude > SoccerEnvironmentController.BallMaxVelocity)
            {
                velocity = velocity.normalized * SoccerEnvironmentController.BallMaxVelocity;
            }
        }

        public void Respawn(Vector2 spawnPos, Quaternion rotation)
        {
            transform.position = spawnPos;
            transform.rotation = rotation;
            velocity = Vector2.zero;
            LastTouchedAgent = null;
            NumOfSpawns++;
        }
    }
}
