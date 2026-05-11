using System;
using UnityFramework.MiniGames.Audio;
using UnityFramework.MiniGames.Core;
using UnityFramework.MiniGames.Core.Events;
using UnityFramework.MiniGames.Data;

namespace UnityFramework.MiniGames.Gameplay
{
    public sealed class MiniGameContext
    {
        public MiniGameContext(
            string gameId,
            MiniGameConfigSO config,
            IAudioDirector audio,
            IInputRouter input,
            IProgressService progress,
            ILocalizationService localization,
            AnswerEvaluatedEventChannelSO answers,
            MiniGameSessionEventChannelSO lifecycle,
            Action requestExitToHub)
        {
            GameId = gameId;
            Config = config;
            Audio = audio;
            Input = input;
            Progress = progress;
            Localization = localization;
            Answers = answers;
            Lifecycle = lifecycle;
            RequestExitToHub = requestExitToHub ?? (() => { });
        }

        public string GameId { get; }
        public MiniGameConfigSO Config { get; }
        public IAudioDirector Audio { get; }
        public IInputRouter Input { get; }
        public IProgressService Progress { get; }
        public ILocalizationService Localization { get; }
        public AnswerEvaluatedEventChannelSO Answers { get; }
        public MiniGameSessionEventChannelSO Lifecycle { get; }
        public Action RequestExitToHub { get; }
    }
}
