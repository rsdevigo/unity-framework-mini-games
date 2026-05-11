using UnityEngine;
using UnityFramework.MiniGames.Audio;

namespace UnityFramework.MiniGames.Data
{
    [CreateAssetMenu(menuName = "Edu/Data/Challenge/Syllable Builder", fileName = "SyllableChallenge")]
    public sealed class SyllableChallengeSO : ChallengeSO
    {
        [SerializeField] string[] _syllablePartsInOrder;
        [SerializeField] Sprite[] _syllableSprites;
        [SerializeField] AudioCueSO _targetWordAudio;

        public string[] SyllablePartsInOrder => _syllablePartsInOrder;
        public Sprite[] SyllableSprites => _syllableSprites;
        public AudioCueSO TargetWordAudio => _targetWordAudio;
    }
}
