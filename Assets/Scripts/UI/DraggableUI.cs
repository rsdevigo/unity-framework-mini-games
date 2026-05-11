using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityFramework.MiniGames.UI
{
    /// <summary>
    /// Canvas UI drag helper (mouse-first, works with touch via EventSystem).
    /// </summary>
    public sealed class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] string _tokenId;
        RectTransform _rt;
        Canvas _canvas;
        CanvasGroup _group;
        Vector2 _startAnchored;

        public string TokenId => _tokenId;

        void Awake()
        {
            _rt = (RectTransform)transform;
            _group = GetComponent<CanvasGroup>();
            if (_group == null)
                _group = gameObject.AddComponent<CanvasGroup>();
            _canvas = GetComponentInParent<Canvas>();
        }

        public void SetTokenId(string id) => _tokenId = id;

        public void OnBeginDrag(PointerEventData eventData)
        {
            _startAnchored = _rt.anchoredPosition;
            _group.alpha = 0.75f;
            _group.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_canvas == null)
                return;
            _rt.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _group.alpha = 1f;
            _group.blocksRaycasts = true;
        }

        public void ResetToStart() => _rt.anchoredPosition = _startAnchored;
    }
}
