using System.Collections;
using UnityEngine;

namespace UnityFramework.MiniGames.Audio
{
    /// <summary>
    /// Looping bed music with simple volume ducking while narration plays.
    /// </summary>
    public sealed class MusicController : MonoBehaviour
    {
        [SerializeField] [Range(0f, 1f)] float _musicVolume = 0.35f;
        
        [SerializeField] [Range(0f, 1f)] float _duckedVolume = 0.12f;
        [SerializeField] float _duckFadeSeconds = 0.25f;

        AudioSource _source;
        Coroutine _duckRoutine;
        float _current = 1f;

        public bool IsDucked { get; private set; }

        void Awake()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = true;
            _source.spatialBlend = 0f;
            _source.volume = _musicVolume;
        }

        public void Play(AudioClip clip)
        {
            if (clip == null)
                return;
            _source.clip = clip;
            _source.volume = _musicVolume * _current;
            _source.Play();
        }

        public void Stop() => _source.Stop();

        public void SetDucked(bool ducked)
        {
            if (IsDucked == ducked)
                return;
            IsDucked = ducked;
            if (_duckRoutine != null)
                StopCoroutine(_duckRoutine);
            _duckRoutine = StartCoroutine(DuckRoutine(ducked ? _duckedVolume : _musicVolume));
        }

        IEnumerator DuckRoutine(float target)
        {
            var start = _source.volume;
            var t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / Mathf.Max(0.01f, _duckFadeSeconds);
                _source.volume = Mathf.Lerp(start, target, t);
                yield return null;
            }

            _source.volume = target;
            _duckRoutine = null;
        }
    }
}
