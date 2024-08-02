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
    private MissileComponent[] Missiles;
    private RobostrikeEnvironmentController RobostrikeEnvironmentController;

    private Collider2D[] MissileCollisions;

    void Start () {
        RobostrikeEnvironmentController = GetComponentInParent<RobostrikeEnvironmentController>();
    }


    void FixedUpdate () {
       FindAllMissiles();
       UpdateMissilePositions();
       //CheckMissileCollisions();
   }

    void FindAllMissiles() {
        Missiles = FindObjectsOfType<MissileComponent>();
        List<MissileComponent> missilesInLayer = new List<MissileComponent>();

        for (int i = 0; i < Missiles.Length; i++) {
            if (Missiles[i].gameObject.layer == gameObject.layer) {
                missilesInLayer.Add(Missiles[i]);
            }
        }
        Missiles = missilesInLayer.ToArray();
    }

    void UpdateMissilePositions()
    {
        for (int i = 0; i < Missiles.Length; i++)
        {
            // Update missile position and check if it's colliding with anything
            //Vector3 missileNewPos = Missiles[i].transform.position += Missiles[i].MissileVelocity * Time.fixedDeltaTime;
            Missiles[i].transform.position += Missiles[i].MissileVelocity * Time.fixedDeltaTime;
            CheckMissileCollision(Missiles[i]);
        }
    }

    void CheckMissileCollision(MissileComponent missileComponent)
    {
        MissileCollisions = Physics2D.OverlapCircleAll(missileComponent.transform.position, MissileRadius, LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer), LayerMask.LayerToName(0)));
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
                Destroy(missileComponent.gameObject);
            }
        }   
    }

    /*void CheckMissileCollisions() {
        RaycastHit[] hits;
        AgentComponent agent;
        for (int i = 0; i < Missiles.Length; i++) {
            hits = Physics.BoxCastAll(Missiles[i].transform.position, MissileSizeHalf, Missiles[i].transform.forward, Missiles[i].transform.rotation, 0f, LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer), LayerMask.LayerToName(0)));
            if (hits.Length > 0) {
                foreach (RaycastHit hit in hits) {
                    if (hit.transform.gameObject == Missiles[i].Parent.gameObject)
                        continue;

                    hit.transform.gameObject.TryGetComponent<AgentComponent>(out agent);
                    if (agent != null) {
                        // Tank was hit
                        //Debug.Log("Tank was hit: " + hit.transform.gameObject.name);

                        agent.Score -= 1;
                    }
                    //Debug.Log(hit.collider.gameObject.name);
                    Destroy(Missiles[i].gameObject);
                }
            }
        }
    }*/
}