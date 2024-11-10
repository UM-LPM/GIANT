using AgentControllers;
using AgentOrganizations;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using Spawners;
using AgentControllers.AIAgentControllers;
using System.Linq;
using Base;
using Problems.Dummy;

namespace Problems.Robostrike
{
    public class Robostrike1vs1MatchSpawner : MatchSpawner
    {
        [Header("Robostrike 1vs1 Match Configuration")]
        [SerializeField] public Transform[] SpawnPoints;

        [Header("Robostrike 1vs1 Agent Sprites")]
        [SerializeField] public Sprite[] Hulls;
        [SerializeField] public Sprite[] Turrets;
        [SerializeField] public Sprite[] Tracks;
        [SerializeField] public Sprite[] Guns;

        // Respawn variables
        Vector3 respawnPos = Vector3.zero;
        Quaternion rotation = Quaternion.identity;

        List<Vector3> agentPositions;

        bool isFarEnough;
        int counter = 0;
        int maxSpawnPoints = 100;

        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            if (Hulls == null || Hulls.Length == 0 || Turrets == null || Turrets.Length == 0 || Tracks == null || Tracks.Length == 0 || Guns == null || Guns.Length == 0)
            {
                throw new System.Exception("Sprites are missing");
                // TODO add error reporting here
            }

            if (environmentController.AgentPrefab == null)
            {
                throw new System.Exception("AgentPrefab is not defined");
                // TODO add error reporting here
            }

            if (environmentController.Match == null || environmentController.Match.Teams == null)
            {
                throw new System.Exception("Match is not defined");
                // TODO add error reporting here
            }

            if (environmentController.Match.Teams.Length != 2)
            {
                throw new System.Exception("Match should have 2 teams");
                // TODO add error reporting here
            }

            if (environmentController.Match.Teams[0].Individuals.Length != 1 || environmentController.Match.Teams[1].Individuals.Length != 1)
            {
                throw new System.Exception("Each team should have 1 individual");
                // TODO add error reporting here
            }

            if (environmentController.Match.Teams[0].Individuals[0].AgentControllers.Length != 1 || environmentController.Match.Teams[1].Individuals[0].AgentControllers.Length != 1)
            {
                throw new System.Exception("Each individual should have 1 agent controller");
                // TODO add error reporting here
            }

            if (SpawnPoints == null || SpawnPoints.Length != 2)
            {
                throw new System.Exception("Spawn points are not defined");
                // TODO add error reporting here
            }
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> agents = new List<T>();

            // Spawn agents
            for (int i = 0; i < environmentController.Match.Teams.Length; i++)
            {
                foreach(Individual individual in environmentController.Match.Teams[i].Individuals)
                {
                    foreach(AgentController agentController in individual.AgentControllers)
                    {
                        // Instantiate and configure agent
                        GameObject agentGameObject = Instantiate(environmentController.AgentPrefab, SpawnPoints[i].position, SpawnPoints[i].rotation, gameObject.transform);

                        // Configure agent
                        T agent = agentGameObject.GetComponent<T>();
                        AgentComponent agentComponent = agent as AgentComponent;
                        agentComponent.AgentController = agentController.Clone(); // Clone the agent controller to prevent shared state between agents
                        agentComponent.IndividualID = individual.IndividualId;
                        agentComponent.TeamID = environmentController.Match.Teams[i].TeamId;

                        // Configure agent sprites
                        ConfigureAgentSprites(environmentController, agentGameObject);

                        // Update list
                        agents.Add(agent);
                    }
                }
            }

            return agents.ToArray();
        }

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent)
        {
            if(!(respawnComponent is AgentComponent))
            {
                throw new System.Exception("Invalid respawn component");
                // TODO add error reporting here
            }

            RobostrikeEnvironmentController robostrikeEnvironmentController = environmentController as RobostrikeEnvironmentController;
            AgentComponent agent = respawnComponent as AgentComponent;

            respawnPos = Vector3.zero;
            rotation = Quaternion.identity;

            if (robostrikeEnvironmentController.AgentRespawnType == RobostrikeAgentRespawnType.StartPos)
            {
                respawnPos = agent.StartPosition;
                rotation = agent.StartRotation;
            }
            else if(robostrikeEnvironmentController.AgentRespawnType == RobostrikeAgentRespawnType.RandomPos)
            {
                GetRandomSpawnPositionAndRotation(robostrikeEnvironmentController, out respawnPos, out rotation);
            }
            else
            {
                throw new System.Exception("Invalid respawn type");
                // TODO add error reporting here
            }

            agent.transform.position = respawnPos;
            agent.transform.rotation = rotation;
        }

        void ConfigureAgentSprites(EnvironmentControllerBase environmentController, GameObject agentGameObject)
        {
            // Get random hull, turret, track and gun sprites
            Sprite hull = Hulls[environmentController.Util.NextInt(0, Hulls.Length)];
            Sprite turret = Turrets[environmentController.Util.NextInt(0, Turrets.Length)];
            Sprite track = Tracks[environmentController.Util.NextInt(0, Tracks.Length)];
            Sprite gun = Guns[environmentController.Util.NextInt(0, Guns.Length)];

            GameObject hullGO = agentGameObject.GetComponentInChildren<HullComponent>().gameObject;
            GameObject turretGO = agentGameObject.GetComponentInChildren<TurretComponent>().gameObject;
            GameObject[] tracksGO = agentGameObject.GetComponentsInChildren<TrackComponent>().Select(x=> x.gameObject).ToArray();
            GameObject gunGO = agentGameObject.GetComponentInChildren<GunComponent>().gameObject;

            // Set sprites

            hullGO.GetComponent<SpriteRenderer>().sprite = hull;
            turretGO.GetComponent<SpriteRenderer>().sprite = turret;
            foreach (GameObject trackGO in tracksGO)
            {
                trackGO.GetComponent<SpriteRenderer>().sprite = track;
            }
            gunGO.GetComponent<SpriteRenderer>().sprite = gun;
        }
    
        void GetRandomSpawnPositionAndRotation(RobostrikeEnvironmentController environmentController, out Vector3 spawnPos, out Quaternion rotation)
        {
            counter = 0;
            agentPositions = environmentController.Agents.Select(agent => agent.transform.position).ToList();
            do
            {
                isFarEnough = true;
                spawnPos = GetRandomSpawnPoint(
                                environmentController.Util,
                                environmentController.GameType,
                                environmentController.ArenaSize,
                                environmentController.ArenaRadius,
                                environmentController.ArenaCenterPoint,
                                environmentController.ArenaOffset);
                if (environmentController.SceneLoadMode == SceneLoadMode.GridMode)
                    spawnPos += environmentController.GridCell.GridCellPosition;

                rotation = GetRandomRotation(environmentController.Util, environmentController.GameType);

                if (!SpawnPointSuitable(environmentController.GameType,
                            spawnPos,
                            rotation,
                            agentPositions,
                            environmentController.AgentColliderExtendsMultiplier,
                            environmentController.MinAgentDistance,
                            true,
                            environmentController.gameObject.layer,
                            environmentController.DefaultLayer))
                {
                    isFarEnough = false;
                }

                // Check if current spawn point is far enough from the agents
                foreach (AgentComponent agent in environmentController.Agents)
                {
                    if (Vector3.Distance(agent.transform.position, spawnPos) < environmentController.MinPowerUpDistanceFromAgents)
                    {
                        isFarEnough = false;
                        break;
                    }
                }

                if (counter >= maxSpawnPoints)
                    break;

                counter++;
            } while (!isFarEnough);
        }
    }
}