using AgentControllers;
using Base;
using UnityEngine;
using Utils;

namespace Problems.Moba_game
{
    public class Moba_gameActionExecutor : ActionExecutor
    {
        private Moba_gameEnvironmentController Moba_gameEnvironmentController;

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
            Moba_gameEnvironmentController = GetComponentInParent<Moba_gameEnvironmentController>();
        }

        public override void ExecuteActions(AgentComponent agent)
        {
            MoveAgent(agent as Moba_gameAgentComponent);
            ShootMissile(agent as Moba_gameAgentComponent);
        }

        private void MoveAgent(Moba_gameAgentComponent agent)
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
                    dirToGo = agent.transform.up * Moba_gameEnvironmentController.ForwardSpeed;
                    break;
                case 2:
                    dirToGo = agent.transform.up * -Moba_gameEnvironmentController.ForwardSpeed;
                    break;
            }

            switch (rotateAxis)
            {
                case 1:
                    rotateDir.z = Moba_gameEnvironmentController.ForwardSpeed;
                    break;
                case 2:
                    rotateDir.z = -Moba_gameEnvironmentController.ForwardSpeed;
                    break;
            }

            switch (rotateTurrentAxis)
            {
                case 1:
                    rotateTurrentDir.z = Moba_gameEnvironmentController.ForwardSpeed;
                    break;
                case 2:
                    rotateTurrentDir.z = -Moba_gameEnvironmentController.ForwardSpeed;
                    break;
            }

            newAgentPos = UnityUtils.RoundToDecimals(agent.transform.position + (dirToGo * Time.fixedDeltaTime * Moba_gameEnvironmentController.AgentMoveSpeed), 2);
            newAgentRotation = Quaternion.Euler(0, 0, agent.transform.rotation.eulerAngles.z + UnityUtils.RoundToDecimals(rotateDir.z * Time.fixedDeltaTime * Moba_gameEnvironmentController.AgentRotationSpeed, 2));

            // Check if agent can be moved and rotated without colliding to other objects
            if (!PhysicsUtil.PhysicsOverlapObject(Moba_gameEnvironmentController.GameType, agent.gameObject, newAgentPos, Moba_gameEnvironmentController.AgentColliderExtendsMultiplier.x, Vector3.zero, newAgentRotation, PhysicsOverlapType.OverlapSphere, true, gameObject.layer, Moba_gameEnvironmentController.DefaultLayer)){
                agent.transform.position = newAgentPos;
                agent.transform.rotation = newAgentRotation;
            }

            // Agent turret rotation
            agent.Turret.transform.rotation = Quaternion.Euler(0, 0, agent.Turret.transform.rotation.eulerAngles.z + UnityUtils.RoundToDecimals(rotateTurrentDir.z * Time.fixedDeltaTime * Moba_gameEnvironmentController.AgentTurrentRotationSpeed, 2));
        }

        private void ShootMissile(Moba_gameAgentComponent agent)
        {
            if (agent.ActionBuffer.GetDiscreteAction("shootMissile") == 1 && agent.NextShootTime <= Moba_gameEnvironmentController.CurrentSimulationTime && agent.AmmoComponent.Ammo > 0)
            {
                spawnPosition = agent.MissileSpawnPoint.transform.position;
                spawnRotation = agent.Turret.transform.rotation;

                localXDir = agent.MissileSpawnPoint.transform.TransformDirection(Vector3.up);
                velocity = localXDir * Moba_gameEnvironmentController.MissleLaunchSpeed;

                //Instantiate object
                obj = Instantiate(Moba_gameEnvironmentController.MissilePrefab, spawnPosition, spawnRotation, transform);
                obj.layer = gameObject.layer;
                mc = obj.GetComponent<MissileComponent>();
                mc.Parent = agent;
                mc.MissileVelocity = velocity;
                mc.Moba_gameEnvironmentController = Moba_gameEnvironmentController;
                agent.NextShootTime = Moba_gameEnvironmentController.CurrentSimulationTime + Moba_gameEnvironmentController.MissileShootCooldown;

                agent.MissileFired();

                // Add missile to missile controller
                Moba_gameEnvironmentController.MissileController.AddMissile(mc);
            }
        }
    }
}
