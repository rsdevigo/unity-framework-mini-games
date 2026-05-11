using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityFramework.MiniGames.Data;

namespace UnityFramework.MiniGames.UI
{
    /// <summary>
    /// Inspector-friendly shell: wires large choice tiles from a <see cref="MultipleChoiceChallengeSO"/>.
    /// </summary>
    public sealed class MultipleChoiceShellView : MonoBehaviour
    {
        [SerializeField] RectTransform _choicesParent;
        [SerializeField] bool _rebuildEachBind = true;

        readonly List<Button> _buttons = new();

        void Awake()
        {
            if (_choicesParent == null)
            {
                var canvas = GameplayUiUtility.CreateOverlayCanvas("MultipleChoiceShell", transform);
                _choicesParent = new GameObject("ChoicesRow", typeof(RectTransform), typeof(HorizontalLayoutGroup)).GetComponent<RectTransform>();
                _choicesParent.SetParent(canvas.transform, false);
                var h = _choicesParent.GetComponent<HorizontalLayoutGroup>();
                h.spacing = 24f;
                h.childAlignment = TextAnchor.MiddleCenter;
                h.childControlWidth = false;
                h.childControlHeight = false;
                h.padding = new RectOffset(40, 40, 200, 200);
                var rt = _choicesParent;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
        }

        public void Bind(MultipleChoiceChallengeSO challenge, int maxChoices, Action<int> onChosen)
        {
            if (challenge == null || onChosen == null)
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
                var btn = GameplayUiUtility.CreateChoiceButton(_choicesParent, id, sprite);
                var captured = originalIndex;
                btn.onClick.AddListener(() => onChosen(captured));
                _buttons.Add(btn);
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

        void ClearChildren()
        {
            foreach (var b in _buttons)
            {
                if (b != null)
                    Destroy(b.gameObject);
            }

            _buttons.Clear();
            for (var i = _choicesParent.childCount - 1; i >= 0; i--)
                Destroy(_choicesParent.GetChild(i).gameObject);
        }
    }
}
