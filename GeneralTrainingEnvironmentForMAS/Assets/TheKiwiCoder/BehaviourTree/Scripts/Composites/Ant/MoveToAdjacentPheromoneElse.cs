using UnityEngine;

namespace TheKiwiCoder
{
    public class MoveToAdjacentPheromoneElse : Sequencer
    {

        protected override void OnStart()
        {

            RayHitObject rayHitObject = ScriptableObject.CreateInstance<RayHitObject>();
            //Need 2D definition, the current one is 3D...
            //rayHitObject.targetGameObject = TargetGameObject2D.Pheromone;
            rayHitObject.side = AgentSideAdvanced.Center;
            children.Add(rayHitObject);

            MoveForward moveForward = ScriptableObject.CreateInstance<MoveForward>();
            children.Add(moveForward);

            Rotate rotate = ScriptableObject.CreateInstance<Rotate>();
            rotate.rotateDirection = RotateDirection.Random;
            children.Add(rotate);
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            return base.OnUpdate();
        }
    }
}
