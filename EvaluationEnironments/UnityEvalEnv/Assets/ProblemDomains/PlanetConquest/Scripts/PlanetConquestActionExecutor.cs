using AgentControllers;
using Base;
using UnityEngine;
using Utils;

namespace Problems.PlanetConquest
{
    public class PlanetConquestActionExecutor : ActionExecutor
    {
        private PlanetConquestEnvironmentController PlanetConquestEnvironmentController;

        // Move Agent variables
        int forwardAxis = 0;
        int rotateAxis = 0;

        // Shoot laser variables
        Vector3 spawnPosition;
        Vector3 localXDir;

        // Temp global variables for optimization purpose
        Vector3 laserEnd;
        RaycastHit2D hit;

        private void Awake()
        {
            PlanetConquestEnvironmentController = GetComponentInParent<PlanetConquestEnvironmentController>();
        }

        public override void ExecuteActions(AgentComponent agent)
        {
            Move(agent as PlanetConquestAgentComponent);
            ShootLaser(agent as PlanetConquestAgentComponent);
        }

        void Move(PlanetConquestAgentComponent agent)
        {
            var thrust = PlanetConquestEnvironmentController.LavaAgentForwardThrust;
            var torque = PlanetConquestEnvironmentController.LavaAgentTourque;
            if (agent.AgentType == AgentType.Ice)
            {
                thrust = PlanetConquestEnvironmentController.IceAgentForwardThrust;
                torque = PlanetConquestEnvironmentController.IceAgentTourque;
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


        void ShootLaser(PlanetConquestAgentComponent agent)
        {
            if (agent.ActionBuffer.GetDiscreteAction("shootMissile") == 1 && agent.NextShootTime <= PlanetConquestEnvironmentController.CurrentSimulationTime)
            {
                ShootIndividualLaser(agent, agent.LaserSpawnPoint);

                agent.LaserFired(PlanetConquestEnvironmentController);

                agent.NextShootTime = PlanetConquestEnvironmentController.CurrentSimulationTime + PlanetConquestEnvironmentController.LaserShootCooldown;
            }
        }

        void ShootIndividualLaser(PlanetConquestAgentComponent agent, LaserSpawnPointComponent laserSpawnPoint)
        {
            spawnPosition = laserSpawnPoint.transform.position;
            localXDir = agent.LaserSpawnPoint.transform.TransformDirection(Vector3.up);
            laserEnd = spawnPosition + localXDir * PlanetConquestEnvironmentController.LaserRange;

            hit = PhysicsUtil.PhysicsRaycast2D(agent.gameObject, spawnPosition, localXDir, PlanetConquestEnvironmentController.LaserRange, true, agent.gameObject.layer, PlanetConquestEnvironmentController.DefaultLayer);

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

        PlanetConquestAgentComponent hitAgent;
        BaseComponent hitBase;

        void HandleHit(Collider2D collider, PlanetConquestAgentComponent agent)
        {
            collider.TryGetComponent(out hitAgent);

            if (hitAgent != null && hitAgent != agent)
            {
                PlanetConquestEnvironmentController.LaserSpaceShipHit(agent, hitAgent);
                return;
            }

            collider.TryGetComponent(out hitBase);
            if (hitBase != null && hitBase != agent)
            {
                PlanetConquestEnvironmentController.LaserBaseHit(agent, hitBase);
                return;
            }

            /*// TODO Solve this using TryGetComponent instead of tag checking
            if (collider.CompareTag("Agent"))
            {
                PlanetConquestAgentComponent hitAgent = collider.GetComponent<PlanetConquestAgentComponent>();
                PlanetConquestEnvironmentController.LaserSpaceShipHit(agent, hitAgent);
            }
            else if (collider.CompareTag("Object5"))
            {
                BaseComponent hitBase = collider.GetComponent<BaseComponent>();
                PlanetConquestEnvironmentController.LaserBaseHit(agent, hitBase);
            }*/
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