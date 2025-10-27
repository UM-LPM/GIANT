using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace Problems.BoxTact
{
    public class BoxTactBoxComponent : MonoBehaviour
    {
        public BoxTactAgentComponent LastAgentThatMoved { get; set; }
        public SpriteRenderer BoxSpriteRenderer { get; set; }

        public bool IsOnTarget { get; set; }

        private void Awake()
        {
            BoxSpriteRenderer = GetComponent<SpriteRenderer>();
            if (!BoxSpriteRenderer)
            {
                DebugSystem.LogError("BoxTactBoxComponent requires a SpriteRenderer component.");
            }

            IsOnTarget = false;
        }
    }
}
