using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;
using UnityFramework.MiniGames.Data;
using UnityFramework.MiniGames.UI;

namespace UnityFramework.MiniGames.Gameplay
{
    /// <summary>
    /// Quantity discrimination using large number buttons (UI Toolkit).
    /// </summary>
    public sealed class MiniGameCounting : MiniGameBase
    {
        UIDocument _doc;

        UIDocument EnsureDoc()
        {
            if (_doc == null)
                _doc = gameObject.GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
            _doc.sortingOrder = 100;
            EduUiToolkitDefaults.ApplyTo(_doc);
            if (_doc.visualTreeAsset == null)
                _doc.visualTreeAsset = EduUiToolkitDefaults.LoadVisualTree("CountingShell");
            return _doc;
        }

        protected override IEnumerator RunSessionRoutine()
        {
            GameplayUiUtility.EnsureEventSystem();
            EnsureDoc();
            yield return null;

            var root = _doc.rootVisualElement;
            var numbersRow = root.Q("numbers-row");
            var promptLabel = root.Q<Label>("prompt-label");
            if (numbersRow == null || promptLabel == null)
                yield break;

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

                numbersRow.Clear();

                if (q.PromptNarration != null)
                    Context.Audio.EnqueueNarration(q.PromptNarration);

                promptLabel.text = $"?  ({q.TargetCount})";

                int? picked = null;
                for (var n = 1; n <= 6; n++)
                {
                    var captured = n;
                    var btn = new Button(() => picked = captured)
                    {
                        text = n.ToString()
                    };
                    btn.AddToClassList("edu-min-touch");
                    btn.style.width = 220;
                    btn.style.height = 220;
                    btn.style.marginRight = 18;
                    btn.style.marginBottom = 18;
                    btn.style.backgroundColor = new Color(0.85f, 0.9f, 1f);
                    btn.style.fontSize = 32;
                    btn.style.color = Color.black;
                    btn.style.borderTopLeftRadius = 12;
                    btn.style.borderTopRightRadius = 12;
                    btn.style.borderBottomLeftRadius = 12;
                    btn.style.borderBottomRightRadius = 12;
                    numbersRow.Add(btn);
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
