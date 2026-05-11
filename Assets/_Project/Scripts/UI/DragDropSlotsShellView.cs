using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityFramework.MiniGames.UI
{
    /// <summary>
    /// Builds a row of drop bins plus draggable tokens for classification / sorting games.
    /// </summary>
    public sealed class DragDropSlotsShellView : MonoBehaviour
    {
        [SerializeField] RectTransform _binsParent;
        [SerializeField] RectTransform _tokensParent;

        readonly List<DraggableUI> _tokens = new();
        readonly List<DropBinView> _bins = new();

        void Awake()
        {
            GameplayUiUtility.EnsureEventSystem();
            if (_binsParent == null || _tokensParent == null)
            {
                var canvas = GameplayUiUtility.CreateOverlayCanvas("DragDropShell", transform);
                var root = new GameObject("Root", typeof(RectTransform), typeof(VerticalLayoutGroup)).GetComponent<RectTransform>();
                root.SetParent(canvas.transform, false);
                var v = root.GetComponent<VerticalLayoutGroup>();
                v.padding = new RectOffset(40, 40, 120, 120);
                v.spacing = 40f;
                v.childAlignment = TextAnchor.UpperCenter;
                v.childControlWidth = true;
                v.childControlHeight = false;
                root.anchorMin = Vector2.zero;
                root.anchorMax = Vector2.one;
                root.offsetMin = Vector2.zero;
                root.offsetMax = Vector2.zero;

                _binsParent = new GameObject("Bins", typeof(RectTransform), typeof(HorizontalLayoutGroup)).GetComponent<RectTransform>();
                _binsParent.SetParent(root, false);
                var bh = _binsParent.GetComponent<HorizontalLayoutGroup>();
                bh.spacing = 24f;
                bh.childAlignment = TextAnchor.MiddleCenter;

                _tokensParent = new GameObject("Tokens", typeof(RectTransform), typeof(HorizontalLayoutGroup)).GetComponent<RectTransform>();
                _tokensParent.SetParent(root, false);
                var th = _tokensParent.GetComponent<HorizontalLayoutGroup>();
                th.spacing = 16f;
                th.childAlignment = TextAnchor.MiddleCenter;
            }
        }

        public void ClearDynamic()
        {
            foreach (var t in _tokens)
            {
                if (t != null)
                    Destroy(t.gameObject);
            }

            _tokens.Clear();
            foreach (var b in _bins)
            {
                if (b != null)
                    Destroy(b.gameObject);
            }

            _bins.Clear();
        }

        public void BuildBins(IReadOnlyList<string> categoryIds, Action<DraggableUI, DropBinView> onDrop)
        {
            ClearDynamic();
            if (categoryIds == null)
                return;
            foreach (var id in categoryIds)
            {
                if (string.IsNullOrEmpty(id))
                    continue;
                var go = new GameObject($"Bin_{id}", typeof(RectTransform), typeof(Image), typeof(DropBinView));
                go.transform.SetParent(_binsParent, false);
                var rt = (RectTransform)go.transform;
                rt.sizeDelta = new Vector2(200, 200);
                var img = go.GetComponent<Image>();
                img.color = new Color(0.95f, 0.95f, 1f);
                var bin = go.GetComponent<DropBinView>();
                bin.SetCategoryId(id);
                if (onDrop != null)
                    bin.Received += d => onDrop(d, bin);
                _bins.Add(bin);
            }
        }

        public DraggableUI AddClassifiableToken(Sprite sprite, string categoryId)
        {
            var go = new GameObject($"Tok_{categoryId}", typeof(RectTransform), typeof(Image), typeof(DraggableUI), typeof(ClassificationItemTag));
            go.transform.SetParent(_tokensParent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(140, 140);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = sprite != null ? Color.white : new Color(1f, 0.92f, 0.8f);
            var drag = go.GetComponent<DraggableUI>();
            drag.SetTokenId(categoryId);
            go.GetComponent<ClassificationItemTag>().CategoryId = categoryId;
            _tokens.Add(drag);
            return drag;
        }
    }
}
