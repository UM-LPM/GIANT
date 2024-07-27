using Collector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class RobostrikeAgentComponent : AgentComponent {

    public Rigidbody Rigidbody { get; set; }
    public TurretComponent Turret { get; set; }
    public MissileSpawnPointComponent MissileSpawnPoint { get; set; }
    public float NextShootTime { get; set; }
    
    /*public float Health { get; set; }
    public float Shield { get; set; }
    public int Ammo { get; set; }*/

    public HealthComponent HealthComponent { get; set; }
    public ShieldComponent ShieldComponent { get; set; }
    public AmmoComponent AmmoComponent { get; set; }

    private AgentStatBars StatBars;

    protected override void DefineAdditionalDataOnAwake() {
        Rigidbody = GetComponent<Rigidbody>();
        this.MissileSpawnPoint = GetComponentInChildren<MissileSpawnPointComponent>();
        this.Turret = GetComponentInChildren<TurretComponent>();

        StatBars = GetComponent<AgentStatBars>();

        HealthComponent = GetComponent<HealthComponent>();
        ShieldComponent = GetComponent<ShieldComponent>();
        AmmoComponent = GetComponent<AmmoComponent>();
    }

    protected override void DefineAdditionalDataOnStart()
    {
        UpdatetStatBars();
    }

    private void OnTriggerEnter(Collider other)
    {
        SectorComponent sectorComponent = other.gameObject.GetComponent<SectorComponent>();
        // New Sector Explored
        if (sectorComponent != null && AgentExploredNewSector(sectorComponent))
        {
            //Debug.Log("New Sector Explored"); // TODO Remove
            if (RobostrikeFitness.FitnessValues[RobostrikeFitness.Keys[(int)RobostrikeFitness.FitnessKeys.AgentExploredSector]] != 0)
            {
                AgentFitness.Fitness.UpdateFitness((RobostrikeFitness.FitnessValues[RobostrikeFitness.Keys[(int)RobostrikeFitness.FitnessKeys.AgentExploredSector]]), RobostrikeFitness.FitnessKeys.AgentExploredSector.ToString());
            }

            // Add explored sector to the list of explored sectors
            LastKnownPositions.Add(sectorComponent.transform.position);
            return;
        }
        // Re-explored Sector
        else if (sectorComponent != null && !AgentExploredNewSector(sectorComponent))
        {
            if (RobostrikeFitness.FitnessValues[RobostrikeFitness.Keys[(int)RobostrikeFitness.FitnessKeys.AgentReExploredSector]] != 0)
            {
                AgentFitness.Fitness.UpdateFitness((RobostrikeFitness.FitnessValues[RobostrikeFitness.Keys[(int)RobostrikeFitness.FitnessKeys.AgentReExploredSector]]), RobostrikeFitness.FitnessKeys.AgentReExploredSector.ToString());
            }
        }
    }

    private bool AgentExploredNewSector(SectorComponent sectorComponent)
    {
        return !LastKnownPositions.Contains(sectorComponent.transform.position);
    }

    public bool SetHealth(int value)
    {
        if(this.HealthComponent.Health + value <= RobostrikeEnvironmentController.MAX_HEALTH)
        {
            this.HealthComponent.Health += value;
            return true;
        }
        else if(this.HealthComponent.Health < RobostrikeEnvironmentController.MAX_HEALTH && this.HealthComponent.Health + value > RobostrikeEnvironmentController.MAX_HEALTH)
        {
            this.HealthComponent.Health = RobostrikeEnvironmentController.MAX_HEALTH;
            return true;
        }
        else {
            return false;
        }
    }

    public bool SetShield(int value)
    {
        if(this.ShieldComponent.Shield + value <= RobostrikeEnvironmentController.MAX_SHIELD)
        {
            this.ShieldComponent.Shield += value;
            return true;
        }
        else if(this.ShieldComponent.Shield < RobostrikeEnvironmentController.MAX_SHIELD && this.ShieldComponent.Shield + value > RobostrikeEnvironmentController.MAX_SHIELD)
        {
            this.ShieldComponent.Shield = RobostrikeEnvironmentController.MAX_SHIELD;
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool SetAmmo(int value)
    {
        if(this.AmmoComponent.Ammo + value <= RobostrikeEnvironmentController.MAX_AMMO)
        {
            this.AmmoComponent.Ammo += value;
            return true;
        }
        else if(this.AmmoComponent.Ammo < RobostrikeEnvironmentController.MAX_AMMO && this.AmmoComponent.Ammo + value > RobostrikeEnvironmentController.MAX_AMMO)
        {
            this.AmmoComponent.Ammo = RobostrikeEnvironmentController.MAX_AMMO;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void MissileFired()
    {
        if(AmmoComponent.Ammo > 0)
        {
            AmmoComponent.Ammo--;
            UpdatetStatBars();
        }
    }

    public void TakeDamage(int value)
    {
        if (ShieldComponent.Shield > 0)
        {
            if (ShieldComponent.Shield - (value * 2) < 0)
            {
                ShieldComponent.Shield = 0;
            }
            else
            {
                ShieldComponent.Shield -= value;
            }
            HealthComponent.Health -= value * 0.5f;
        }
        else
        {
            HealthComponent.Health -= value;
        }

        // Update Stat Bars 
        UpdatetStatBars();
    }

    public void UpdatetStatBars()
    {
        if (StatBars != null)
            StatBars.SetStats(HealthComponent.Health, ShieldComponent.Shield, AmmoComponent.Ammo);
    }
}