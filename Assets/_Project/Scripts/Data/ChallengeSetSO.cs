using System.Collections.Generic;
using UnityEngine;

namespace UnityFramework.MiniGames.Data
{
    [CreateAssetMenu(menuName = "Edu/Data/Challenge Set", fileName = "ChallengeSet")]
    public sealed class ChallengeSetSO : ScriptableObject
    {
        [SerializeField] List<ChallengeSO> _challenges = new();

        public IReadOnlyList<ChallengeSO> Challenges => _challenges;
    }
}
