using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityFramework.MiniGames.Data;
using UnityFramework.MiniGames.UI;

namespace UnityFramework.MiniGames.Gameplay
{
    /// <summary>
    /// Click syllable tiles in the correct order (UI Toolkit).
    /// </summary>
    public sealed class MiniGameSyllableBuilder : MiniGameBase
    {
        int? _pendingSyllableIndex;
        UIDocument _doc;

        UIDocument EnsureDoc()
        {
            if (_doc == null)
                _doc = gameObject.GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
            _doc.sortingOrder = 100;
            EduUiToolkitDefaults.ApplyTo(_doc);
            if (_doc.visualTreeAsset == null)
                _doc.visualTreeAsset = EduUiToolkitDefaults.LoadVisualTree("SyllableShell");
            return _doc;
        }

        protected override IEnumerator RunSessionRoutine()
        {
            GameplayUiUtility.EnsureEventSystem();
            EnsureDoc();
            yield return null;

            var root = _doc.rootVisualElement;
            var syllablesRow = root.Q("syllables-row");
            if (syllablesRow == null)
                yield break;

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

                syllablesRow.Clear();

                if (s.PromptNarration != null)
                    Context.Audio.EnqueueNarration(s.PromptNarration);
                if (s.TargetWordAudio != null)
                    Context.Audio.EnqueueNarration(s.TargetWordAudio);

                var order = Enumerable.Range(0, s.SyllablePartsInOrder.Length).OrderBy(_ => Random.value).ToArray();
                foreach (var idx in order)
                {
                    var part = s.SyllablePartsInOrder[idx];
                    var sprite = s.SyllableSprites != null && idx < s.SyllableSprites.Length
                        ? s.SyllableSprites[idx]
                        : null;
                    var captured = idx;
                    var btn = new Button(() => _pendingSyllableIndex = captured)
                    {
                        text = part
                    };
                    btn.AddToClassList("edu-min-touch");
                    btn.style.width = 220;
                    btn.style.height = 220;
                    btn.style.marginRight = 14;
                    btn.style.marginBottom = 14;
                    btn.style.backgroundColor = new Color(0.85f, 0.9f, 1f);
                    btn.style.fontSize = 26;
                    btn.style.color = Color.black;
                    btn.style.borderTopLeftRadius = 12;
                    btn.style.borderTopRightRadius = 12;
                    btn.style.borderBottomLeftRadius = 12;
                    btn.style.borderBottomRightRadius = 12;
                    if (sprite != null)
                        btn.style.backgroundImage = new StyleBackground(sprite);
                    syllablesRow.Add(btn);
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
