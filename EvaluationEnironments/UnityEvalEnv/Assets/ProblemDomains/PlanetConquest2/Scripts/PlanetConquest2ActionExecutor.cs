using AgentControllers;
using Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Problems.PlanetConquest2
{
    public class PlanetConquest2ActionExecutor : ActionExecutor
    {
        private PlanetConquest2EnvironmentController planetConquest2EnvironmentController;

        // Move Agent variables
        Vector3 dirToGo = Vector3.zero;
        Vector3 rotateDir = Vector3.zero;
        Vector3 rotateTurrentDir = Vector3.zero;

        int forwardAxis = 0;
        int rotateAxis = 0;

        Vector3 newAgentPos;
        Quaternion newAgentRotation;

        // Shoot laser variables
        Vector3 spawnPosition;
        Vector3 localXDir;

        // Temp global variables for optimization purpose
        Vector3 laserEnd;
        RaycastHit2D hit;

        PlanetConquest2AgentComponent hitAgent;
        BaseComponent hitBase;

        float agentSpeed = 0f;
        float rotationSpeed = 0f;

        private void Awake()
        {
            planetConquest2EnvironmentController = GetComponentInParent<PlanetConquest2EnvironmentController>();
        }

        public override void ExecuteActions(AgentComponent agent)
        {
            MoveAgent(agent as PlanetConquest2AgentComponent);
            ShootLaser(agent as PlanetConquest2AgentComponent);
        }

        private void MoveAgent(PlanetConquest2AgentComponent agent)
        {
            agentSpeed = planetConquest2EnvironmentController.LavaAgentForwardThrust;
            rotationSpeed = planetConquest2EnvironmentController.LavaAgentTourque;
            if(agent.AgentType == AgentType.Ice)
            {
                agentSpeed = planetConquest2EnvironmentController.IceAgentForwardThrust;
                rotationSpeed = planetConquest2EnvironmentController.IceAgentTourque;
            }

            dirToGo = Vector3.zero;
            rotateDir = Vector3.zero;
            rotateTurrentDir = Vector3.zero;

            forwardAxis = agent.ActionBuffer.GetDiscreteAction("moveForwardDirection");
            rotateAxis = agent.ActionBuffer.GetDiscreteAction("rotateDirection");

            switch (forwardAxis)
            {
                case 1:
                    dirToGo = agent.transform.up * planetConquest2EnvironmentController.ForwardSpeed;
                    break;
                case 2:
                    dirToGo = agent.transform.up * -planetConquest2EnvironmentController.ForwardSpeed;
                    break;
            }

            switch (rotateAxis)
            {
                case 1:
                    rotateDir.z = planetConquest2EnvironmentController.ForwardSpeed;
                    break;
                case 2:
                    rotateDir.z = -planetConquest2EnvironmentController.ForwardSpeed;
                    break;
            }

            newAgentPos = UnityUtils.RoundToDecimals(agent.transform.position + (dirToGo * Time.fixedDeltaTime * agentSpeed), 2);
            newAgentRotation = Quaternion.Euler(0, 0, agent.transform.rotation.eulerAngles.z + UnityUtils.RoundToDecimals(rotateDir.z * Time.fixedDeltaTime * rotationSpeed, 2));

            // Check if agent can be moved and rotated without colliding to other objects
            if (!PhysicsUtil.PhysicsOverlapObject(planetConquest2EnvironmentController.GameType, agent.gameObject, newAgentPos, planetConquest2EnvironmentController.AgentColliderExtendsMultiplier.x, Vector3.zero, newAgentRotation, PhysicsOverlapType.OverlapSphere, true, gameObject.layer, planetConquest2EnvironmentController.DefaultLayer))
            {
                agent.transform.position = newAgentPos;
                agent.transform.rotation = newAgentRotation;
            }
        }

        private void ShootLaser(PlanetConquest2AgentComponent agent)
        {
            if (agent.ActionBuffer.GetDiscreteAction("shootMissile") == 1 && agent.NextShootTime <= planetConquest2EnvironmentController.CurrentSimulationTime)
            {
                ShootIndividualLaser(agent, agent.LaserSpawnPoint);

                agent.LaserFired(planetConquest2EnvironmentController);

                agent.NextShootTime = planetConquest2EnvironmentController.CurrentSimulationTime + planetConquest2EnvironmentController.LaserShootCooldown;
            }
        }

        void ShootIndividualLaser(PlanetConquest2AgentComponent agent, LaserSpawnPointComponent laserSpawnPoint)
        {
            spawnPosition = laserSpawnPoint.transform.position;
            localXDir = agent.LaserSpawnPoint.transform.TransformDirection(Vector3.up);
            laserEnd = spawnPosition + localXDir * planetConquest2EnvironmentController.LaserRange;

            hit = PhysicsUtil.PhysicsRaycast2D(agent.gameObject, spawnPosition, localXDir, planetConquest2EnvironmentController.LaserRange, true, agent.gameObject.layer, planetConquest2EnvironmentController.DefaultLayer);

            if (hit.collider != null && agent != hit.collider.gameObject)
            {
                laserEnd = hit.point;
                HandleHit(hit.collider, agent);
            }

            if (agent.LineRenderer != null)
            {
                StartCoroutine(ShowLaser(spawnPosition, laserEnd, agent.LineRenderer));
            }
        }

        void HandleHit(Collider2D collider, PlanetConquest2AgentComponent agent)
        {
            collider.TryGetComponent(out hitAgent);

            if (hitAgent != null && hitAgent != agent)
            {
                planetConquest2EnvironmentController.LaserSpaceShipHit(agent, hitAgent);
                return;
            }

            collider.TryGetComponent(out hitBase);
            if (hitBase != null && hitBase != agent)
            {
                planetConquest2EnvironmentController.LaserBaseHit(agent, hitBase);
                return;
            }
        }

        System.Collections.IEnumerator ShowLaser(Vector3 spawnPosition, Vector3 endPoint, LineRenderer lineRenderer)
        {
            lineRenderer.SetPosition(0, spawnPosition);
            lineRenderer.SetPosition(1, endPoint);
            lineRenderer.enabled = true;
            yield return new WaitForSeconds(0.05f);
            lineRenderer.enabled = false;
        }
    }
}
