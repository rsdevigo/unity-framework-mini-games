using System.Collections.Generic;
using UnityEngine;
using UnityFramework.MiniGames.Audio;

namespace UnityFramework.MiniGames.Data
{
    public abstract class ChallengeSO : ScriptableObject
    {
        [SerializeField] string _id;
        [SerializeField] string[] _conceptKeys = { "generic:demo" };
        [SerializeField] BnccTagSO[] _bnccTags;
        [SerializeField] AudioCueSO _promptNarration;

        public string Id => string.IsNullOrEmpty(_id) ? name : _id;
        public IReadOnlyList<string> ConceptKeys => _conceptKeys;
        public BnccTagSO[] BnccTags => _bnccTags;
        public AudioCueSO PromptNarration => _promptNarration;
    }
}
