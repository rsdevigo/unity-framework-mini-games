using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityFramework.MiniGames.Data;
using UnityFramework.MiniGames.UI;

namespace UnityFramework.MiniGames.Gameplay
{
    /// <summary>
    /// Click syllable tiles in the correct order (mouse-first, low text).
    /// </summary>
    public sealed class MiniGameSyllableBuilder : MiniGameBase
    {
        int? _pendingSyllableIndex;

        protected override IEnumerator RunSessionRoutine()
        {
            GameplayUiUtility.EnsureEventSystem();
            var canvas = GameplayUiUtility.CreateOverlayCanvas("SyllableGame", transform);
            var root = new GameObject("Root", typeof(RectTransform), typeof(VerticalLayoutGroup)).GetComponent<RectTransform>();
            root.SetParent(canvas.transform, false);
            var v = root.GetComponent<VerticalLayoutGroup>();
            v.spacing = 28f;
            v.padding = new RectOffset(60, 60, 140, 60);
            v.childAlignment = TextAnchor.MiddleCenter;
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            var set = Config.ChallengeSet;
            if (set?.Challenges == null || set.Challenges.Count == 0)
                yield break;

            var diff = Config.Difficulty;
            var rounds = diff != null ? diff.Rounds : 3;

            for (var r = 0; r < rounds; r++)
            {
                var ch = ChallengePicker.PickRandom(set.Challenges);
                if (ch is not SyllableChallengeSO s || s.SyllablePartsInOrder == null || s.SyllablePartsInOrder.Length == 0)
                    continue;

                for (var i = root.childCount - 1; i >= 0; i--)
                    Destroy(root.GetChild(i).gameObject);

                if (s.PromptNarration != null)
                    Context.Audio.EnqueueNarration(s.PromptNarration);
                if (s.TargetWordAudio != null)
                    Context.Audio.EnqueueNarration(s.TargetWordAudio);

                var row = new GameObject("Syllables", typeof(RectTransform), typeof(HorizontalLayoutGroup)).GetComponent<RectTransform>();
                row.SetParent(root, false);
                row.GetComponent<HorizontalLayoutGroup>().spacing = 14f;

                var order = Enumerable.Range(0, s.SyllablePartsInOrder.Length).OrderBy(_ => Random.value).ToArray();
                foreach (var idx in order)
                {
                    var part = s.SyllablePartsInOrder[idx];
                    var sprite = s.SyllableSprites != null && idx < s.SyllableSprites.Length
                        ? s.SyllableSprites[idx]
                        : null;
                    var b = GameplayUiUtility.CreateChoiceButton(row, part, sprite);
                    var captured = idx;
                    b.onClick.AddListener(() => _pendingSyllableIndex = captured);
                }

                var next = 0;
                var sw = Stopwatch.StartNew();
                while (next < s.SyllablePartsInOrder.Length)
                {
                    _pendingSyllableIndex = null;
                    while (!_pendingSyllableIndex.HasValue)
                        yield return null;

                    var clicked = _pendingSyllableIndex.Value;
                    _pendingSyllableIndex = null;

                    if (clicked != next)
                    {
                        var bad = new EvaluationResult(false, new[] { $"syllable:{s.SyllablePartsInOrder[next]}" }, (float)sw.Elapsed.TotalSeconds);
                        RaiseAnswerEvaluated(bad);
                        PlayFeedback(bad);
                        next = 0;
                        sw = Stopwatch.StartNew();
                        continue;
                    }

                    next++;
                }

                sw.Stop();
                var good = new EvaluationResult(true, new[] { "syllable:word_complete" }, (float)sw.Elapsed.TotalSeconds);
                RaiseAnswerEvaluated(good);
                PlayFeedback(good);
                yield return new WaitForSecondsRealtime(0.45f);
            }
        }
    }
}
