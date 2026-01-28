using System;
using UnityEngine;

namespace Problems.Mario
{
    public class FinishComponent : MonoBehaviour
    {
        public BoxCollider2D BoxCollider2D;

        private void Awake()
        {
            BoxCollider2D = GetComponent<BoxCollider2D>();
        }
    }
}
