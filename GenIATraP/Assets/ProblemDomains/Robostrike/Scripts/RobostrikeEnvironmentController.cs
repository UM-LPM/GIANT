using UnityEngine;

namespace Problems.Robostrike
{
    public class RobostrikeEnvironmentController : EnvironmentControllerBase
    {
        [SerializeField] public static int MAX_HEALTH = 10;
        [SerializeField] public static int MAX_SHIELD = 10;
        [SerializeField] public static int MAX_AMMO = 20;

        [Header("Robostrike General Configuration")]
        [SerializeField] int AgentStartHealth = 10;
        [SerializeField] int AgentStartShield = 0;
        [SerializeField] int AgentStartAmmo = 0;
        [SerializeField] RobostrikeGameScenarioType GameScenarioType = RobostrikeGameScenarioType.Normal;
        [SerializeField] RobostrikeAgentRespawnType AgentRespawnType = RobostrikeAgentRespawnType.StartPos;

        [Header("Robostrike Movement Configuration")]
        [SerializeField] float AgentMoveSpeed = 5f;
        [SerializeField] float AgentRotationSpeed = 80f;
        [SerializeField] float AgentTurrentRotationSpeed = 90f;

        [Header("Robostrike Missile Configuration")]
        [SerializeField] GameObject MissilePrefab;
        [SerializeField, Tooltip("Destroy Missile After X seconds")] public float DestroyMissileAfter = 3.0f;
        [SerializeField] float MissileShootCooldown = 1.0f;
        [SerializeField] float MissleLaunchSpeed = 30f;
        [SerializeField] public static int MissileDamage = 2;

        [Header("Robostrike PowerUps Configuration")]
        [SerializeField] int HealthPowerUpValue = 5;
        [SerializeField] int ShieldPowerUpValue = 5;
        [SerializeField] int AmmoPowerUpValue = 10;

        [Header("Robostrike PowerUps Prefabs")]
        [SerializeField] public float MinPowerUpDistance = 8f;
        [SerializeField] public Vector3 PowerUpColliderExtendsMultiplier = new Vector3(0.505f, 0.495f, 0.505f);
        [SerializeField] GameObject HealthBoxPrefab;
        [SerializeField] int HealthBoxSpawnAmount = 2;
        [SerializeField] GameObject ShieldBoxPrefab;
        [SerializeField] int ShieldBoxSpawnAmount = 2;
        [SerializeField] GameObject AmmoBoxPrefab;
        [SerializeField] int AmmoBoxSpawnAmount = 2;

        // TODO Implement
    }

    public enum RobostrikeGameScenarioType
    {
        Normal,
        Deathmatch
    }

    public enum RobostrikeAgentRespawnType
    {
        StartPos,
        Random
    }

}