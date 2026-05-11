using System;
using UnityEngine;

namespace UnityFramework.MiniGames.Data
{
    [Serializable]
    public sealed class ClassifiableItem
    {
        public Sprite sprite;
        public string categoryId;
    }

    [CreateAssetMenu(menuName = "Edu/Data/Challenge/Classification", fileName = "ClassificationChallenge")]
    public sealed class ClassificationChallengeSO : ChallengeSO
    {
        [SerializeField] ClassifiableItem[] _items;
        [SerializeField] string[] _binLabels;

        public ClassifiableItem[] Items => _items;
        public string[] BinLabels => _binLabels;
    }
}
