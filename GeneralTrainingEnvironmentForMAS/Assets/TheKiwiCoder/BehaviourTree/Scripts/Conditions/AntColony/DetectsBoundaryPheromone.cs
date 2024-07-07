using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class DetectBoundaryPheromone : ConditionNode
{
    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }
    protected override bool CheckConditions()
    {
        return DetectPheromone(context.transform.position, PheromoneType.Boundary, context.gameObject.GetComponent<AntAgentComponent>().detectionRadius);
    }

    private bool DetectPheromone(Vector3 position, PheromoneType type, float detectionRadius)
    {
        RaySensor2D raySensor = context.gameObject.GetComponent<RaySensor2D>();

        if (raySensor != null)
        {
            RayPerceptionInput perceptionInput = new RayPerceptionInput
            {
                RayLength = detectionRadius,
                DetectableTags = new List<string> { type.ToString() },
                Angles = new List<float> { 0 },
                CastRadius = 0.5f,
                Transform = context.transform,
                CastType = RayPerceptionCastType.Cast2D,
                LayerMask = LayerMask.GetMask("Pheromones")
            };

            SensorPerceiveOutput[] perceptionOutputs = raySensor.Perceive();

            foreach (var output in perceptionOutputs)
            {
                if (output.HasHit && output.HitGameObjects != null)
                {
                    foreach (var hitObject in output.HitGameObjects)
                    {
                        if (hitObject != null && hitObject.CompareTag(type.ToString()))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
}
