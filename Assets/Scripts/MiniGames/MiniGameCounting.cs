using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using UnityFramework.MiniGames.Data;
using UnityFramework.MiniGames.UI;

namespace UnityFramework.MiniGames.Gameplay
{
    /// <summary>
    /// Quantity discrimination using large number buttons (mouse-first).
    /// </summary>
    public sealed class MiniGameCounting : MiniGameBase
    {
        protected override IEnumerator RunSessionRoutine()
        {
            GameplayUiUtility.EnsureEventSystem();
            var canvas = GameplayUiUtility.CreateOverlayCanvas("CountingGame", transform);
            var root = new GameObject("Root", typeof(RectTransform), typeof(VerticalLayoutGroup)).GetComponent<RectTransform>();
            root.SetParent(canvas.transform, false);
            var v = root.GetComponent<VerticalLayoutGroup>();
            v.spacing = 32f;
            v.padding = new RectOffset(60, 60, 160, 60);
            v.childAlignment = TextAnchor.MiddleCenter;
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            var set = Config.ChallengeSet;
            if (set == null || set.Challenges == null || set.Challenges.Count == 0)
                yield break;

            var diff = Config.Difficulty;
            var rounds = diff != null ? diff.Rounds : 3;

            for (var r = 0; r < rounds; r++)
            {
                var ch = ChallengePicker.PickRandom(set.Challenges);
                if (ch is not QuantityChallengeSO q)
                    continue;

                foreach (Transform c in root)
                    Destroy(c.gameObject);

                if (q.PromptNarration != null)
                    Context.Audio.EnqueueNarration(q.PromptNarration);

                var prompt = new GameObject("Prompt", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
                prompt.transform.SetParent(root, false);
                prompt.font = GameplayUiUtility.BuiltinRuntimeFont;
                prompt.fontSize = 42;
                prompt.alignment = TextAnchor.MiddleCenter;
                prompt.color = Color.white;
                prompt.text = $"?  ({q.TargetCount})";

                var row = new GameObject("Numbers", typeof(RectTransform), typeof(HorizontalLayoutGroup)).GetComponent<RectTransform>();
                row.SetParent(root, false);
                var h = row.GetComponent<HorizontalLayoutGroup>();
                h.spacing = 18f;

                int? picked = null;
                for (var n = 1; n <= 6; n++)
                {
                    var b = GameplayUiUtility.CreateChoiceButton(row, n.ToString(), null);
                    var captured = n;
                    b.onClick.AddListener(() => picked = captured);
                }

                var sw = Stopwatch.StartNew();
                while (!picked.HasValue)
                    yield return null;
                sw.Stop();

                var ok = picked.Value == q.TargetCount;
                var keys = new string[q.ConceptKeys.Count];
                for (var i = 0; i < keys.Length; i++)
                    keys[i] = q.ConceptKeys[i];

                var result = new EvaluationResult(ok, keys, (float)sw.Elapsed.TotalSeconds);
                RaiseAnswerEvaluated(result);
                PlayFeedback(result);
                yield return new WaitForSecondsRealtime(0.45f);
            }
        }
    }
}
