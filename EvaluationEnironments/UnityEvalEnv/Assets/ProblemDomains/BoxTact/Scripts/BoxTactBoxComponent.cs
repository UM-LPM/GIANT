using UnityEngine;
using UnityEngine.UIElements;

namespace Problems.BoxTact
{
    public class BoxTactBoxComponent : MonoBehaviour
    {
        public BoxTactAgentComponent LastAgentThatMoved { get; set; }
        public SpriteRenderer BoxSpriteRenderer { get; set; }

        private void Awake()
        {
            BoxSpriteRenderer = GetComponent<SpriteRenderer>();
            if (!BoxSpriteRenderer)
            {
                Debug.LogError("BoxTactBoxComponent requires a SpriteRenderer component.");
            }
        }
    }
}
