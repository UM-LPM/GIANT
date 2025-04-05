using AgentControllers;
using Base;
using Problems.Robostrike;
using UnityEngine;

namespace Problems.PlanetConquest
{
    public class PlanetConquestActionExecutor : ActionExecutor
    {
        private PlanetConquestEnvironmentController PlanetConquestEnvironmentController;

        // Move Agent variables
        int forwardAxis = 0;
        int rotateAxis = 0;

        // Shoot missile variables
        Vector3 spawnPosition;
        Vector3 spawnPosition1;
        Vector3 localXDir;

        public float laserRange = 300f;


        // Temp global variables for optimization purpose
        Vector3 laserEnd;
        RaycastHit2D[] hits;

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
                ShootIndividualLaser(agent, agent.MissileSpawnPoint);

                if (agent.AgentType == AgentType.Ice)
                {
                    ShootIndividualLaser(agent, agent.MissileSpawnPoint1);
                }

                agent.LaserFired(PlanetConquestEnvironmentController);

                agent.NextShootTime = PlanetConquestEnvironmentController.CurrentSimulationTime + PlanetConquestEnvironmentController.LaserShootCooldown;
            }
        }

        void ShootIndividualLaser(PlanetConquestAgentComponent agent, MissileSpawnPointComponent missileSpawnPoint)
        {
            spawnPosition = missileSpawnPoint.transform.position;
            localXDir = agent.MissileSpawnPoint.transform.TransformDirection(Vector3.up);
            laserEnd = spawnPosition + localXDir * laserRange;
            hits = Physics2D.RaycastAll(spawnPosition, localXDir, laserRange);
            foreach (var hit in hits)
            {
                if (!hit.collider.isTrigger)
                {
                    laserEnd = hit.point;
                    HandleHit(hit.collider, agent);
                    break;
                }
            }

            if (agent.LineRenderer != null)
            {
                StartCoroutine(ShowLaser(spawnPosition, laserEnd, agent.LineRenderer));
            }
        }

        void HandleHit(Collider2D collider, PlanetConquestAgentComponent agent)
        {
            // TODO Solve this using TryGetComponent instead of tag checking
            if (collider.CompareTag("Agent"))
            {
                PlanetConquestAgentComponent hitAgent = collider.GetComponent<PlanetConquestAgentComponent>();
                PlanetConquestEnvironmentController.LaserSpaceShipHit(agent, hitAgent);
            }
            else if (collider.CompareTag("Object5"))
            {
                BaseComponent hitBase = collider.GetComponent<BaseComponent>();
                PlanetConquestEnvironmentController.LaserBaseHit(agent, hitBase);
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
    }
}