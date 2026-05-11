using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFramework.MiniGames.Data
{
    [Serializable]
    public sealed class LocalizedRow
    {
        public string key;
        public string ptBR;
        [Tooltip("Optional: e.g. indigenous language text when available.")]
        public string secondary;
    }

    [CreateAssetMenu(menuName = "Edu/Data/Localized Table", fileName = "LocalizedTable")]
    public sealed class LocalizedTableSO : ScriptableObject
    {
        [SerializeField] List<LocalizedRow> _rows = new();

        readonly Dictionary<string, LocalizedRow> _cache = new();

        void OnEnable() => Rebuild();

        public void Rebuild()
        {
            _cache.Clear();
            foreach (var r in _rows)
            {
                if (string.IsNullOrEmpty(r.key))
                    continue;
                _cache[r.key] = r;
            }
        }

        public bool TryGet(string languageId, string key, out string value)
        {
            value = null;
            if (string.IsNullOrEmpty(key))
                return false;
            if (_cache.Count == 0)
                Rebuild();
            if (!_cache.TryGetValue(key, out var row))
                return false;

            if (languageId != null && languageId.StartsWith("pt", StringComparison.OrdinalIgnoreCase))
            {
                value = row.ptBR;
                return !string.IsNullOrEmpty(value);
            }

            value = string.IsNullOrEmpty(row.secondary) ? row.ptBR : row.secondary;
            return !string.IsNullOrEmpty(value);
        }
    }
}
