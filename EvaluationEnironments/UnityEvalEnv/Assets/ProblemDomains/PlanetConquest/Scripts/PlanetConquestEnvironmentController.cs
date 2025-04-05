using Base;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.PlanetConquest {
    public class PlanetConquestEnvironmentController : EnvironmentControllerBase
    {
        [SerializeField] public static int MAX_BASE_HEALTH = 20;
        [SerializeField] public static int MAX_HEALTH = 10;
        [SerializeField] public static int MAX_ENERGY = 30;

        [Header("Moba_game General Configuration")]
        [SerializeField] int AgentStartHealth = 10;
        [SerializeField] int AgentStartEnergy = 30;
        [SerializeField] int BaseStartHealth = 20;
        [SerializeField] public int LaserEnergyConsumption = 5;
        [SerializeField] public int LaserHitEnergyBonus = 10;
        [SerializeField] public bool UnlimitedEnergy = false;
        [SerializeField] public PlanetConquestAgentRespawnType AgentRespawnType = PlanetConquestAgentRespawnType.StartPos;
        [SerializeField] public PlanetConquestGameScenarioType GameScenarioType = PlanetConquestGameScenarioType.Normal;
        [SerializeField] public Boolean FrienlyFire = false;
        [SerializeField] private int LavaAgentCost = 5;
        [SerializeField] private int IceAgentCost = 5;
        private PlanetConquest1vs1MatchSpawner Match1v1Spawner;


        [Header("Moba_game Base Configuration")]
        [SerializeField] public GameObject BasePrefab;
        private List<BaseComponent> Bases;

        [Header("Moba_game Planets Configuration")]

        [SerializeField] public GameObject LavaPlanetPrefab;
        [SerializeField] public int LavaPlanetSpawnAmount = 3;
        [SerializeField] public GameObject IcePlanetPrefab;
        [SerializeField] public int IcePlanetSpawnAmount = 3;
        private PlanetSpawner PlanetSpawner;
        private List<PlanetComponent> Planets;

        [Header("Moba_game Movement Configuration")]
        [SerializeField] public float AgentMoveSpeed = 5f;
        [SerializeField] public float AgentRotationSpeed = 80f;
        [SerializeField] public float AgentTurrentRotationSpeed = 90f;
        [SerializeField] public float LavaAgentForwardThrust = 5f;
        [SerializeField] public float LavaAgentTourque = 1f;
        [SerializeField] public float IceAgentForwardThrust = 2.5f;
        [SerializeField] public float IceAgentTourque = 0.5f;

        [Header("Moba_game Laser Configuration")]
        [SerializeField] public float LaserShootCooldown = 1.0f;
        [SerializeField] public static int LaserDamage = 2;


        [Header("Moba_game Planets Configuration")]
        [SerializeField] public float MinPowerUpDistance = 8f;
        [SerializeField] public float MinPowerUpDistanceFromAgents = 8f;
        [SerializeField] public Vector3 PowerUpColliderExtendsMultiplier = new Vector3(0.505f, 0.495f, 0.505f);


        public void LaserSpaceShipHit(PlanetConquestAgentComponent agent, PlanetConquestAgentComponent hitAgent)
        {
            throw new NotImplementedException();
        }

        public void LaserBaseHit(PlanetConquestAgentComponent agent, BaseComponent hitBase)
        {
            throw new NotImplementedException();
        }
    }

    public enum PlanetConquestGameScenarioType
    {
        Normal,
        Deathmatch
    }

    public enum PlanetConquestAgentRespawnType
    {
        StartPos,
        RandomPos
    }
}
