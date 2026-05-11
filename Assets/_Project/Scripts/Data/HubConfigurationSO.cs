using System;
using UnityEngine;

namespace UnityFramework.MiniGames.Data
{
    [Serializable]
    public sealed class HubMapNode
    {
        [SerializeField] string _nodeId;
        [SerializeField] string _linkedGameId;
        [SerializeField] Vector2 _anchoredUiPosition;
        [SerializeField] UnlockRuleSO _unlockRule;

        public string NodeId => _nodeId;
        public string LinkedGameId => _linkedGameId;
        public Vector2 AnchoredUiPosition => _anchoredUiPosition;
        public UnlockRuleSO UnlockRule => _unlockRule;
    }

    [CreateAssetMenu(menuName = "Edu/Data/Hub Configuration", fileName = "HubConfiguration")]
    public sealed class HubConfigurationSO : ScriptableObject
    {
        [SerializeField] MiniGameCatalogSO _catalog;
        [SerializeField] HubMapNode[] _nodes;

        public MiniGameCatalogSO Catalog => _catalog;
        public HubMapNode[] Nodes => _nodes;
    }
}
