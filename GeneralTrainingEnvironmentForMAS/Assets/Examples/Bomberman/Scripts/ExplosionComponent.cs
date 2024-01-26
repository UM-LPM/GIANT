using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ExplosionComponent : MonoBehaviour {
    [SerializeField] public AnimatedSpriteRenderer Start;
    [SerializeField] public AnimatedSpriteRenderer Middle;
    [SerializeField] public AnimatedSpriteRenderer End;

    public AgentComponent Parent { get; set; }
    public BombermanEnvironmentController BombermanEnvironmentController { get; set; }

    public void SetActiveRenderer(AnimatedSpriteRenderer renderer) {
        Start.enabled = renderer == Start;
        Middle.enabled = renderer == Middle;
        End.enabled = renderer == End;
    }

    public void SetDirection(Vector2 direction) {
        float angle = Mathf.Atan2(direction.y, direction.x);
        transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
    }
}
