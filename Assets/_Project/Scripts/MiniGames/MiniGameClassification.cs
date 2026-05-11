using System.Collections;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
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
        DraggableUI _pendingDrag;
        DropBinView _pendingBin;

        protected override void OnInitialized()
        {
            _shell = GetComponent<DragDropSlotsShellView>();
            if (_shell == null)
                _shell = gameObject.AddComponent<DragDropSlotsShellView>();
        }

        protected override IEnumerator RunSessionRoutine()
        {
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

                    _pendingDrag = null;
                    _pendingBin = null;
                    var sw = Stopwatch.StartNew();
                    while (_pendingDrag == null || _pendingBin == null)
                        yield return null;
                    sw.Stop();

                    var tag = _pendingDrag.GetComponent<ClassificationItemTag>();
                    var expected = tag != null ? tag.CategoryId : _pendingDrag.TokenId;
                    var ok = expected == _pendingBin.CategoryId;
                    var result = new EvaluationResult(ok, new[] { $"classify:{expected}" }, (float)sw.Elapsed.TotalSeconds);
                    RaiseAnswerEvaluated(result);
                    PlayFeedback(result);

                    if (ok)
                        Destroy(_pendingDrag.gameObject);
                    else
                        _pendingDrag.ResetToStart();

                    _pendingDrag = null;
                    _pendingBin = null;
                    yield return new WaitForSecondsRealtime(0.35f);
                }
            }
        }

        void OnDropped(DraggableUI d, DropBinView bin)
        {
            _pendingDrag = d;
            _pendingBin = bin;
        }
    }
}
