using System;
using AgentControllers;
using Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Problems.DodgeBall
{
    public class DodgeBallActionExecutor : ActionExecutor
    {
        private DodgeBallEnvironmentController DodgeBallEnvironmentController;


        // Temp variables
        private DodgeBallBallComponent BallInRange;
        private Quaternion turnRotation;
        private Vector3 newPosition;

        void Awake()
        {
            DodgeBallEnvironmentController = GetComponentInParent<DodgeBallEnvironmentController>();
        }

        public override void ExecuteActions(AgentComponent agent)
        {
            MoveAgent(agent as DodgeBallAgentComponent);
            PickUpBall(agent as DodgeBallAgentComponent);
            ThrowBall(agent as DodgeBallAgentComponent);
        }

        void MoveAgent(DodgeBallAgentComponent agent)
        {
            var dirToGo = Vector3.zero;
            var rotateDir = Vector3.zero;

            var forwardAxis = agent.ActionBuffer.GetDiscreteAction("moveForwardDirection");
            var rightAxis = agent.ActionBuffer.GetDiscreteAction("moveSideDirection");
            var rotateAxis = agent.ActionBuffer.GetDiscreteAction("rotateDirection");

            switch (forwardAxis)
            {
                case 1:
                    dirToGo = agent.transform.forward * DodgeBallEnvironmentController.ForwardSpeed;
                    break;
                case 2:
                    dirToGo = agent.transform.forward * -DodgeBallEnvironmentController.ForwardSpeed;
                    break;
            }

            switch (rightAxis)
            {
                case 1:
                    dirToGo = agent.transform.right * -DodgeBallEnvironmentController.LateralSpeed;
                    break;
                case 2:
                    dirToGo = agent.transform.right * DodgeBallEnvironmentController.LateralSpeed;
                    break;
            }

            switch (rotateAxis)
            {
                case 1:
                    rotateDir = agent.transform.up * -1f;
                    break;
                case 2:
                    rotateDir = agent.transform.up * 1f;
                    break;
            }

            turnRotation = Quaternion.Euler(0.0f, rotateDir.y * Time.fixedDeltaTime * DodgeBallEnvironmentController.AgentRotationSpeed, 0.0f);
            agent.Rigidbody.MoveRotation(agent.Rigidbody.rotation * turnRotation);
            agent.Rigidbody.AddForce(dirToGo * DodgeBallEnvironmentController.AgentRunSpeed, ForceMode.VelocityChange);

            // Clamp the agent's position to prevent it from crossing the center line
            newPosition = agent.Rigidbody.position;
            if (agent.StartPosition.x > 0)
            {
                // Agent on positive X side: prevent X from becoming <= 0
                newPosition.x = Mathf.Max(0f, newPosition.x);
            }
            else
            {
                // Agent on negative X side: prevent X from becoming >= 0
                newPosition.x = Mathf.Min(0f, newPosition.x);
            }

            // Apply the clamped position
            agent.Rigidbody.MovePosition(newPosition);
        }

        void ThrowBall(DodgeBallAgentComponent agent)
        {
            if (agent.ActionBuffer.GetDiscreteAction("throwBall") == 1)
            {
                // Throw ball in the direction the agent is facing
                if (agent.BallInHand)
                {
                    agent.BallsThrown++;

                    agent.BallInHand.transform.position = agent.BallPosition.transform.position;
                    agent.BallInHand.transform.SetParent(transform);

                    agent.BallInHand.SetBallActive(true);

                    Vector3 throwDirection = agent.transform.forward * DodgeBallEnvironmentController.AgentThrowPower;
                    agent.BallInHand.Rigidbody.AddForce(throwDirection, ForceMode.VelocityChange);

                    agent.BallInHand = null;
                }
            }
        }

        void PickUpBall(DodgeBallAgentComponent agent)
        {
            if (!agent.BallInHand)
            {
                if (/*DodgeBallEnvironmentController.BallPickUpMode == BallPickUpMode.Automatic ||*/
                    (agent.ActionBuffer.GetDiscreteAction("pickupBall") == 1 /*&&
                     DodgeBallEnvironmentController.BallPickUpMode == BallPickUpMode.Manual*/))
                {
                    BallInRange = DodgeBallEnvironmentController.BallInRange(agent);
                    if (BallInRange)
                    {
                        agent.PickUpBall(BallInRange);
                    }
                }
            }
        }
    }
}
