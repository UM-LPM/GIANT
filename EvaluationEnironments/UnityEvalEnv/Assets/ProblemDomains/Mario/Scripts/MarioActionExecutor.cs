using AgentControllers;
using Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

namespace Problems.Mario
{
    public class MarioActionExecutor : ActionExecutor
    {
        private MarioEnvironmentController MarioEnvironmentController;

        [Header("Ground Check")]
        public Transform agentGroundPosition;

        private Vector2 velocity;
        private float coyoteCounter;
        private bool isGrounded;

        private void Awake()
        {
            MarioEnvironmentController = GetComponentInParent<MarioEnvironmentController>();
        }

        public override void ExecuteActions(AgentComponent agent)
        {
            MoveAgent(agent as MarioAgentComponent);
        }

        private void MoveAgent(MarioAgentComponent agent)
        {
            int sideAxis = agent.ActionBuffer.GetDiscreteAction("moveSideDirection");
            bool smallJump = agent.ActionBuffer.GetDiscreteAction("smallJump") == 1;
            bool bigJump = agent.ActionBuffer.GetDiscreteAction("bigJump") == 1;

            CheckGrounded(agent);
            HandleHorizontal(sideAxis);
            HandleJump(smallJump, bigJump);
            ApplyGravity(bigJump);
            ApplyMovement(agent);
        }

        private void HandleHorizontal(int inputX)
        {
            inputX = inputX == 2? 1: inputX == 1 ? -1 : 0;

            float targetSpeed = inputX * MarioEnvironmentController.MaxSpeed;

            float accelRate;
            if (isGrounded)
            {
                accelRate = Mathf.Abs(targetSpeed) > 0.01f
                    ? (Mathf.Sign(targetSpeed) != Mathf.Sign(velocity.x)
                        ? MarioEnvironmentController.TurnDeceleration
                        : MarioEnvironmentController.Acceleration)
                    : MarioEnvironmentController.Deceleration;
            }
            else
            {
                accelRate = MarioEnvironmentController.Acceleration * MarioEnvironmentController.AirControlMultiplier;
            }

            velocity.x = Mathf.MoveTowards(
                velocity.x,
                targetSpeed,
                accelRate * Time.fixedDeltaTime
            );
        }

        private void HandleJump(bool smallJump, bool bigJump)
        {
            if ((smallJump || bigJump) && coyoteCounter > 0)
            {
                float jumpStrength = MarioEnvironmentController.JumpVelocity;

                // Big jump = speed-scaled bonus
                if (bigJump)
                    jumpStrength += Mathf.Abs(velocity.x) * MarioEnvironmentController.SpeedJumpBonus;

                velocity.y = jumpStrength;

                coyoteCounter = 0;
            }
        }

        private void ApplyGravity(bool bigJump)
        {
            if(velocity.y == 0 && isGrounded)
            {
                return;
            }
            
            if (velocity.y < 0)
            {
                velocity.y -= MarioEnvironmentController.Gravity * MarioEnvironmentController.FallMultiplier * Time.fixedDeltaTime;
            }
            else if (velocity.y > 0 && !bigJump)
            {
                velocity.y -= MarioEnvironmentController.Gravity * MarioEnvironmentController.LowJumpMultiplier * Time.fixedDeltaTime;
            }
            else
            {
                velocity.y -= MarioEnvironmentController.Gravity * Time.fixedDeltaTime;
            }
        }

        private void ApplyMovement(MarioAgentComponent agent)
        {
            Vector2 move = velocity * Time.fixedDeltaTime;

            // Handle gorizontal movement collisions
            if (move.x != 0)
            {
                float dirX = Mathf.Sign(move.x);
                float distance = Mathf.Abs(move.x) + MarioEnvironmentController.SkinWidth;

                var results = PhysicsUtil.BoxCast2D(
                    MarioEnvironmentController.PhysicsScene2D,
                    agent.gameObject,
                    agent.BoxCollider2D.bounds.center,
                    agent.BoxCollider2D.bounds.size / 2f,
                    0f,
                    Vector2.right * dirX,
                    distance,
                    true,
                    agent.gameObject.layer
                );

                for (int i = 0; i < results.Length; i++)
                {
                    var hit = results[i];
                    if (hit.collider.gameObject == agent.gameObject)
                        continue;

                    var finish = hit.collider.GetComponent<FinishComponent>();
                    if (finish != null)
                    {
                        MarioEnvironmentController.OnAgentReachedFinish(agent, finish);
                    }

                    float hitDistance = results[i].distance - MarioEnvironmentController.SkinWidth;
                    move.x = hitDistance * dirX;
                    velocity.x = 0;
                    break;
                }
            }

            agent.transform.position += new Vector3(move.x, 0, 0);

            // Handle vertical movement collisions
            if (move.y != 0)
            {
                float dirY = Mathf.Sign(move.y);
                float distance = Mathf.Abs(move.y) + MarioEnvironmentController.SkinWidth;

                var results = PhysicsUtil.BoxCast2D(
                    MarioEnvironmentController.PhysicsScene2D,
                    agent.gameObject,
                    agent.BoxCollider2D.bounds.center,
                    agent.BoxCollider2D.bounds.size / 2f,
                    0f,
                    Vector2.up * dirY,
                    distance,
                    true,
                    agent.gameObject.layer
                );

                for (int i = 0; i < results.Length; i++)
                {
                    var hit = results[i];
                    if (hit.collider.gameObject == agent.gameObject)
                        continue;

                    var finish = hit.collider.GetComponent<FinishComponent>();
                    if (finish != null)
                    {
                        MarioEnvironmentController.OnAgentReachedFinish(agent, finish);
                    }

                    float hitDistance = results[i].distance - MarioEnvironmentController.SkinWidth;
                    move.y = hitDistance * dirY;
                    velocity.y = 0; // ceiling or ground hit
                    break;
                }
            }

            agent.transform.position += new Vector3(0, move.y, 0);
        }

        private void CheckGrounded(MarioAgentComponent agent)
        {
            var results = PhysicsUtil.BoxCast2D(
                MarioEnvironmentController.PhysicsScene2D,
                agent.gameObject,
                agent.BoxCollider2D.bounds.center,
                new Vector2(agent.BoxCollider2D.bounds.size.x / 2, agent.BoxCollider2D.bounds.size.y),
                0f,
                Vector2.down,
                MarioEnvironmentController.SkinWidth,
                true,
                agent.gameObject.layer
            );

            isGrounded = false;
            for (int i = 0; i < results.Length; i++)
            {
                if (results[i].collider.gameObject != agent.gameObject)
                {
                    isGrounded = true;
                    break;
                }
            }

            if (isGrounded && agent.transform.position.y < agentGroundPosition.position.y)
            {
                agent.transform.position = new Vector3(agent.transform.position.x, agentGroundPosition.position.y, agent.transform.position.z);
            }

            if (isGrounded && velocity.y <= 0)
            {
                velocity.y = 0;
                coyoteCounter = MarioEnvironmentController.CoyoteTime;
            }
            else
            {
                coyoteCounter -= Time.fixedDeltaTime;
            }
        }
    }
}
