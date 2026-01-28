using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

namespace Problems.Mario
{
    public abstract class EnemyComponent: MonoBehaviour
    {
        protected MarioEnvironmentController MarioEnvironmentController;
        protected BoxCollider2D BoxCollider2D;
        protected Vector2 moveDir = Vector2.left;

        private void Awake()
        {
            MarioEnvironmentController = gameObject.transform.parent.GetComponentInParent<MarioEnvironmentController>();
            BoxCollider2D = GetComponent<BoxCollider2D>();
        }

        public abstract void UpdateEnemy();
    }
}
