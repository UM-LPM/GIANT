using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RobocodeAgentComponent : AgentComponent {

    public Rigidbody Rigidbody { get; set; }
    public TurretComponent Turret { get; set; }
    public MissileSpawnPointComponent MissileSpawnPoint { get; set; }
    public float NextShootTime { get; set; }
    public float Health { get; set; }

    protected override void DefineAdditionalDataOnAwake() {
        Rigidbody = GetComponent<Rigidbody>();
        this.MissileSpawnPoint = GetComponentInChildren<MissileSpawnPointComponent>();
        this.Turret = GetComponentInChildren<TurretComponent>();
    }
}