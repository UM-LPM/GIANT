using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// This class is responsible for controlling the missiles in the Robostrike environment, when no physics is used (used by default).
/// </summary>
public class MissileController : MonoBehaviour {

    [SerializeField] Vector3 MissileSizeHalf = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField] float MissileRadius = 0.225f;
    private List<MissileComponent> Missiles;
    private RobostrikeEnvironmentController RobostrikeEnvironmentController;

    private Collider2D[] MissileCollisions;

    void Start () {
        RobostrikeEnvironmentController = gameObject.GetComponent<RobostrikeEnvironmentController>();
        Missiles = new List<MissileComponent>();
    }


    void FixedUpdate () {
       UpdateMissilePositions();
       //CheckMissileCollisions();
   }

    void UpdateMissilePositions()
    {
        for (int i = 0; i < Missiles.Count; i++)
        {
            // Update missile position and check if it's colliding with anything
            //Vector3 missileNewPos = Missiles[i].transform.position += Missiles[i].MissileVelocity * Time.fixedDeltaTime;
            //Missiles[i].transform.position += Missiles[i].MissileVelocity * Time.fixedDeltaTime;
            Missiles[i].transform.position = UnityUtils.RoundToDecimals(Missiles[i].transform.position + Missiles[i].MissileVelocity * Time.fixedDeltaTime,2);

            CheckMissileCollision(Missiles[i]);
        }
    }

    void CheckMissileCollision(MissileComponent missileComponent)
    {
        MissileCollisions = Physics2D.OverlapCircleAll(missileComponent.transform.position, MissileRadius, LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer)) + RobostrikeEnvironmentController.DefaultLayer);
        if (MissileCollisions.Length > 0)
        {
            foreach (Collider2D collision in MissileCollisions)
            {
                if (collision.gameObject == missileComponent.Parent.gameObject || collision.gameObject == missileComponent.gameObject || collision.isTrigger)
                    continue;

                AgentComponent otherAgent;
                collision.gameObject.TryGetComponent<AgentComponent>(out otherAgent);

                if (otherAgent != null)
                {
                    RobostrikeEnvironmentController.TankHit(missileComponent, otherAgent);
                }
                else
                {
                    RobostrikeEnvironmentController.ObstacleMissedAgent(missileComponent);
                }

                missileComponent.MissileHitTarget = true;
                RemoveMissile(missileComponent);
                Destroy(missileComponent.gameObject);
            }
        }   
    }

    public void AddMissile(MissileComponent missile)
    {
        Missiles.Add(missile);
    }

    public void RemoveMissile(MissileComponent missile)
    {
        Missiles.Remove(missile);
    }

}