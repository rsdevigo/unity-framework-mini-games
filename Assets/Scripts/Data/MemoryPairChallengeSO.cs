using UnityEngine;
using UnityFramework.MiniGames.Audio;

namespace UnityFramework.MiniGames.Data
{
    [CreateAssetMenu(menuName = "Edu/Data/Challenge/Memory Pair", fileName = "MemoryPair")]
    public sealed class MemoryPairChallengeSO : ChallengeSO
    {
        [SerializeField] string _pairId;
        [SerializeField] AudioCueSO _cardFaceA;
        [SerializeField] AudioCueSO _cardFaceB;

        public string PairId => _pairId;
        public AudioCueSO CardFaceA => _cardFaceA;
        public AudioCueSO CardFaceB => _cardFaceB;
    }
}
