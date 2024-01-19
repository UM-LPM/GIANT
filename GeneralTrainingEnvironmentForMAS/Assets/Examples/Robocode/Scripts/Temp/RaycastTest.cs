using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RaycastTest: MonoBehaviour {

    private RayHitObject rayHitObject;

    void Start() {
        rayHitObject = new RayHitObject() { targetGameObject = TargetGameObject.Agent, side = AgentSideAdvanced.Center};
        rayHitObject.context = new TheKiwiCoder.Context();
        rayHitObject.context.gameObject = gameObject;
    }

    private void FixedUpdate() {
        if (rayHitObject.Check())
            Debug.Log("Ray hit target");
        else
            Debug.Log("Target not hit");
    }
}