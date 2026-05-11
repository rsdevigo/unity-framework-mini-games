using UnityEngine;

namespace UnityFramework.MiniGames.Data
{
    [CreateAssetMenu(menuName = "Edu/Data/BNCC Tag", fileName = "BnccTag")]
    public sealed class BnccTagSO : ScriptableObject
    {
        [SerializeField] string _officialCode;
        [SerializeField] string _displayName;

        public string OfficialCode => _officialCode;
        public string DisplayName => _displayName;
    }
}
