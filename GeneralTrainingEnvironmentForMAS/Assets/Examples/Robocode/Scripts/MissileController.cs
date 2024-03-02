using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// This class is responsible for controlling the missiles in the Robocode environment, when no physics is used (used by default).
/// </summary>
public class MissileController : MonoBehaviour {

    [SerializeField] Vector3 MissileSizeHalf = new Vector3(0.1f, 0.1f, 0.1f);
    private MissileComponent[] Missiles;
    private RobocodeEnvironmentController RobocodeEnvironment;

    void Start () {
        RobocodeEnvironment = GetComponentInParent<RobocodeEnvironmentController>();
    }


    /*void FixedUpdate () {
       FindAllMissiles();
       CheckMissileCollisions();
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

    void CheckMissileCollisions() {
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