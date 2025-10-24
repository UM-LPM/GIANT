using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Problems.BombClash
{
    public class BombComponent : MonoBehaviour
    {
        [HideInInspector] public Vector2Int Position;

        private void Awake()
        {
            Position = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        }

        public void Move(int x, int y)
        {
            Position.x += x;
            Position.y += y;

            transform.position = new Vector2(Position.x, Position.y);
        }
    }

    public class ActiveBomb
    {
        public BombComponent Bomb { get; set; }
        public BombClashAgentComponent Owner { get; set; }
        public int TimeTolExplosion { get; set; }

        public ActiveBomb(BombComponent bomb, BombClashAgentComponent owner, int timeTolExplosion)
        {
            Bomb = bomb;
            Owner = owner;
            TimeTolExplosion = timeTolExplosion;
        }

        public bool DecreaseTime()
        {
            TimeTolExplosion--;
            return TimeTolExplosion <= 0f;
        }
    }

}