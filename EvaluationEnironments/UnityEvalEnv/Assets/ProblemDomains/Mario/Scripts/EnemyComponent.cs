using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Problems.Mario
{
    public class EnemyComponent: MonoBehaviour
    {
        public EnemyType EnemyType;
    }

    public enum EnemyType
    {
        Goomba,
        Koopa
    }
}
