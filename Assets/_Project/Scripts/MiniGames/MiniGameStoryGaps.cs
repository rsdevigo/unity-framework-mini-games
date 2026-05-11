using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityFramework.MiniGames.Data;
using UnityFramework.MiniGames.UI;

namespace UnityFramework.MiniGames.Gameplay
{
    /// <summary>
    /// Story pages with embedded multiple-choice gaps (<see cref="StoryPageSO"/>).
    /// </summary>
    public sealed class MiniGameStoryGaps : MiniGameBase
    {
        MultipleChoiceShellView _shell;

        protected override void OnInitialized()
        {
            GameplayUiUtility.EnsureEventSystem();
            _shell = GetComponent<MultipleChoiceShellView>();
            if (_shell == null)
                _shell = gameObject.AddComponent<MultipleChoiceShellView>();
        }

        protected override IEnumerator RunSessionRoutine()
        {
            var book = Config.StoryBook;
            if (book?.Pages == null || book.Pages.Length == 0)
                yield break;

            var diff = Config.Difficulty;
            var maxChoices = diff != null ? diff.MaxChoiceCount : 4;

            foreach (var page in book.Pages)
            {
                if (page?.GapChallenge == null)
                    continue;

                var mc = page.GapChallenge;
                int? picked = null;
                _shell.Bind(mc, maxChoices, i => picked = i);

                if (mc.PromptNarration != null)
                    Context.Audio.EnqueueNarration(mc.PromptNarration);

                var sw = Stopwatch.StartNew();
                while (!picked.HasValue)
                    yield return null;
                sw.Stop();

                var ok = picked.Value == mc.CorrectIndex;
                string[] keys;
                if (mc.ConceptKeys.Count > 0)
                {
                    keys = new string[mc.ConceptKeys.Count];
                    for (var i = 0; i < keys.Length; i++)
                        keys[i] = mc.ConceptKeys[i];
                }
                else
                    keys = new[] { $"{Context.GameId}:story" };

                var result = new EvaluationResult(ok, keys, (float)sw.Elapsed.TotalSeconds);
                RaiseAnswerEvaluated(result);
                PlayFeedback(result);
                yield return new WaitForSecondsRealtime(0.45f);
            }
        }
    }
}
