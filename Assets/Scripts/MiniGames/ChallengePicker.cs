using System.Collections.Generic;
using UnityEngine;
using UnityFramework.MiniGames.Data;

namespace UnityFramework.MiniGames.Gameplay
{
    public static class ChallengePicker
    {
        public static ChallengeSO PickRandom(IReadOnlyList<ChallengeSO> challenges)
        {
            if (challenges == null || challenges.Count == 0)
                return null;
            return challenges[Random.Range(0, challenges.Count)];
        }
    }
}
