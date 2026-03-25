using AgentControllers;
using Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Problems.Shooter
{
    public class ShooterActionExecutor : ActionExecutor
    {
        private ShooterEnvironmentController ShooterEnvironmentController;

        // Move Agent variables
        Vector3 newAgentPos;
        Quaternion newAgentRotation;

        Vector3 dirToGo = Vector3.zero;
        Vector3 rotateDir = Vector3.zero;
        Vector3 rotateTurrentDir = Vector3.zero;

        int forwardAxis = 0;
        int rotateAxis = 0;
        int rotateTurrentAxis = 0;

        GameObject obj;
        WeaponBulletComponent wbc;
        Vector3 spawnPosition;
        Quaternion spawnRotation;
        Vector3 localXDir;
        Vector3 velocity;

        private void Awake()
        {
            ShooterEnvironmentController = GetComponentInParent<ShooterEnvironmentController>();
        }

        public override void ExecuteActions(AgentComponent agent)
        {
            MoveAgent(agent as ShooterAgentComponent);
            Shoot(agent as ShooterAgentComponent);
        }

        private void MoveAgent(ShooterAgentComponent agent)
        {
            dirToGo = Vector3.zero;
            rotateDir = Vector3.zero;
            rotateTurrentDir = Vector3.zero;

            forwardAxis = agent.ActionBuffer.GetDiscreteAction("moveForwardDirection");
            rotateAxis = agent.ActionBuffer.GetDiscreteAction("rotateDirection");

            switch (forwardAxis)
            {
                case 1:
                    dirToGo = agent.transform.up * ShooterEnvironmentController.ForwardSpeed;
                    break;
                case 2:
                    dirToGo = agent.transform.up * -ShooterEnvironmentController.ForwardSpeed;
                    break;
            }

            switch (rotateAxis)
            {
                case 1:
                    rotateDir.z = ShooterEnvironmentController.ForwardSpeed;
                    break;
                case 2:
                    rotateDir.z = -ShooterEnvironmentController.ForwardSpeed;
                    break;
            }

            switch (rotateTurrentAxis)
            {
                case 1:
                    rotateTurrentDir.z = ShooterEnvironmentController.ForwardSpeed;
                    break;
                case 2:
                    rotateTurrentDir.z = -ShooterEnvironmentController.ForwardSpeed;
                    break;
            }

            newAgentPos = agent.transform.position + (dirToGo * Time.fixedDeltaTime * ShooterEnvironmentController.AgentMoveSpeed);
            newAgentRotation = Quaternion.Euler(0, 0, agent.transform.rotation.eulerAngles.z + rotateDir.z * Time.fixedDeltaTime * ShooterEnvironmentController.AgentRotationSpeed);

            // Check if agent can be moved and rotated without colliding to other objects
            if (!PhysicsUtil.PhysicsOverlapObject(ShooterEnvironmentController.PhysicsScene, ShooterEnvironmentController.PhysicsScene2D, ShooterEnvironmentController.GameType, agent.gameObject, newAgentPos, ShooterEnvironmentController.AgentColliderExtendsMultiplier.x, Vector3.zero, newAgentRotation, PhysicsOverlapType.OverlapSphere, true, gameObject.layer))
            {
                agent.transform.position = newAgentPos;
            }

            agent.transform.rotation = newAgentRotation;
        }
    
        private void Shoot(ShooterAgentComponent agent) {
            if (agent.ActionBuffer.GetDiscreteAction("shoot") == 1 && agent.HasWeapon() && agent.NextShootTime <= ShooterEnvironmentController.CurrentSimulationSteps)
            {
                spawnPosition = agent.WeaponComponent.BulletSpawnPoint.transform.position;
                spawnRotation = agent.WeaponComponent.BulletSpawnPoint.transform.rotation;

                localXDir = agent.WeaponComponent.BulletSpawnPoint.transform.TransformDirection(Vector3.up);
                velocity = localXDir * ShooterEnvironmentController.BulletSpeed;

                //Instantiate object
                obj = Instantiate(ShooterEnvironmentController.WeaponBulletPrefab, spawnPosition, spawnRotation, transform);
                obj.layer = gameObject.layer;
                wbc = obj.GetComponent<WeaponBulletComponent>();
                wbc.Parent = agent;
                wbc.Velocity = velocity;
                //wbc.ShooterEnvironmentController = ShooterEnvironmentController;
                agent.NextShootTime = ShooterEnvironmentController.CurrentSimulationSteps + ShooterEnvironmentController.BulletShootCooldown;

                agent.BulletFired();

                // Add missile to missile controller
                ShooterEnvironmentController.BulletController.AddBullet(wbc);
            }
        }
    }
}
