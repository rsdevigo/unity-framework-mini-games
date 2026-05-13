using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityFramework.MiniGames.Data;

namespace UnityFramework.MiniGames.UI
{
    /// <summary>
    /// Multiple-choice row built with UI Toolkit (UXML templates under Resources/EduUI/).
    /// </summary>
    public sealed class MultipleChoiceShellView : MonoBehaviour
    {
        [SerializeField] UIDocument _uiDocument;
        [SerializeField] PanelSettings _panelSettingsOverride;
        [SerializeField] VisualTreeAsset _shellUxml;
        [SerializeField] VisualTreeAsset _choiceTileUxml;
        [SerializeField] bool _rebuildEachBind = true;
        [SerializeField] int _sortingOrder = 100;

        VisualElement _choicesRow;

        void Awake() => EnsureUiDocument();

        void EnsureUiDocument()
        {
            if (_uiDocument == null)
                _uiDocument = GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
            _uiDocument.sortingOrder = _sortingOrder;
            if (_panelSettingsOverride != null)
                _uiDocument.panelSettings = _panelSettingsOverride;
            else
                EduUiToolkitDefaults.ApplyTo(_uiDocument);
            if (_shellUxml == null)
                _shellUxml = EduUiToolkitDefaults.LoadVisualTree("MultipleChoiceShell");
            if (_choiceTileUxml == null)
                _choiceTileUxml = EduUiToolkitDefaults.LoadVisualTree("ChoiceTile");
            _uiDocument.visualTreeAsset = _shellUxml;
            _choicesRow = _uiDocument.rootVisualElement.Q("choices-row");
        }

        public void Bind(MultipleChoiceChallengeSO challenge, int maxChoices, Action<int> onChosen)
        {
            if (challenge == null || onChosen == null)
                return;
            EnsureUiDocument();
            if (_choicesRow == null)
                _choicesRow = _uiDocument.rootVisualElement.Q("choices-row");
            if (_choicesRow == null)
                return;
            if (_rebuildEachBind)
                ClearChildren();

            var totalImages = challenge.OptionImages?.Count ?? 0;
            var totalIds = challenge.OptionIds?.Count ?? 0;
            var total = Mathf.Max(totalImages, totalIds);
            if (total <= 0)
                total = 2;

            var n = Mathf.Clamp(Mathf.Min(maxChoices, total), 1, total);
            var subset = BuildSubsetIndices(total, n, challenge.CorrectIndex);

            foreach (var originalIndex in subset)
            {
                var sprite = challenge.OptionImages != null && originalIndex < challenge.OptionImages.Count
                    ? challenge.OptionImages[originalIndex]
                    : null;
                var id = challenge.OptionIds != null && originalIndex < challenge.OptionIds.Count
                    ? challenge.OptionIds[originalIndex]
                    : $"opt{originalIndex}";
                var tile = _choiceTileUxml.CloneTree();
                var btn = tile.Q<Button>("choice-button");
                var label = tile.Q<Label>("choice-label");
                if (label != null)
                    label.text = id;
                if (sprite != null && btn != null)
                    btn.style.backgroundImage = new StyleBackground(sprite);
                var captured = originalIndex;
                if (btn != null)
                    btn.clicked += () => onChosen(captured);
                _choicesRow.Add(tile);
            }
        }

        static int[] BuildSubsetIndices(int total, int count, int correct)
        {
            count = Mathf.Clamp(count, 1, total);
            var correctClamped = Mathf.Clamp(correct, 0, total - 1);
            var set = new HashSet<int> { correctClamped };
            while (set.Count < count)
                set.Add(UnityEngine.Random.Range(0, total));
            var arr = new int[set.Count];
            set.CopyTo(arr);
            for (var i = arr.Length - 1; i > 0; i--)
            {
                var j = UnityEngine.Random.Range(0, i + 1);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }

            return arr;
        }

        void ClearChildren() => _choicesRow?.Clear();
    }
}
