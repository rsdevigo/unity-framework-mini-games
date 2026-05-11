using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityFramework.MiniGames.Audio;

namespace UnityFramework.MiniGames.Core
{
    public interface IAudioDirector
    {
        float MasterVolume { get; set; }
        float SfxVolume { get; set; }
        float NarrationVolume { get; set; }

        /// <summary>Legacy one-shot on SFX bus.</summary>
        void PlayOneShot(AudioClip clip, float volumeScale = 1f);

        void PlaySfxCue(AudioCueSO cue);
        void EnqueueNarration(AudioCueSO cue);
        void StopNarration();

        bool IsNarrationPlaying { get; }
        event Action NarrationClipFinished;
    }

    public interface IInputRouter
    {
        bool IsReady { get; }
        Vector2 PointerScreenPosition { get; }
        bool PrimaryClickPressedThisFrame { get; }
        bool PrimaryClickReleasedThisFrame { get; }
        bool PrimaryPressed { get; }
        Vector2 DragScreenDelta { get; }
        bool CancelPressedThisFrame { get; }
        void SetGameplayInputEnabled(bool enabled);
    }

    public interface IProgressReader
    {
        int GetCompletedSessions(string miniGameId);
    }

    public interface IProgressService : IProgressReader
    {
        int ProfileSlot { get; set; }
        string ProfileRootPath { get; }
        void SaveTextAtomic(string relativePath, string contents);
        bool TryLoadText(string relativePath, out string contents);
        Task FlushAsync(CancellationToken cancellationToken = default);

        void RecordAnswer(string miniGameId, bool correct, string[] conceptKeys, float latencySeconds);
        void NotifySessionCompleted(string miniGameId);
        string ExportProgressJson();
        void ImportProgressJson(string json);
        void SaveProgressModel();
        bool TryLoadProgressModel();
        string ExportTeacherCsv();
    }

    public interface ILocalizationService
    {
        string CurrentLanguageId { get; set; }
        string Get(string key, string fallback = null);
        bool TryGet(string key, out string value);
    }

    /// <summary>
    /// Immutable bundle registered into <see cref="AppContext"/> at bootstrap.
    /// </summary>
    public readonly struct AppServices
    {
        public AppServices(
            IAudioDirector audio,
            IInputRouter input,
            IProgressService progress,
            ILocalizationService localization)
        {
            Audio = audio ?? throw new ArgumentNullException(nameof(audio));
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Progress = progress ?? throw new ArgumentNullException(nameof(progress));
            Localization = localization ?? throw new ArgumentNullException(nameof(localization));
        }

        public IAudioDirector Audio { get; }
        public IInputRouter Input { get; }
        public IProgressService Progress { get; }
        public ILocalizationService Localization { get; }
    }
}
