using UnityEngine;
using UnityEngine.EventSystems;

namespace MultiCraft.Scripts.Utils
{
    public class ButtonSound: MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            UISoundManager.Instance.PlayButtonClickSound();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            UISoundManager.Instance.PlayButtonHoverSound();
        }
    }
}