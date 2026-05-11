using System.Collections.Generic;
using UnityEngine;
using UnityFramework.MiniGames.Core;
using UnityFramework.MiniGames.Data;

namespace UnityFramework.MiniGames.Localization
{
    public sealed class LocalizationService : MonoBehaviour, ILocalizationService
    {
        [SerializeField] string _defaultLanguageId = "pt-BR";
        [SerializeField] LocalizedTableSO[] _tables;

        readonly Dictionary<string, string> _fallbackTable = new();

        public string CurrentLanguageId { get; set; }

        void Awake()
        {
            CurrentLanguageId = string.IsNullOrEmpty(_defaultLanguageId) ? "pt-BR" : _defaultLanguageId;
            SeedMinimalFallbacks();
        }

        void SeedMinimalFallbacks()
        {
            _fallbackTable["app.hub.title"] = "Mapa";
            _fallbackTable["mg.consonants"] = "Sons — consoantes";
            _fallbackTable["mg.counting"] = "Contar";
            _fallbackTable["mg.memory"] = "Memória sonora";
            _fallbackTable["mg.story"] = "História com lacunas";
            _fallbackTable["mg.classify"] = "Classificar";
            _fallbackTable["mg.syllables"] = "Sílabas";
        }

        public string Get(string key, string fallback = null)
        {
            if (TryGet(key, out var value))
                return value;
            return string.IsNullOrEmpty(fallback) ? key : fallback;
        }

        public bool TryGet(string key, out string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                value = null;
                return false;
            }

            if (_fallbackTable.TryGetValue(key, out value))
                return true;

            if (_tables != null)
            {
                foreach (var t in _tables)
                {
                    if (t != null && t.TryGet(CurrentLanguageId, key, out value))
                        return true;
                }
            }

            value = null;
            return false;
        }
    }
}
