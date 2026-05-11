using System.Collections.Generic;
using UnityEngine;
using UnityFramework.MiniGames.Audio;

namespace UnityFramework.MiniGames.Data
{
    [CreateAssetMenu(menuName = "Edu/Data/Challenge/Multiple Choice", fileName = "MCChallenge")]
    public sealed class MultipleChoiceChallengeSO : ChallengeSO
    {
        [SerializeField] Sprite _stimulusImage;
        [SerializeField] Sprite[] _optionImages;
        [SerializeField] string[] _optionIds;
        [SerializeField] int _correctIndex;
        [SerializeField] AudioCueSO[] _optionHoverAudio;

        public Sprite StimulusImage => _stimulusImage;
        public IReadOnlyList<Sprite> OptionImages => _optionImages;
        public IReadOnlyList<string> OptionIds => _optionIds;
        public int CorrectIndex => _correctIndex;
        public AudioCueSO GetHoverAudio(int index) =>
            _optionHoverAudio != null && index >= 0 && index < _optionHoverAudio.Length
                ? _optionHoverAudio[index]
                : null;
    }
}
