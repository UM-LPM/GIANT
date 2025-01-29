using Base;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.Bomberman
{
    public class BombermanAgentComponent : AgentComponent
    {
        [Header("Animation Sprite Renderers")]
        [SerializeField] public AnimatedSpriteRenderer SpriteRendererUp;
        [SerializeField] public AnimatedSpriteRenderer SpriteRendererDown;
        [SerializeField] public AnimatedSpriteRenderer SpriteRendererLeft;
        [SerializeField] public AnimatedSpriteRenderer SpriteRendererRight;
        [SerializeField] public AnimatedSpriteRenderer SpriteRendererDeath;

        public BombermanEnvironmentController BombermanEnvironmentController { get; set; }

        public float MoveSpeed { get; set; }
        public int MaxAgentBombAmout { get; set; }

        public int ExplosionRadius { get; set; }

        public int BombsRemaining { get; set; }

        public int Health { get; set; }
        public float NextDamageTime { get; set; } // Required to prevent constant damage from explosions

        public AnimatedSpriteRenderer ActiveSpriteRenderer { get; set; }

        public Vector2 MoveDirection { get; private set; }
        public float NextAgentUpdateTime { get; set; }

        List<Vector2> ExploredSectors;
        // Agent fitness variables
        public int SectorsExplored { get; set; }
        public int BombsPlaced { get; set; }
        public int BlocksDestroyed { get; set; }
        public int PowerUpsCollected { get; set; }
        public int BombsHitAgent { get; set; }
        public int AgentsKilled { get; set; }
        public int AgentHitByBombs { get; set; }
        public int AgentHitByOwnBombs { get; set; }
        public bool AgentDied { get; set; }
        public int SurvivalBonuses { get; set; }
        public bool LastSurvivalBonus { get; set; }


        private Vector2 CurrentAgentPosition;

        protected override void DefineAdditionalDataOnAwake()
        {
            ActiveSpriteRenderer = SpriteRendererDown;
            MoveDirection = Vector2.zero;
            ExploredSectors = new List<Vector2>();
            CurrentAgentPosition = new Vector2(transform.position.x, transform.position.y);
        }

        public void SetStartParams(BombermanEnvironmentController bombermanEnvironmentController, float startMoveSpeed, int startHealth, int startExplosionRadius, int startBombAmount)
        {
            BombermanEnvironmentController = bombermanEnvironmentController;
            MoveSpeed = startMoveSpeed;
            Health = startHealth;
            ExplosionRadius = startExplosionRadius;
            SetBombs(startBombAmount);
            MoveDirection = Vector2.zero;
        }

        public void AddBomb()
        {
            MaxAgentBombAmout++;
            BombsRemaining++;
        }

        public void SetBombs(int Bombs)
        {
            MaxAgentBombAmout = Bombs;
            BombsRemaining = MaxAgentBombAmout;
        }

        public void SetDirection(Vector2 newDirection, AnimatedSpriteRenderer spriteRenderer)
        {
            MoveDirection = newDirection;

            SpriteRendererUp.enabled = spriteRenderer == SpriteRendererUp;
            SpriteRendererDown.enabled = spriteRenderer == SpriteRendererDown;
            SpriteRendererLeft.enabled = spriteRenderer == SpriteRendererLeft;
            SpriteRendererRight.enabled = spriteRenderer == SpriteRendererRight;

            ActiveSpriteRenderer = spriteRenderer;
            ActiveSpriteRenderer.Idle = MoveDirection == Vector2.zero;
        }

        public void DeathSequence()
        {
            this.enabled = false;

            SpriteRendererUp.enabled = false;
            SpriteRendererDown.enabled = false;
            SpriteRendererLeft.enabled = false;
            SpriteRendererRight.enabled = false;

            SpriteRendererDeath.enabled = true;

            Invoke(nameof(OnDeathSequnceEnded), 0.5f);
        }

        void OnDeathSequnceEnded()
        {
            gameObject.SetActive(false);
            BombermanEnvironmentController.CheckEndingState();
        }

        public void CheckIfNewSectorExplored()
        {
            CurrentAgentPosition.x = transform.position.x;
            CurrentAgentPosition.y = transform.position.y;
            // Check if there's already sector with agents x and y coordinates
            if (!ExploredSectors.Contains(CurrentAgentPosition))
            {
                ExploredSectors.Add(new Vector2(transform.position.x, transform.position.y));
                SectorsExplored++;
            }
        }
    }
}