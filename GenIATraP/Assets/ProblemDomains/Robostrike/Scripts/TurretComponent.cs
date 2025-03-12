using UnityEngine;
using UnityEngine.UIElements;

namespace Problems.Robostrike
{
    public class TurretComponent : MonoBehaviour
    {
        public void SetTurretColor(Color color)
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            renderer.color = color;
        }
    }
}