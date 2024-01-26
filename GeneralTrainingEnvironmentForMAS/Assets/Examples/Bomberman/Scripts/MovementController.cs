using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField] public float Speed = 5f;
    [SerializeField] KeyCode InputUp = KeyCode.W;
    [SerializeField] KeyCode InputDown = KeyCode.S;
    [SerializeField] KeyCode InputLeft = KeyCode.A;
    [SerializeField] KeyCode InputRight = KeyCode.D;

    [SerializeField] AnimatedSpriteRenderer SpriteRendererUp;
    [SerializeField] AnimatedSpriteRenderer SpriteRendererDown;
    [SerializeField] AnimatedSpriteRenderer SpriteRendererLeft;
    [SerializeField] AnimatedSpriteRenderer SpriteRendererRight;

    [SerializeField] AnimatedSpriteRenderer SpriteRendererDeath;
    
    AnimatedSpriteRenderer ActiveSpriteRenderer;

    public Rigidbody2D Rigidbody { get; private set; }


    private Vector2 direction = Vector2.down;

    
    void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        ActiveSpriteRenderer = SpriteRendererDown;
    }

    private void Update() {
        if(Input.GetKey(InputUp)) {
            SetDirection(Vector2.up, SpriteRendererUp);
        }
        else if (Input.GetKey(InputDown)) {
            SetDirection(Vector2.down, SpriteRendererDown);
        }
        else if(Input.GetKey(InputLeft)) {
            SetDirection(Vector2.left, SpriteRendererLeft);
        }
        else if(Input.GetKey(InputRight)) {
            SetDirection(Vector2.right, SpriteRendererRight);
        }
        else {
            // Player is not moving
            SetDirection(Vector2.zero, ActiveSpriteRenderer);
        }
    }

    private void FixedUpdate() {
        Vector2 position = Rigidbody.position;
        Vector2 translation = direction * Speed * Time.fixedDeltaTime;

        Rigidbody.MovePosition(position + translation);
    }

    private void SetDirection(Vector2 newDirection, AnimatedSpriteRenderer spriteRenderer) {
        direction = newDirection;

        SpriteRendererUp.enabled = spriteRenderer == SpriteRendererUp;
        SpriteRendererDown.enabled = spriteRenderer == SpriteRendererDown;
        SpriteRendererLeft.enabled = spriteRenderer == SpriteRendererLeft;
        SpriteRendererRight.enabled = spriteRenderer == SpriteRendererRight;

        ActiveSpriteRenderer = spriteRenderer;
        ActiveSpriteRenderer.Idle = direction == Vector2.zero;
    }


    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.name.Contains("Explosion")) {
            DeathSequence();
        }
    }

    void DeathSequence() {
        this.enabled = false;
        GetComponent<BombController>().enabled = false;

        SpriteRendererUp.enabled = false;
        SpriteRendererDown.enabled = false;
        SpriteRendererLeft.enabled = false;
        SpriteRendererRight.enabled = false;

        SpriteRendererDeath.enabled = true;

        Invoke(nameof(OnDeathSequnceEnded), 1.25f);
    }

    void OnDeathSequnceEnded() {
        gameObject.SetActive(false);
    }
}
