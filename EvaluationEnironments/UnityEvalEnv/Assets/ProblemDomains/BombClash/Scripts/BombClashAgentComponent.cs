using Base;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.BombClash
{
    public class BombClashAgentComponent : AgentComponent
    {
        [Header("Animation Sprite Renderers")]
        [SerializeField] public AnimatedSpriteRenderer SpriteRendererUp;
        [SerializeField] public AnimatedSpriteRenderer SpriteRendererDown;
        [SerializeField] public AnimatedSpriteRenderer SpriteRendererLeft;
        [SerializeField] public AnimatedSpriteRenderer SpriteRendererRight;
        [SerializeField] public AnimatedSpriteRenderer SpriteRendererDeath;

        public BombClashEnvironmentController BombClashEnvironmentController { get; set; }

        public float MoveSpeed { get; set; }
        public int MaxAgentBombAmout { get; set; }

        public int ExplosionRadius { get; set; }

        public int BombsRemaining { get; set; }

        public int Health { get; set; }
        public int NextDamageTime { get; set; } // Required to prevent constant damage from explosions

        public AnimatedSpriteRenderer ActiveSpriteRenderer { get; set; }

        public Vector2Int MoveDirection { get; private set; }
        public int NextAgentUpdateTime { get; set; }

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

        [HideInInspector] public Vector2Int Position;

        protected override void DefineAdditionalDataOnAwake()
        {
            ActiveSpriteRenderer = SpriteRendererDown;
            MoveDirection = Vector2Int.zero;
            ExploredSectors = new List<Vector2>();
            Position = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        }

        public void SetStartParams(BombClashEnvironmentController bombermanEnvironmentController, float startMoveSpeed, int startHealth, int startExplosionRadius, int startBombAmount)
        {
            BombClashEnvironmentController = bombermanEnvironmentController;
            MoveSpeed = startMoveSpeed;
            Health = startHealth;
            ExplosionRadius = startExplosionRadius;
            SetBombs(startBombAmount);
            MoveDirection = Vector2Int.zero;
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

        public void SetDirection(Vector2Int newDirection, AnimatedSpriteRenderer spriteRenderer)
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

            //Invoke(nameof(OnDeathSequnceEnded), 0.5f); // Death animation
            gameObject.SetActive(false);
        }

        void OnDeathSequnceEnded()
        {
            gameObject.SetActive(false);
            BombClashEnvironmentController.CheckEndingState();
        }

        public void CheckIfNewSectorExplored()
        {
            //CurrentAgentPosition.x = (int)transform.position.x;
            //CurrentAgentPosition.y = (int)transform.position.y;
            // Check if there's already sector with agents x and y coordinates
            if (!ExploredSectors.Contains(Position))
            {
                ExploredSectors.Add(new Vector2Int((int)transform.position.x, (int)transform.position.y));
                SectorsExplored++;
            }
        }

        public void Move(int x, int y)
        {
            Position.x += x;
            Position.y += y;

            transform.position = new Vector2(Position.x, Position.y);
        }
    }
}