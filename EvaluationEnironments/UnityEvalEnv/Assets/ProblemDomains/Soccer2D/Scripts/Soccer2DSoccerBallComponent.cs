using Base;
using System.Linq;
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

        Soccer2DEnvironmentController Soccer2DEnvironmentController;

        public float Radius { get; private set; }

        public int NumOfSpawns { get; set; }

        private Vector2 velocity;
        private CircleCollider2D circleCollider;

        RaycastHit2D[] hits;

        Vector2 normal;
        Vector2 reflectedVelocity;
        Vector2 newPosition;

        Vector2 currentPosition;
        Vector2 displacement;
        float distance;

        private void Awake()
        {
            Soccer2DEnvironmentController = GetComponentInParent<Soccer2DEnvironmentController>();
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
        }

        public void OnStep()
        {
            // Apply damping
            velocity *= Soccer2DEnvironmentController.BallDampingFactor;

            // Stop ball if slow
            if (velocity.magnitude < Soccer2DEnvironmentController.BallMinVelocityThreshold)
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
                Soccer2DEnvironmentController.PhysicsScene2D,
                gameObject,
                currentPosition,
                Radius,
                velocity.normalized,
                distance,
                true,
                gameObject.layer);

                if(hits.Length > 0)
                {
                    // Take the closest hit
                    var closestHit = hits.Where(h => h.collider != null && h.collider.gameObject != gameObject).OrderBy(h => h.distance).First();
                    HandleCollision(closestHit);
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
            reflectedVelocity = Vector2.Reflect(velocity, normal) * Soccer2DEnvironmentController.BallBounceFactor;
            velocity = reflectedVelocity;

            // Offset the ball slightly away from the surface
            newPosition = hit.point + normal * (Radius);
            transform.position = newPosition;
        }

        void CheckForObjectCollisions()
        {
            var agents = PhysicsUtil.PhysicsOverlapSphere<Soccer2DAgentComponent>(
                Soccer2DEnvironmentController.PhysicsScene,
                Soccer2DEnvironmentController.PhysicsScene2D,
                Soccer2DEnvironmentController.GameType,
                gameObject,
                transform.position,
                Radius + Soccer2DEnvironmentController.BallCollisionCheckRadius,
                true,
                gameObject.layer
            );

            if (agents == null || agents.Length == 0)
                return;

            Vector3 totalForce = Vector3.zero;

            foreach (var agent in agents)
            {
                LastTouchedAgent = agent;
                var dir = (transform.position - agent.transform.position).normalized;
                var agentPower = Mathf.Max(0.05f, agent.Velocity.magnitude / Soccer2DEnvironmentController.AgentMaxAcceleration);

                totalForce += dir * (Soccer2DEnvironmentController.KickPower * agentPower);

                if (agentPower > 0.05f)
                {
                    Soccer2DEnvironmentController.AgentTouchedSoccerBall(agent);
                }
            }

            if (totalForce != Vector3.zero)
            {
                AddForce(totalForce);
            }
        }

        public Vector2 GetVelocity()
        {
            return velocity;
        }

        public void AddForce(Vector2 force)
        {
            velocity += force;
            if (velocity.magnitude > Soccer2DEnvironmentController.BallMaxVelocity)
            {
                velocity = velocity.normalized * Soccer2DEnvironmentController.BallMaxVelocity;
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
