using UnityEngine;

namespace Problems
{
    public class AnimatedSpriteRenderer : MonoBehaviour
    {
        [SerializeField] float AnimationTime = 0.25f;
        [SerializeField] Sprite IdleSprite;
        [SerializeField] Sprite[] AnimationSprites;
        [SerializeField] bool Loop = true;
        [SerializeField] public bool Idle = true;

        private SpriteRenderer SpriteRenderer;
        private int AnimationFrame;

        private void Awake()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
            SpriteRenderer.enabled = false;
        }

        private void OnEnable()
        {
            SpriteRenderer.enabled = true;
        }

        private void OnDisable()
        {
            SpriteRenderer.enabled = false;
        }

        private void Start()
        {
            InvokeRepeating(nameof(NextFrame), AnimationTime, AnimationTime);
        }

        void NextFrame()
        {
            AnimationFrame++;

            if (Loop && AnimationFrame >= AnimationSprites.Length)
            {
                AnimationFrame = 0;
            }

            if (Idle)
            {
                SpriteRenderer.sprite = IdleSprite;
            }
            else if (AnimationFrame >= 0 && AnimationFrame < AnimationSprites.Length)
            {
                SpriteRenderer.sprite = AnimationSprites[AnimationFrame];
            }
        }
    }
}