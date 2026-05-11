using System.Collections.Generic;
using UnityEngine;

namespace UnityFramework.MiniGames.Audio
{
    /// <summary>
    /// Short feedback clips using a small pooled <see cref="AudioSource"/> set.
    /// </summary>
    public sealed class SfxPlayer : MonoBehaviour
    {
        [SerializeField] int _poolSize = 8;

        readonly List<AudioSource> _pool = new();
        int _next;

        void Awake()
        {
            for (var i = 0; i < _poolSize; i++)
            {
                var s = gameObject.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.loop = false;
                s.spatialBlend = 0f;
                _pool.Add(s);
            }
        }

        public void PlayOneShot(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null || _pool.Count == 0)
                return;
            var src = _pool[_next];
            _next = (_next + 1) % _pool.Count;
            src.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
        }

        public void PlayCue(AudioCueSO cue)
        {
            if (cue == null || cue.Clip == null)
                return;
            PlayOneShot(cue.Clip, cue.VolumeScale);
        }
    }
}
