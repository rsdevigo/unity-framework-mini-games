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
    /// Simple memory: match two cards sharing the same <see cref="MemoryPairChallengeSO.PairId"/>.
    /// </summary>
    public sealed class MiniGameMemory : MiniGameBase
    {
        sealed class CardVm
        {
            public Button Button;
            public string PairId;
            public bool FaceUp;
            public bool Matched;
        }

        UIDocument _doc;
        CardVm _pending;

        UIDocument EnsureDoc()
        {
            if (_doc == null)
                _doc = gameObject.GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
            _doc.sortingOrder = 100;
            EduUiToolkitDefaults.ApplyTo(_doc);
            if (_doc.visualTreeAsset == null)
                _doc.visualTreeAsset = EduUiToolkitDefaults.LoadVisualTree("MemoryShell");
            return _doc;
        }

        protected override IEnumerator RunSessionRoutine()
        {
            GameplayUiUtility.EnsureEventSystem();
            EnsureDoc();
            yield return null;

            var grid = _doc.rootVisualElement.Q("memory-grid");
            if (grid == null)
                yield break;

            var set = Config.ChallengeSet;
            if (set?.Challenges == null)
                yield break;

            var pairs = set.Challenges.OfType<MemoryPairChallengeSO>().ToList();
            if (pairs.Count == 0)
                yield break;

            var deck = new List<CardVm>();
            foreach (var p in pairs)
            {
                deck.Add(MakeCard(p));
                deck.Add(MakeCard(p));
            }

            Shuffle(deck);
            foreach (var c in deck)
                grid.Add(c.Button);

            CardVm first = null;
            var sw = Stopwatch.StartNew();
            var matchesNeeded = pairs.Count;
            var matches = 0;

            while (matches < matchesNeeded)
            {
                _pending = null;
                while (_pending == null)
                    yield return null;

                var picked = _pending;
                _pending = null;
                if (picked.Matched || picked.FaceUp)
                    continue;

                if (first == null)
                {
                    Flip(picked, true);
                    first = picked;
                    continue;
                }

                if (first == picked)
                    continue;

                Flip(picked, true);
                var ok = first.PairId == picked.PairId;
                sw.Stop();
                var result = new EvaluationResult(ok, new[] { $"memory:{first.PairId}" }, (float)sw.Elapsed.TotalSeconds);
                RaiseAnswerEvaluated(result);
                PlayFeedback(result);

                if (ok)
                {
                    first.Matched = true;
                    picked.Matched = true;
                    first.Button.SetEnabled(false);
                    picked.Button.SetEnabled(false);
                    matches++;
                }
                else
                {
                    yield return new WaitForSecondsRealtime(0.55f);
                    Flip(first, false);
                    Flip(picked, false);
                }

                first = null;
                sw = Stopwatch.StartNew();
                yield return new WaitForSecondsRealtime(0.2f);
            }
        }

        CardVm MakeCard(MemoryPairChallengeSO model)
        {
            var vm = new CardVm { PairId = model.PairId, FaceUp = false, Matched = false };
            var btn = new Button { text = "?" };
            vm.Button = btn;
            btn.style.width = 160;
            btn.style.height = 160;
            btn.style.marginRight = 12;
            btn.style.marginBottom = 12;
            btn.style.backgroundColor = new Color(0.2f, 0.45f, 0.85f);
            btn.style.color = Color.white;
            btn.style.fontSize = 22;
            btn.style.borderTopLeftRadius = 8;
            btn.style.borderTopRightRadius = 8;
            btn.style.borderBottomLeftRadius = 8;
            btn.style.borderBottomRightRadius = 8;
            btn.RegisterCallback<ClickEvent>(_ =>
            {
                if (vm.Matched)
                    return;
                _pending = vm;
            });
            return vm;
        }

        static void Flip(CardVm c, bool faceUp)
        {
            c.FaceUp = faceUp;
            c.Button.text = faceUp ? c.PairId : "?";
        }

        static void Shuffle(List<CardVm> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
