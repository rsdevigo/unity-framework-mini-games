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
    /// Simple memory: match two cards sharing the same <see cref="MemoryPairChallengeSO.PairId"/>.
    /// </summary>
    public sealed class MiniGameMemory : MiniGameBase
    {
        sealed class CardVm
        {
            public Button Button;
            public Text Label;
            public string PairId;
            public bool FaceUp;
            public bool Matched;
        }

        CardVm _pending;

        protected override IEnumerator RunSessionRoutine()
        {
            GameplayUiUtility.EnsureEventSystem();
            var canvas = GameplayUiUtility.CreateOverlayCanvas("MemoryGame", transform);
            var grid = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup)).GetComponent<RectTransform>();
            grid.SetParent(canvas.transform, false);
            var gl = grid.GetComponent<GridLayoutGroup>();
            gl.cellSize = new Vector2(160, 160);
            gl.spacing = new Vector2(12, 12);
            gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gl.constraintCount = 4;
            grid.anchorMin = new Vector2(0.5f, 0.5f);
            grid.anchorMax = new Vector2(0.5f, 0.5f);
            grid.sizeDelta = new Vector2(900, 400);

            var set = Config.ChallengeSet;
            if (set?.Challenges == null)
                yield break;

            var pairs = set.Challenges.OfType<MemoryPairChallengeSO>().ToList();
            if (pairs.Count == 0)
                yield break;

            var deck = new List<CardVm>();
            foreach (var p in pairs)
            {
                deck.Add(MakeCard(grid, p));
                deck.Add(MakeCard(grid, p));
            }

            Shuffle(deck);
            WireDeck(deck);

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
                    first.Button.interactable = false;
                    picked.Button.interactable = false;
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
                WireDeck(deck);
                yield return new WaitForSecondsRealtime(0.2f);
            }
        }

        void WireDeck(List<CardVm> deck)
        {
            foreach (var c in deck)
            {
                if (c.Matched || c.Button == null)
                    continue;
                c.Button.onClick.RemoveAllListeners();
                var vm = c;
                c.Button.onClick.AddListener(() => _pending = vm);
            }
        }

        static CardVm MakeCard(Transform parent, MemoryPairChallengeSO model)
        {
            var go = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.2f, 0.45f, 0.85f);
            var btn = go.GetComponent<Button>();
            var txGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            txGo.transform.SetParent(go.transform, false);
            var trt = (RectTransform)txGo.transform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            var tx = txGo.GetComponent<Text>();
            tx.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            tx.alignment = TextAnchor.MiddleCenter;
            tx.color = Color.white;
            tx.resizeTextForBestFit = true;
            tx.resizeTextMinSize = 8;
            tx.resizeTextMaxSize = 28;
            tx.text = "?";
            return new CardVm { Button = btn, Label = tx, PairId = model.PairId, FaceUp = false };
        }

        static void Flip(CardVm c, bool faceUp)
        {
            c.FaceUp = faceUp;
            c.Label.text = faceUp ? c.PairId : "?";
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
