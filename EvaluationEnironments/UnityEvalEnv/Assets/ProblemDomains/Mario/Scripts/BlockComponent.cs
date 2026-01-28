using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Problems.Mario
{
    public abstract class BlockComponent: MonoBehaviour
    {
        protected MarioEnvironmentController MarioEnvironmentController;
        protected SpriteRenderer SpriteRenderer;
        protected bool IsHit = false;

        private void Awake()
        {
            MarioEnvironmentController = GetComponentInParent<MarioEnvironmentController>();
            SpriteRenderer = GetComponent<SpriteRenderer>();
        }

        public abstract void OnHit(MarioAgentComponent agent);
    }
}
