using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BombermanAgentComponent : AgentComponent {

    [Header("Movement controlls")]
    [SerializeField] public KeyCode InputUp = KeyCode.W;
    [SerializeField] public KeyCode InputDown = KeyCode.S;
    [SerializeField] public KeyCode InputLeft = KeyCode.A;
    [SerializeField] public KeyCode InputRight = KeyCode.D;

    [Header("Animation controlls")]
    [SerializeField] public AnimatedSpriteRenderer SpriteRendererUp;
    [SerializeField] public AnimatedSpriteRenderer SpriteRendererDown;
    [SerializeField] public AnimatedSpriteRenderer SpriteRendererLeft;
    [SerializeField] public AnimatedSpriteRenderer SpriteRendererRight;

    [SerializeField] public AnimatedSpriteRenderer SpriteRendererDeath;

    [Header("Bomb")]
    [SerializeField] public KeyCode BombKey = KeyCode.Space;

    public BombermanEnvironmentController BombermanEnvironmentController{ get; set; }

    public float MoveSpeed { get; set; }
    public int MaxAgentBombAmout { get; set; }

    public int ExplosionRadius { get; set; }

    public int BombsRemaining { get; set; }

    public int Health { get; set; }
    public float NextDamageTime { get; set; } // Required to prevent constant damage from explosions

    public AnimatedSpriteRenderer ActiveSpriteRenderer { get; set; }

    public Vector2 MoveDirection { get; private set; }
    public float NextAgentUpdateTime { get; set; }

    protected override void DefineAdditionalDataOnAwake() {
        ActiveSpriteRenderer = SpriteRendererDown;
        MoveDirection = Vector2.zero;
    }

    public void SetDirection(Vector2 newDirection, AnimatedSpriteRenderer spriteRenderer) {
        MoveDirection = newDirection;

        SpriteRendererUp.enabled = spriteRenderer == SpriteRendererUp;
        SpriteRendererDown.enabled = spriteRenderer == SpriteRendererDown;
        SpriteRendererLeft.enabled = spriteRenderer == SpriteRendererLeft;
        SpriteRendererRight.enabled = spriteRenderer == SpriteRendererRight;

        ActiveSpriteRenderer = spriteRenderer;
        ActiveSpriteRenderer.Idle = MoveDirection == Vector2.zero;
    }


    /*private void OnTriggerEnter2D(Collider2D collision) {
        ExplosionComponent explosion;
        
        if(collision.gameObject.TryGetComponent<ExplosionComponent>(out explosion)) {
            BombermanEnvironmentController.ExlosionHitAgent(this, explosion);
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        BombComponent bombComponent;
        other.gameObject.TryGetComponent<BombComponent>(out bombComponent);
        if (bombComponent) {
            other.isTrigger = false;
        }
    }*/

    public void DeathSequence() {
        this.enabled = false;

        SpriteRendererUp.enabled = false;
        SpriteRendererDown.enabled = false;
        SpriteRendererLeft.enabled = false;
        SpriteRendererRight.enabled = false;

        SpriteRendererDeath.enabled = true;

        Invoke(nameof(OnDeathSequnceEnded), 0.5f);
    }

    void OnDeathSequnceEnded() {
        gameObject.SetActive(false);
        BombermanEnvironmentController.CheckEndingState();
    }

    public void AddBomb() {
        MaxAgentBombAmout++;
        BombsRemaining++;
    }

    public void SetBombs(int Bombs) {
        MaxAgentBombAmout = Bombs;
        BombsRemaining = MaxAgentBombAmout;
    }

}
