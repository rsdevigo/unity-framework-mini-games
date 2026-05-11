using UnityEngine;

namespace UnityFramework.MiniGames.Data
{
    [CreateAssetMenu(menuName = "Edu/Data/Difficulty Profile", fileName = "DifficultyProfile")]
    public sealed class DifficultyProfileSO : ScriptableObject
    {
        [SerializeField] [Min(1)] int _rounds = 5;
        [SerializeField] [Min(0)] int _wrongStreakBeforeSimplify = 2;
        [SerializeField] [Min(2)] int _maxChoiceCount = 4;
        [SerializeField] [Min(2)] int _minChoiceCount = 2;

        public int Rounds => _rounds;
        public int WrongStreakBeforeSimplify => _wrongStreakBeforeSimplify;
        public int MaxChoiceCount => _maxChoiceCount;
        public int MinChoiceCount => _minChoiceCount;

        public int ChoiceCountForStreak(int wrongStreak)
        {
            if (wrongStreak >= _wrongStreakBeforeSimplify)
                return Mathf.Max(_minChoiceCount, _maxChoiceCount - 1);
            return _maxChoiceCount;
        }
    }
}
