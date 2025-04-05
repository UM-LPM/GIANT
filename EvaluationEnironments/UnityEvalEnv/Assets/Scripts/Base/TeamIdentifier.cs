using UnityEngine;

namespace Base
{
    public class TeamIdentifier : MonoBehaviour
    {
        public int TeamID { get; set; }

        private void Awake()
        {
            TeamID = -1; // Default value (No team)
        }
    }
}