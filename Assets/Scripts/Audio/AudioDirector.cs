using System;
using UnityEngine;
using UnityFramework.MiniGames.Core;

namespace UnityFramework.MiniGames.Audio
{
    /// <summary>
    /// Facade over SFX, narration queue, and optional music ducking.
    /// </summary>
    public sealed class AudioDirector : MonoBehaviour, IAudioDirector
    {
        [SerializeField] [Range(0f, 1f)] float _masterVolume = 1f;
        [SerializeField] [Range(0f, 1f)] float _sfxVolume = 1f;
        [SerializeField] [Range(0f, 1f)] float _narrationVolume = 1f;

        SfxPlayer _sfx;
        NarrationController _narration;
        MusicController _music;

        public event Action NarrationClipFinished;

        void Awake()
        {
            _sfx = GetComponent<SfxPlayer>() ?? gameObject.AddComponent<SfxPlayer>();
            _narration = GetComponent<NarrationController>() ?? gameObject.AddComponent<NarrationController>();
            _music = GetComponent<MusicController>() ?? gameObject.AddComponent<MusicController>();

            _narration.ClipFinished += OnNarrationClipFinished;
            ApplyVolumes();
        }

        void OnDestroy()
        {
            if (_narration != null)
                _narration.ClipFinished -= OnNarrationClipFinished;
        }

        void OnNarrationClipFinished()
        {
            NarrationClipFinished?.Invoke();
            if (!_narration.IsPlaying && !_narration.HasPending)
                _music.SetDucked(false);
        }

        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Mathf.Clamp01(value);
                ApplyVolumes();
            }
        }

        public float SfxVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                ApplyVolumes();
            }
        }

        public float NarrationVolume
        {
            get => _narrationVolume;
            set
            {
                _narrationVolume = Mathf.Clamp01(value);
                ApplyVolumes();
            }
        }

        public bool IsNarrationPlaying => _narration != null && _narration.IsPlaying;

        public void PlayOneShot(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null || _sfx == null)
                return;
            var v = Mathf.Clamp01(volumeScale * _sfxVolume * _masterVolume);
            _sfx.PlayOneShot(clip, v);
        }

        public void PlaySfxCue(AudioCueSO cue)
        {
            if (cue == null || _sfx == null)
                return;
            var v = Mathf.Clamp01(cue.VolumeScale * _sfxVolume * _masterVolume);
            _sfx.PlayOneShot(cue.Clip, v);
        }

        public void EnqueueNarration(AudioCueSO cue)
        {
            if (cue == null || _narration == null)
                return;
            _music.SetDucked(true);
            _narration.Enqueue(cue);
        }

        public void StopNarration() => _narration?.StopAll();

        public void PlayMusic(AudioClip clip) => _music?.Play(clip);

        void ApplyVolumes()
        {
            if (_narration != null)
                _narration.Volume = _narrationVolume * _masterVolume;
        }
    }
}
