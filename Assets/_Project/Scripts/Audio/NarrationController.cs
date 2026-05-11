using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFramework.MiniGames.Audio
{
    /// <summary>
    /// Single-voice narration with priority interrupts and optional same-tier queueing.
    /// </summary>
    public sealed class NarrationController : MonoBehaviour
    {
        [SerializeField] [Range(0f, 1f)] float _volume = 1f;

        readonly Queue<AudioCueSO> _queue = new();
        AudioSource _voice;
        AudioCueSO _playing;
        Coroutine _playRoutine;
        readonly Dictionary<AudioCueSO, float> _lastPlayed = new();

        public bool IsPlaying => _voice != null && _voice.isPlaying;
        public AudioCueSO CurrentCue => _playing;
        public bool HasPending => _queue.Count > 0;

        public event Action ClipFinished;

        void Awake()
        {
            _voice = gameObject.AddComponent<AudioSource>();
            _voice.playOnAwake = false;
            _voice.loop = false;
            _voice.spatialBlend = 0f;
        }

        public float Volume
        {
            get => _volume;
            set => _volume = Mathf.Clamp01(value);
        }

        public void Enqueue(AudioCueSO cue)
        {
            if (cue == null || cue.Clip == null)
                return;

            if (cue.CooldownSeconds > 0f &&
                _lastPlayed.TryGetValue(cue, out var t) &&
                Time.unscaledTime - t < cue.CooldownSeconds)
                return;

            var incoming = (int)cue.Priority;

            if (_playing != null)
            {
                var current = (int)_playing.Priority;
                if (incoming > current)
                {
                    _queue.Clear();
                    StopActive();
                    StartCue(cue);
                    return;
                }

                if (incoming == current)
                {
                    if (cue.SameTierQueues)
                        _queue.Enqueue(cue);
                    else
                    {
                        StopActive();
                        StartCue(cue);
                    }

                    return;
                }

                _queue.Enqueue(cue);
                return;
            }

            StartCue(cue);
        }

        public void StopAll()
        {
            _queue.Clear();
            StopActive();
        }

        void StartCue(AudioCueSO cue)
        {
            _playing = cue;
            if (_playRoutine != null)
                StopCoroutine(_playRoutine);
            _playRoutine = StartCoroutine(PlayRoutine(cue));
        }

        IEnumerator PlayRoutine(AudioCueSO cue)
        {
            _voice.Stop();
            _voice.clip = cue.Clip;
            _voice.volume = Volume * Mathf.Clamp01(cue.VolumeScale);
            _voice.Play();
            _lastPlayed[cue] = Time.unscaledTime;

            while (_voice.isPlaying)
                yield return null;

            _playing = null;
            _playRoutine = null;
            ClipFinished?.Invoke();

            if (_queue.Count > 0)
            {
                var next = _queue.Dequeue();
                StartCue(next);
            }
        }

        void StopActive()
        {
            if (_playRoutine != null)
            {
                StopCoroutine(_playRoutine);
                _playRoutine = null;
            }

            if (_voice != null)
                _voice.Stop();
            _playing = null;
        }
    }
}
