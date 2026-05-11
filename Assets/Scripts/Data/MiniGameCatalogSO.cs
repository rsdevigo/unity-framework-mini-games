using System;
using UnityEngine;

namespace UnityFramework.MiniGames.Data
{
    [Serializable]
    public sealed class MiniGameCatalogEntry
    {
        [SerializeField] string _gameId;
        [SerializeField] string _displayNameKey;
        [SerializeField] string _additiveSceneName;
        [SerializeField] BnccTagSO[] _bnccTags;
        [SerializeField] [Range(3, 8)] int _recommendedAgeMin = 4;
        [SerializeField] [Range(3, 12)] int _recommendedAgeMax = 6;

        public string GameId => _gameId;
        public string DisplayNameKey => _displayNameKey;
        public string AdditiveSceneName => _additiveSceneName;
        public BnccTagSO[] BnccTags => _bnccTags;
        public int RecommendedAgeMin => _recommendedAgeMin;
        public int RecommendedAgeMax => _recommendedAgeMax;
    }

    [CreateAssetMenu(menuName = "Edu/Data/Mini Game Catalog", fileName = "MiniGameCatalog")]
    public sealed class MiniGameCatalogSO : ScriptableObject
    {
        [SerializeField] MiniGameCatalogEntry[] _entries;

        public MiniGameCatalogEntry[] Entries => _entries;

        public bool TryFind(string gameId, out MiniGameCatalogEntry entry)
        {
            entry = null;
            if (string.IsNullOrEmpty(gameId) || _entries == null)
                return false;
            foreach (var e in _entries)
            {
                if (e != null && e.GameId == gameId)
                {
                    entry = e;
                    return true;
                }
            }

            return false;
        }
    }
}
