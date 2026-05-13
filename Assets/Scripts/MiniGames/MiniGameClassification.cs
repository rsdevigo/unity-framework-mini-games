using System.Collections;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityFramework.MiniGames.Data;
using UnityFramework.MiniGames.UI;

namespace UnityFramework.MiniGames.Gameplay
{
    /// <summary>
    /// One draggable token at a time into the correct category bin.
    /// </summary>
    public sealed class MiniGameClassification : MiniGameBase
    {
        DragDropSlotsShellView _shell;
        VisualElement _pendingToken;
        string _pendingBinCategory;
        bool _dropReceived;

        protected override void OnInitialized()
        {
            _shell = GetComponent<DragDropSlotsShellView>();
            if (_shell == null)
                _shell = gameObject.AddComponent<DragDropSlotsShellView>();
        }

        protected override IEnumerator RunSessionRoutine()
        {
            yield return null;

            var set = Config.ChallengeSet;
            if (set?.Challenges == null || set.Challenges.Count == 0)
                yield break;

            var diff = Config.Difficulty;
            var rounds = diff != null ? diff.Rounds : 3;

            for (var r = 0; r < rounds; r++)
            {
                var ch = ChallengePicker.PickRandom(set.Challenges);
                if (ch is not ClassificationChallengeSO cl || cl.Items == null || cl.BinLabels == null)
                    continue;

                var order = cl.Items.Where(i => i != null && !string.IsNullOrEmpty(i.categoryId)).OrderBy(_ => Random.value).ToList();
                foreach (var item in order)
                {
                    _shell.ClearDynamic();
                    _shell.BuildBins(cl.BinLabels, OnDropped);
                    _shell.AddClassifiableToken(item.sprite, item.categoryId);

                    if (cl.PromptNarration != null)
                        Context.Audio.EnqueueNarration(cl.PromptNarration);

                    _pendingToken = null;
                    _pendingBinCategory = null;
                    _dropReceived = false;
                    var sw = Stopwatch.StartNew();
                    while (!_dropReceived)
                        yield return null;
                    sw.Stop();

                    var state = _pendingToken.userData as ClassificationTokenState;
                    var expected = state != null ? state.CategoryId : string.Empty;
                    var ok = expected == _pendingBinCategory;
                    var result = new EvaluationResult(ok, new[] { $"classify:{expected}" }, (float)sw.Elapsed.TotalSeconds);
                    RaiseAnswerEvaluated(result);
                    PlayFeedback(result);

                    if (ok)
                        _pendingToken.RemoveFromHierarchy();
                    else
                        _shell.ResetTokenHome(_pendingToken);

                    _pendingToken = null;
                    _pendingBinCategory = null;
                    _dropReceived = false;
                    yield return new WaitForSecondsRealtime(0.35f);
                }
            }
        }

        void OnDropped(VisualElement token, string binCategory)
        {
            _pendingToken = token;
            _pendingBinCategory = binCategory;
            _dropReceived = true;
        }
    }
}
