using UnityEngine;
using UnityFramework.MiniGames.Core;

namespace UnityFramework.MiniGames.Data
{
    public abstract class UnlockRuleSO : ScriptableObject
    {
        public abstract bool IsUnlocked(IProgressReader progress);
    }

    [CreateAssetMenu(menuName = "Edu/Data/Unlock/Always Unlocked", fileName = "Unlock_Always")]
    public sealed class AlwaysUnlockedRuleSO : UnlockRuleSO
    {
        public override bool IsUnlocked(IProgressReader progress) => true;
    }

    [CreateAssetMenu(menuName = "Edu/Data/Unlock/Require Completed Games", fileName = "Unlock_RequireGames")]
    public sealed class RequireMiniGamesCompletedRuleSO : UnlockRuleSO
    {
        [SerializeField] string[] _requiredGameIds;

        public override bool IsUnlocked(IProgressReader progress)
        {
            if (progress == null || _requiredGameIds == null)
                return true;
            foreach (var id in _requiredGameIds)
            {
                if (string.IsNullOrEmpty(id))
                    continue;
                if (progress.GetCompletedSessions(id) < 1)
                    return false;
            }

            return true;
        }
    }
}
