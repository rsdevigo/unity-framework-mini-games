using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityFramework.MiniGames.Data;
using UnityFramework.MiniGames.UI;

namespace UnityFramework.MiniGames.Gameplay
{
    /// <summary>
    /// Generic multiple-choice rounds driven entirely by <see cref="MultipleChoiceChallengeSO"/> assets.
    /// </summary>
    public sealed class MiniGameMultipleChoice : MiniGameBase
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
            var set = Config.ChallengeSet;
            if (set == null || set.Challenges == null || set.Challenges.Count == 0)
                yield break;

            var diff = Config.Difficulty;
            var rounds = diff != null ? diff.Rounds : 3;
            var wrongStreak = 0;

            for (var r = 0; r < rounds; r++)
            {
                var ch = ChallengePicker.PickRandom(set.Challenges);
                if (ch is not MultipleChoiceChallengeSO mc)
                    continue;

                var total = Mathf.Max(mc.OptionImages?.Count ?? 0, mc.OptionIds?.Count ?? 0);
                if (total <= 0)
                    continue;

                var maxChoices = diff != null ? diff.ChoiceCountForStreak(wrongStreak) : total;
                maxChoices = Mathf.Clamp(maxChoices, 2, total);

                int? picked = null;
                _shell.Bind(mc, maxChoices, i => picked = i);

                if (mc.PromptNarration != null)
                    Context.Audio.EnqueueNarration(mc.PromptNarration);

                var sw = Stopwatch.StartNew();
                while (!picked.HasValue)
                    yield return null;
                sw.Stop();

                var ok = picked.Value == mc.CorrectIndex;
                wrongStreak = ok ? 0 : wrongStreak + 1;

                string[] keys;
                if (mc.ConceptKeys.Count > 0)
                {
                    keys = new string[mc.ConceptKeys.Count];
                    for (var i = 0; i < keys.Length; i++)
                        keys[i] = mc.ConceptKeys[i];
                }
                else
                    keys = new[] { $"{Context.GameId}:mc" };

                var result = new EvaluationResult(ok, keys, (float)sw.Elapsed.TotalSeconds);
                RaiseAnswerEvaluated(result);
                PlayFeedback(result);
                yield return new WaitForSecondsRealtime(0.45f);
            }
        }
    }
}
