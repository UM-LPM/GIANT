using UnityEngine;

namespace Problems.Dummy
{
    public class DummyEnvironmentController : EnvironmentControllerBase
    {
        [Header("Dummy configuration Movement")]
        [SerializeField] public float AgentMoveSpeed = 5f;
        [SerializeField] public float AgentRotationSpeed = 80f;

        [HideInInspector] public float ForwardSpeed = 1f;

        public override void UpdateAgents(bool updateBTs)
        {
            throw new System.NotImplementedException();
        }
    }
}