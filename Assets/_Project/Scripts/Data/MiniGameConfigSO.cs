using UnityEngine;
using UnityFramework.MiniGames.Audio;

namespace UnityFramework.MiniGames.Data
{
    [CreateAssetMenu(menuName = "Edu/Data/Mini Game Config", fileName = "MiniGameConfig")]
    public sealed class MiniGameConfigSO : ScriptableObject
    {
        [SerializeField] string _gameId = "demo";
        [SerializeField] ChallengeSetSO _challengeSet;
        [SerializeField] DifficultyProfileSO _difficulty;
        [SerializeField] AudioCueSO _successCue;
        [SerializeField] AudioCueSO _neutralRetryCue;
        [SerializeField] StoryBookSO _storyBook;

        public string GameId => _gameId;
        public ChallengeSetSO ChallengeSet => _challengeSet;
        public DifficultyProfileSO Difficulty => _difficulty;
        public AudioCueSO SuccessCue => _successCue;
        public AudioCueSO NeutralRetryCue => _neutralRetryCue;
        public StoryBookSO StoryBook => _storyBook;
    }
}
