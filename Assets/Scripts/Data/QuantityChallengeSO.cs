using UnityEngine;
using UnityFramework.MiniGames.Audio;

namespace UnityFramework.MiniGames.Data
{
    [CreateAssetMenu(menuName = "Edu/Data/Challenge/Quantity", fileName = "QuantityChallenge")]
    public sealed class QuantityChallengeSO : ChallengeSO
    {
        [SerializeField] [Min(0)] int _targetCount = 3;
        [SerializeField] Sprite _tokenSprite;
        [SerializeField] AudioCueSO _countNarrationTemplate;
        [SerializeField] AudioClip[] _numberClips;

        public int TargetCount => _targetCount;
        public Sprite TokenSprite => _tokenSprite;
        public AudioCueSO CountNarrationTemplate => _countNarrationTemplate;

        public AudioClip GetNumberClip(int n)
        {
            if (_numberClips == null || _numberClips.Length == 0)
                return null;
            var i = Mathf.Clamp(n - 1, 0, _numberClips.Length - 1);
            return _numberClips[i];
        }
    }
}
