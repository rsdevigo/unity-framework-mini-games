using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityFramework.MiniGames.UI
{
    public sealed class DropBinView : MonoBehaviour, IDropHandler
    {
        [SerializeField] string _categoryId;
        [SerializeField] Image _background;

        public string CategoryId => _categoryId;

        public event Action<DraggableUI> Received;

        public void SetCategoryId(string id) => _categoryId = id;

        public void OnDrop(PointerEventData eventData)
        {
            var d = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<DraggableUI>() : null;
            if (d == null)
                return;
            Received?.Invoke(d);
        }

        public void SetHighlight(bool on)
        {
            if (_background == null)
                _background = GetComponent<Image>();
            if (_background == null)
                return;
            _background.color = on ? new Color(0.7f, 1f, 0.7f) : Color.white;
        }
    }
}
