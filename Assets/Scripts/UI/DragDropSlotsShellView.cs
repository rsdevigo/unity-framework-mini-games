using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityFramework.MiniGames.UI
{
    /// <summary>
    /// Drop bins + draggable classification tokens (UI Toolkit).
    /// </summary>
    public sealed class DragDropSlotsShellView : MonoBehaviour
    {
        [SerializeField] UIDocument _uiDocument;
        [SerializeField] PanelSettings _panelSettingsOverride;
        [SerializeField] VisualTreeAsset _shellUxml;
        [SerializeField] int _sortingOrder = 100;

        VisualElement _binsRow;
        VisualElement _tokensRow;
        Action<VisualElement, string> _dropHandler;

        void Awake() => EnsureDocument();

        void EnsureDocument()
        {
            GameplayUiUtility.EnsureEventSystem();
            if (_uiDocument == null)
                _uiDocument = GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
            _uiDocument.sortingOrder = _sortingOrder;
            if (_panelSettingsOverride != null)
                _uiDocument.panelSettings = _panelSettingsOverride;
            else
                EduUiToolkitDefaults.ApplyTo(_uiDocument);
            if (_shellUxml == null)
                _shellUxml = EduUiToolkitDefaults.LoadVisualTree("DragDropShell");
            _uiDocument.visualTreeAsset = _shellUxml;
            var root = _uiDocument.rootVisualElement;
            _binsRow = root.Q("bins-row");
            _tokensRow = root.Q("tokens-row");
        }

        public void ClearDynamic()
        {
            EnsureDocument();
            _binsRow?.Clear();
            _tokensRow?.Clear();
        }

        public void BuildBins(IReadOnlyList<string> categoryIds, Action<VisualElement, string> onDrop)
        {
            ClearDynamic();
            _dropHandler = onDrop;
            if (categoryIds == null || _binsRow == null)
                return;
            foreach (var id in categoryIds)
            {
                if (string.IsNullOrEmpty(id))
                    continue;
                var bin = new VisualElement();
                bin.name = $"bin_{id}";
                bin.AddToClassList("edu-drop-bin");
                bin.userData = id;
                var lbl = new Label(id);
                lbl.style.unityTextAlign = TextAnchor.MiddleCenter;
                lbl.style.color = new Color(0.15f, 0.15f, 0.2f);
                lbl.style.whiteSpace = WhiteSpace.Normal;
                bin.Add(lbl);
                _binsRow.Add(bin);
            }
        }

        public VisualElement AddClassifiableToken(Sprite sprite, string categoryId)
        {
            EnsureDocument();
            var token = new VisualElement();
            token.name = $"tok_{categoryId}";
            token.AddToClassList("classification-token");
            token.pickingMode = PickingMode.Position;
            token.userData = new ClassificationTokenState { CategoryId = categoryId };

            if (sprite != null)
            {
                var fill = new VisualElement();
                fill.style.flexGrow = 1;
                fill.style.backgroundImage = new StyleBackground(sprite);
                fill.pickingMode = PickingMode.Ignore;
                token.Add(fill);
            }
            else
            {
                var lbl = new Label(categoryId);
                lbl.style.unityTextAlign = TextAnchor.MiddleCenter;
                lbl.style.flexGrow = 1;
                lbl.style.color = Color.black;
                token.Add(lbl);
            }

            token.AddManipulator(new EduClassificationDragManipulator((t, cat) =>
            {
                if (!string.IsNullOrEmpty(cat))
                    _dropHandler?.Invoke(t, cat);
            }));

            _tokensRow?.Add(token);
            StartCoroutine(CaptureTokenHomeNextFrame(token));
            return token;
        }

        public void ResetTokenHome(VisualElement token)
        {
            if (token?.userData is ClassificationTokenState st)
                EduDragDropVisuals.ResetTokenHome(token, st.HomeX, st.HomeY);
        }

        static IEnumerator CaptureTokenHomeNextFrame(VisualElement token)
        {
            yield return null;
            if (token == null)
                yield break;
            if (token.userData is not ClassificationTokenState st)
                yield break;
            var l = token.layout;
            st.HomeX = l.x;
            st.HomeY = l.y;
        }
    }
}
