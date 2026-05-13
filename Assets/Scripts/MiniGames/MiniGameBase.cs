//using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityFramework.MiniGames.Core;
using UnityFramework.MiniGames.Core.Events;
using UnityFramework.MiniGames.Data;
using UnityFramework.MiniGames.Hub;
using UnityFramework.MiniGames.UI;

namespace UnityFramework.MiniGames.Gameplay
{
    /// <summary>
    /// Shared lifecycle for additive mini-games. Concrete games override <see cref="RunSessionRoutine"/>.
    /// </summary>
    public abstract class MiniGameBase : MonoBehaviour
    {
        [SerializeField] MiniGameConfigSO _configAsset;
        [SerializeField] string _gameIdOverride;
        [SerializeField] FeedbackKit _feedback;
        [SerializeField] AnswerEvaluatedEventChannelSO _answerEvents;
        [SerializeField] MiniGameSessionEventChannelSO _lifecycleEvents;

        MiniGameContext _context;
        bool _running;

        protected MiniGameContext Context => _context;
        protected MiniGameConfigSO Config => _context != null ? _context.Config : _configAsset;
        protected FeedbackKit Feedback => _feedback;

        void Start()
        {
            if (_running)
                return;
            if (!AppContext.IsInitialized)
            {
                Debug.LogWarning($"{nameof(MiniGameBase)}: AppContext not ready.");
                return;
            }

            if (_configAsset == null)
            {
                Debug.LogWarning($"{nameof(MiniGameBase)}: Missing MiniGameConfigSO on {name}.");
                return;
            }

            var gid = string.IsNullOrEmpty(_gameIdOverride) ? _configAsset.GameId : _gameIdOverride;
            _context = new MiniGameContext(
                gid,
                _configAsset,
                AppContext.Audio,
                AppContext.Input,
                AppContext.Progress,
                AppContext.Localization,
                _answerEvents,
                _lifecycleEvents,
                () => MiniGameSessionHub.RequestExitToHub?.Invoke());

            _running = true;
            OnInitialized();
            _lifecycleEvents?.Raise(gid, "Started");
            StartCoroutine(SessionWrapper());
        }

        void EnsureFeedbackKit()
        {
            if (_feedback != null)
                return;
            var uiDoc = GetComponentInChildren<UIDocument>(true);
            if (uiDoc != null)
                _feedback = uiDoc.gameObject.GetComponent<FeedbackKit>() ?? uiDoc.gameObject.AddComponent<FeedbackKit>();
        }

        IEnumerator SessionWrapper()
        {
            yield return RunSessionRoutine();
            _lifecycleEvents?.Raise(Context.GameId, "Completed");
            Context.Progress.NotifySessionCompleted(Context.GameId);
            MiniGameSessionHub.RequestExitToHub?.Invoke();
        }

        protected virtual void OnInitialized() { }

        protected abstract IEnumerator RunSessionRoutine();

        protected void RaiseAnswerEvaluated(in EvaluationResult result)
        {
            Context.Progress.RecordAnswer(Context.GameId, result.Correct, result.ConceptKeys, result.LatencySeconds);
            _answerEvents?.Raise(new AnswerEvaluatedEvent(Context.GameId, result.Correct, result.ConceptKeys, result.LatencySeconds));
        }

        protected void PlayFeedback(in EvaluationResult result)
        {
            EnsureFeedbackKit();
            if (Feedback != null)
                Feedback.Play(result.Correct, null);
            if (result.Correct && Config.SuccessCue != null)
                Context.Audio.PlaySfxCue(Config.SuccessCue);
            else if (!result.Correct && Config.NeutralRetryCue != null)
                Context.Audio.EnqueueNarration(Config.NeutralRetryCue);
        }
    }
}
