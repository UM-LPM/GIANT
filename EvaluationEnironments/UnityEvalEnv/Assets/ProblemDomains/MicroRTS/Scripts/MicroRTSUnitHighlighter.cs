using UnityEngine;

namespace Problems.MicroRTS
{
    public class MicroRTSUnitHighlighter : MonoBehaviour
    {
        [SerializeField] private Color highlightColor = Color.green;

        private Renderer unitRenderer;
        private Color originalColor;
        private bool isHighlighted = false;

        void Awake()
        {
            unitRenderer = GetComponent<Renderer>();
            if (unitRenderer != null)
            {
                if (unitRenderer is SpriteRenderer spriteRenderer)
                {
                    originalColor = spriteRenderer.color;
                }
                else if (unitRenderer.material != null)
                {
                    originalColor = unitRenderer.material.color;
                }
            }
        }

        public void SetHighlighted(bool highlighted)
        {
            if (isHighlighted == highlighted) return;

            isHighlighted = highlighted;

            if (unitRenderer == null) return;

            if (unitRenderer is SpriteRenderer spriteRenderer)
            {
                spriteRenderer.color = highlighted ? highlightColor : originalColor;
            }
            else if (unitRenderer.material != null)
            {
                if (highlighted)
                {
                    Material highlightMaterial = new Material(unitRenderer.material);
                    highlightMaterial.color = highlightColor;
                    unitRenderer.material = highlightMaterial;
                }
                else
                {
                    Material originalMaterial = new Material(unitRenderer.material);
                    originalMaterial.color = originalColor;
                    unitRenderer.material = originalMaterial;
                }
            }
        }
    }
}

