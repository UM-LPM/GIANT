using System;
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
        Vector3 spawnPosition1;
        Quaternion spawnRotation;
        Vector3 localXDir;
        Vector3 velocity;

        Vector3 newAgentPos;
        Quaternion newAgentRotation;
        public float laserRange = 300f;

        private void Awake()
        {
            Moba_gameEnvironmentController = GetComponentInParent<Moba_gameEnvironmentController>();
        }

        public override void ExecuteActions(AgentComponent agent)
        {
            //MoveAgent(agent as Moba_gameAgentComponent);
            //ShootMissile(agent as Moba_gameAgentComponent);
            HandleMovement(agent as Moba_gameAgentComponent);
            ShootLaser(agent as Moba_gameAgentComponent);
        }

        void HandleMovement(Moba_gameAgentComponent agent)
        {
            var thrust = Moba_gameEnvironmentController.LavaAgentForwardThrust;
            var torque = Moba_gameEnvironmentController.LavaAgentTourque;
            if (agent.AgentType == "ice")
            {
                thrust = Moba_gameEnvironmentController.IceAgentForwardThrust;
                torque = Moba_gameEnvironmentController.IceAgentTourque;
            }

            Rigidbody2D rigidbodi = agent.GetComponent<Rigidbody2D>();
            forwardAxis = agent.ActionBuffer.GetDiscreteAction("moveForwardDirection");
            switch (forwardAxis)
            {
                case 1:
                    rigidbodi.AddRelativeForce(new Vector2(0, thrust));
                    break;
            }
            rotateAxis = agent.ActionBuffer.GetDiscreteAction("rotateDirection");
            switch (rotateAxis)
            {
                case 1:
                    rigidbodi.AddTorque(torque);
                    break;
                case 2:
                    rigidbodi.AddTorque(-torque);
                    break;
            }
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
            if (!PhysicsUtil.PhysicsOverlapObject(Moba_gameEnvironmentController.GameType, agent.gameObject, newAgentPos, Moba_gameEnvironmentController.AgentColliderExtendsMultiplier.x, Vector3.zero, newAgentRotation, PhysicsOverlapType.OverlapSphere, true, gameObject.layer, Moba_gameEnvironmentController.DefaultLayer))
            {
                agent.transform.position = newAgentPos;
                agent.transform.rotation = newAgentRotation;
            }

            // Agent turret rotation
            agent.Turret.transform.rotation = Quaternion.Euler(0, 0, agent.Turret.transform.rotation.eulerAngles.z + UnityUtils.RoundToDecimals(rotateTurrentDir.z * Time.fixedDeltaTime * Moba_gameEnvironmentController.AgentTurrentRotationSpeed, 2));
        }

        void ShootLaser(Moba_gameAgentComponent agent)
        {
            if (agent.ActionBuffer.GetDiscreteAction("shootMissile") == 1 && agent.NextShootTime <= Moba_gameEnvironmentController.CurrentSimulationTime)
            {
                spawnPosition = agent.MissileSpawnPoint.transform.position;
                localXDir = agent.MissileSpawnPoint.transform.TransformDirection(Vector3.up);
                Vector3 laserEnd = spawnPosition + localXDir * laserRange;
                RaycastHit2D[] hits = Physics2D.RaycastAll(spawnPosition, localXDir, laserRange);
                foreach (var hit in hits)
                {
                    if (!hit.collider.isTrigger)
                    {
                        laserEnd = hit.point;
                        HandleHit(hit.collider, agent);
                        break;
                    }
                }
                LineRenderer lineRenderer = agent.GetComponent<LineRenderer>();
                StartCoroutine(ShowLaser(spawnPosition, laserEnd, lineRenderer));
                agent.LaserFired();

                if (agent.AgentType == "ice")
                {
                    spawnPosition1 = agent.MissileSpawnPoint1.transform.position;
                    localXDir = agent.MissileSpawnPoint1.transform.TransformDirection(Vector3.up);
                    Vector3 laserEnd1 = spawnPosition1 + localXDir * laserRange;
                    RaycastHit2D[] hits1 = Physics2D.RaycastAll(spawnPosition1, localXDir, laserRange);
                    foreach (var hit in hits1)
                    {
                        if (!hit.collider.isTrigger)
                        {
                            laserEnd1 = hit.point;
                            HandleHit(hit.collider, agent);
                            break;
                        }
                    }
                    GameObject laserChild = agent.transform.Find("LineRenderer").gameObject;
                    LineRenderer lineRenderer1 = laserChild.GetComponent<LineRenderer>();
                    StartCoroutine(ShowLaser(spawnPosition1, laserEnd1, lineRenderer1));
                    agent.LaserFired();

                }
                agent.NextShootTime = Moba_gameEnvironmentController.CurrentSimulationTime + Moba_gameEnvironmentController.MissileShootCooldown;
                
            }
        }

        private void HandleHit(Collider2D collider, Moba_gameAgentComponent agent)
        {
            if (collider.CompareTag("Agent"))
            {
                Moba_gameAgentComponent hitAgent = collider.GetComponent<Moba_gameAgentComponent>();
                Moba_gameEnvironmentController.LaserTankHit(agent, hitAgent);
            }
            else if (collider.CompareTag("Object5"))
            {
                Moba_gameBaseComponent hitBase = collider.GetComponent<Moba_gameBaseComponent>();
                Moba_gameEnvironmentController.LaserBaseHit(agent, hitBase);
            }
        }

        System.Collections.IEnumerator ShowLaser(Vector3 spawnPosition, Vector3 endPoint, LineRenderer lineRenderer)
        {
            lineRenderer.SetPosition(0, spawnPosition);
            lineRenderer.SetPosition(1, endPoint);
            lineRenderer.enabled = true;
            yield return new WaitForSeconds(0.1f);
            lineRenderer.enabled = false;
        }


        private void ShootMissile(Moba_gameAgentComponent agent)
        {
            // if (agent.ActionBuffer.GetDiscreteAction("shootMissile") == 1 && agent.NextShootTime <= Moba_gameEnvironmentController.CurrentSimulationTime && agent.AmmoComponent.Ammo > 0)
            // {
            //     spawnPosition = agent.MissileSpawnPoint.transform.position;
            //     spawnRotation = agent.Turret.transform.rotation;

            //     localXDir = agent.MissileSpawnPoint.transform.TransformDirection(Vector3.up);
            //     velocity = localXDir * Moba_gameEnvironmentController.MissleLaunchSpeed;

            //     //Instantiate object
            //     obj = Instantiate(Moba_gameEnvironmentController.MissilePrefab, spawnPosition, spawnRotation, transform);
            //     obj.layer = gameObject.layer;
            //     mc = obj.GetComponent<MissileComponent>();
            //     mc.Parent = agent;
            //     mc.MissileVelocity = velocity;
            //     mc.Moba_gameEnvironmentController = Moba_gameEnvironmentController;
            //     agent.NextShootTime = Moba_gameEnvironmentController.CurrentSimulationTime + Moba_gameEnvironmentController.MissileShootCooldown;

            //     agent.MissileFired();

            //     // Add missile to missile controller
            //     Moba_gameEnvironmentController.MissileController.AddMissile(mc);
            //     if (agent.AgentType == "ice")
            //     {
            //         spawnPosition1 = agent.MissileSpawnPoint1.transform.position;
            //         localXDir = agent.MissileSpawnPoint1.transform.TransformDirection(Vector3.up);
            //         velocity = localXDir * Moba_gameEnvironmentController.MissleLaunchSpeed;
            //         obj = Instantiate(Moba_gameEnvironmentController.MissilePrefab, spawnPosition1, spawnRotation, transform);
            //         obj.layer = gameObject.layer;
            //         mc = obj.GetComponent<MissileComponent>();
            //         mc.Parent = agent;
            //         mc.MissileVelocity = velocity;
            //         mc.Moba_gameEnvironmentController = Moba_gameEnvironmentController;
            //         agent.NextShootTime = Moba_gameEnvironmentController.CurrentSimulationTime + Moba_gameEnvironmentController.MissileShootCooldown;

            //         agent.MissileFired();

            //         // Add missile to missile controller
            //         Moba_gameEnvironmentController.MissileController.AddMissile(mc);
            //     }
            // }
        }
    }
}
