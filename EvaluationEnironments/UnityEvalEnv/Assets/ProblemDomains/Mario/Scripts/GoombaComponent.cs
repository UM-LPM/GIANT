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
    public class GoombaComponent: EnemyComponent
    {

        public override void UpdateEnemy()
        {
            // Move the Goomba in the current direction (if enemy hits a wall, reverse direction)
            var move = (moveDir * MarioEnvironmentController.EnemyPatrolSpeed * Time.fixedDeltaTime);
            float distance = Mathf.Abs(move.x);

            var results = PhysicsUtil.BoxCast2D(
                    MarioEnvironmentController.PhysicsScene2D,
                    gameObject,
                    BoxCollider2D.bounds.center,
                    BoxCollider2D.bounds.size / 2f,
                    0f,
                    moveDir,
                    distance,
                    true,
                    gameObject.layer
            );

            bool hasHit = false;
            foreach (var result in results)
            {
                var agent = result.collider.gameObject.GetComponent <MarioAgentComponent>();
                if (agent != null)
                {
                    MarioEnvironmentController.OnEnemyKilledAgent(agent, this);
                }
                
                if (result.collider != null && result.collider.gameObject != gameObject)
                {
                    hasHit = true;
                }
            }

            if (hasHit)
            {
                moveDir = -moveDir; // Reverse direction
            }
            else
            {
                transform.position += new Vector3(move.x, 0, 0);
            }
        }

    }
}
