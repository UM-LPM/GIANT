using Base;
using UnityEngine;

namespace Problems.Soccer2D
{
    public class Soccer2DGoalComponent : MonoBehaviour
    {
        [SerializeField] public Soccer2DUtils.SoccerTeam Team;
        [HideInInspector] public int GoalsReceived { get; set; }

        public TeamIdentifier TeamIdentifier { get; private set; }

        private void Awake()
        {
            TeamIdentifier = GetComponent<TeamIdentifier>();
        }
    }
}
