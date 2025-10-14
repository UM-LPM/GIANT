using AgentControllers;
using Base;
using UnityEngine;
using Utils;

namespace Problems.Robostrike
{
    public class RobostrikeActionExecutor : ActionExecutor
    {
        private RobostrikeEnvironmentController RobostrikeEnvironmentController;

        // Move Agent variables
        Vector3 dirToGo = Vector3.zero;
        Vector3 rotateDir = Vector3.zero;
        Vector3 rotateTurrentDir = Vector3.zero;

        int forwardAxis = 0;
        int rotateAxis = 0;
        int rotateTurrentAxis = 0;

        // Shoot missile variables
        GameObject obj;
        Rigidbody rb;
        MissileComponent mc;
        Vector3 spawnPosition;
        Quaternion spawnRotation;
        Vector3 localXDir;
        Vector3 velocity;

        Vector3 newAgentPos;
        Quaternion newAgentRotation;

        private void Awake()
        {
            RobostrikeEnvironmentController = GetComponentInParent<RobostrikeEnvironmentController>();
        }

        public override void ExecuteActions(AgentComponent agent)
        {
            MoveAgent(agent as RobostrikeAgentComponent);
            ShootMissile(agent as RobostrikeAgentComponent);
        }

        private void MoveAgent(RobostrikeAgentComponent agent)
        {
            dirToGo = Vector3.zero;
            rotateDir = Vector3.zero;
            rotateTurrentDir = Vector3.zero;

            forwardAxis = agent.ActionBuffer.GetDiscreteAction("moveForwardDirection");
            rotateAxis = agent.ActionBuffer.GetDiscreteAction("rotateDirection");
            rotateTurrentAxis = agent.ActionBuffer.GetDiscreteAction("rotateTurretDirection");

            switch (forwardAxis)
            {
                case 1:
                    dirToGo = agent.transform.up * RobostrikeEnvironmentController.ForwardSpeed;
                    break;
                case 2:
                    dirToGo = agent.transform.up * -RobostrikeEnvironmentController.ForwardSpeed;
                    break;
            }

            switch (rotateAxis)
            {
                case 1:
                    rotateDir.z = RobostrikeEnvironmentController.ForwardSpeed;
                    break;
                case 2:
                    rotateDir.z = -RobostrikeEnvironmentController.ForwardSpeed;
                    break;
            }

            switch (rotateTurrentAxis)
            {
                case 1:
                    rotateTurrentDir.z = RobostrikeEnvironmentController.ForwardSpeed;
                    break;
                case 2:
                    rotateTurrentDir.z = -RobostrikeEnvironmentController.ForwardSpeed;
                    break;
            }

            newAgentPos = agent.transform.position + (dirToGo * Time.fixedDeltaTime * RobostrikeEnvironmentController.AgentMoveSpeed);
            newAgentRotation = Quaternion.Euler(0, 0, agent.transform.rotation.eulerAngles.z + rotateDir.z * Time.fixedDeltaTime * RobostrikeEnvironmentController.AgentRotationSpeed);

            // Check if agent can be moved and rotated without colliding to other objects
            if (!PhysicsUtil.PhysicsOverlapObject(RobostrikeEnvironmentController.PhysicsScene, RobostrikeEnvironmentController.PhysicsScene2D, RobostrikeEnvironmentController.GameType, agent.gameObject, newAgentPos, RobostrikeEnvironmentController.AgentColliderExtendsMultiplier.x, Vector3.zero, newAgentRotation, PhysicsOverlapType.OverlapSphere, true, gameObject.layer)){
                agent.transform.position = newAgentPos;
                agent.transform.rotation = newAgentRotation;
            }

            // Agent turret rotation
            agent.Turret.transform.rotation = Quaternion.Euler(0, 0, agent.Turret.transform.rotation.eulerAngles.z + rotateTurrentDir.z * Time.fixedDeltaTime * RobostrikeEnvironmentController.AgentTurrentRotationSpeed);
        }

        private void ShootMissile(RobostrikeAgentComponent agent)
        {
            if (agent.ActionBuffer.GetDiscreteAction("shootMissile") == 1 && agent.NextShootTime <= RobostrikeEnvironmentController.CurrentSimulationTime && agent.AmmoComponent.Ammo > 0)
            {
                spawnPosition = agent.MissileSpawnPoint.transform.position;
                spawnRotation = agent.Turret.transform.rotation;

                localXDir = agent.MissileSpawnPoint.transform.TransformDirection(Vector3.up);
                velocity = localXDir * RobostrikeEnvironmentController.MissleLaunchSpeed;

                //Instantiate object
                obj = Instantiate(RobostrikeEnvironmentController.MissilePrefab, spawnPosition, spawnRotation, transform);
                obj.layer = gameObject.layer;
                mc = obj.GetComponent<MissileComponent>();
                mc.Parent = agent;
                mc.MissileVelocity = velocity;
                mc.RobostrikeEnvironmentController = RobostrikeEnvironmentController;
                agent.NextShootTime = RobostrikeEnvironmentController.CurrentSimulationTime + RobostrikeEnvironmentController.MissileShootCooldown;

                agent.MissileFired();

                // Add missile to missile controller
                RobostrikeEnvironmentController.MissileController.AddMissile(mc);
            }
        }
    }
}
