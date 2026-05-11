using UnityEngine;

namespace UnityFramework.MiniGames.Data
{
    [CreateAssetMenu(menuName = "Edu/Data/Story Book", fileName = "StoryBook")]
    public sealed class StoryBookSO : ScriptableObject
    {
        [SerializeField] StoryPageSO[] _pages;

        public StoryPageSO[] Pages => _pages;
    }

    [CreateAssetMenu(menuName = "Edu/Data/Story Page", fileName = "StoryPage")]
    public sealed class StoryPageSO : ScriptableObject
    {
        [SerializeField] Sprite _illustration;
        [SerializeField] MultipleChoiceChallengeSO _gapChallenge;

        public Sprite Illustration => _illustration;
        public MultipleChoiceChallengeSO GapChallenge => _gapChallenge;
    }
}
