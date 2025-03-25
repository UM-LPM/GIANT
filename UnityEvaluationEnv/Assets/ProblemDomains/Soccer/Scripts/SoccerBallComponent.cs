using UnityEngine;

namespace Problems.Soccer
{
    public class SoccerBallComponent : MonoBehaviour
    {
        public Rigidbody Rigidbody { get; set; }
        public Vector3 StartPosition { get; set; }
        public Quaternion StartRotation { get; set; }
        public SoccerAgentComponent LastTouchedAgent { get; set; }

        SoccerEnvironmentController SoccerEnvironmentController;

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            SoccerEnvironmentController = GetComponentInParent<SoccerEnvironmentController>();
            StartPosition = transform.position;
            StartRotation = transform.rotation;
        }

        private void OnCollisionEnter(Collision collision)
        {
            SoccerAgentComponent agent = collision.gameObject.GetComponent<SoccerAgentComponent>();
            if (agent != null)
            {
                LastTouchedAgent = agent;
            }

            GoalComponent goal = collision.gameObject.GetComponent<GoalComponent>();
            if (goal != null)
            {
                SoccerEnvironmentController.GoalScored(LastTouchedAgent, goal);
            }
        }
    }
}
