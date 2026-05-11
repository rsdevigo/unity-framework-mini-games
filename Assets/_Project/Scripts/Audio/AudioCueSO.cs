using UnityEngine;

namespace UnityFramework.MiniGames.Audio
{
    [CreateAssetMenu(menuName = "Edu/Audio/Audio Cue", fileName = "AudioCue")]
    public sealed class AudioCueSO : ScriptableObject
    {
        [SerializeField] AudioClip _clip;
        [SerializeField] [Range(0f, 1f)] float _volumeScale = 1f;
        [SerializeField] AudioPriority _priority = AudioPriority.Tutorial;
        [SerializeField] bool _sameTierQueues = true;
        [SerializeField] bool _blockInputUntilDone;
        [SerializeField] float _cooldownSeconds;
        [SerializeField] string _subtitleLocalizationKey;

        public AudioClip Clip => _clip;
        public float VolumeScale => _volumeScale;
        public AudioPriority Priority => _priority;
        public bool SameTierQueues => _sameTierQueues;
        public bool BlockInputUntilDone => _blockInputUntilDone;
        public float CooldownSeconds => _cooldownSeconds;
        public string SubtitleLocalizationKey => _subtitleLocalizationKey;
    }
}
