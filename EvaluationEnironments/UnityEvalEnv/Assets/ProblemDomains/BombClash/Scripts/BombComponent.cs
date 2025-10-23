using UnityEngine;

namespace Problems.BombClash
{
    public class BombComponent : MonoBehaviour
    {
    }

    public class ActiveBomb
    {
        public BombComponent Bomb { get; set; }
        public BombermanAgentComponent Owner { get; set; }
        public int TimeTolExplosion { get; set; }
        public ActiveBomb(BombComponent bomb, BombermanAgentComponent owner, int timeTolExplosion)
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