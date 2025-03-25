using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.Soccer
{
    public class GoalComponent : MonoBehaviour
    {
        [SerializeField] public SoccerUtils.SoccerTeam Team;
        [HideInInspector] public int GoalsReceived { get; set; }
    }
}
