using UnityEngine;

namespace CustomUI
{
    public class TogglePanelPropertyGroup : MonoBehaviour
    {
        [SerializeField] Vector2 OpenPos;
        [SerializeField] Vector2 ClosePos;

        bool isOpened = true;

        private void Start()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.anchoredPosition = OpenPos;
        }

        public void OnTogglePanelPropertyGroup()
        {
            isOpened = !isOpened;
            
            Vector2 targetPos = isOpened ? OpenPos : ClosePos;
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.anchoredPosition = targetPos;
        }

    }
}
