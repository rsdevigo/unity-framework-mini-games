using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityFramework.MiniGames.Core;
using UnityFramework.MiniGames.Data;
using UnityFramework.MiniGames.UI;

namespace UnityFramework.MiniGames.Hub
{
    /// <summary>
    /// Interactive hub map: spawns nodes from <see cref="HubConfigurationSO"/> and loads additive mini-game scenes (UI Toolkit).
    /// Node positions use the same convention as the old uGUI map: <see cref="HubMapNode.AnchoredUiPosition"/> is an offset from the <b>center</b> of the map.
    /// </summary>
    public sealed class HubWorldController : MonoBehaviour
    {
        const float HubNodeWidth = 180f;
        const float HubNodeHeight = 180f;

        [SerializeField] HubConfigurationSO _configuration;
        [SerializeField] UIDocument _uiDocument;
        [SerializeField] PanelSettings _panelSettingsOverride;
        [SerializeField] VisualTreeAsset _hubMapUxml;
        [SerializeField] VisualTreeAsset _nodeTemplateUxml;
        [SerializeField] int _sortingOrder = 20;

        VisualElement _mapRoot;
        readonly List<(VisualElement element, Vector2 anchoredOffset)> _hubNodeLayouts = new();
        EventCallback<GeometryChangedEvent> _mapGeometryHandler;
        bool _mapGeometryRegistered;

        void Start()
        {
            GameplayUiUtility.EnsureEventSystem();
            EnsureUiDocument();
            MiniGameSessionHub.RequestExitToHub = HandleExitToHub;
            if (_configuration != null)
                BuildMap();
        }

        void OnDestroy()
        {
            if (_mapRoot != null && _mapGeometryRegistered && _mapGeometryHandler != null)
            {
                _mapRoot.UnregisterCallback(_mapGeometryHandler);
                _mapGeometryRegistered = false;
            }

            if (MiniGameSessionHub.RequestExitToHub == HandleExitToHub)
                MiniGameSessionHub.RequestExitToHub = null;
        }

        void EnsureUiDocument()
        {
            if (_uiDocument == null)
                _uiDocument = GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
            _uiDocument.sortingOrder = _sortingOrder;
            if (_panelSettingsOverride != null)
                _uiDocument.panelSettings = _panelSettingsOverride;
            else
                EduUiToolkitDefaults.ApplyTo(_uiDocument);
            if (_hubMapUxml == null)
                _hubMapUxml = EduUiToolkitDefaults.LoadVisualTree("HubMap");
            if (_nodeTemplateUxml == null)
                _nodeTemplateUxml = EduUiToolkitDefaults.LoadVisualTree("HubNodeTemplate");
            if (_hubMapUxml == null)
            {
                Debug.LogError(
                    "HubWorldController: VisualTreeAsset 'HubMap' not found. Run **Edu Framework → UI Toolkit → Ensure Default PanelSettings** and confirm UXML exists under Assets/.../Resources/EduUI/HubMap.uxml, then reimport.");
                return;
            }

            if (_nodeTemplateUxml == null)
                Debug.LogWarning("HubWorldController: 'HubNodeTemplate' not found — map nodes cannot spawn.");

            _uiDocument.visualTreeAsset = _hubMapUxml;
            _mapRoot = _uiDocument.rootVisualElement.Q("map-root");
        }

        void RegisterMapGeometryForLayout()
        {
            if (_mapRoot == null || _mapGeometryRegistered)
                return;
            _mapGeometryHandler = OnMapRootGeometryChanged;
            _mapRoot.RegisterCallback(_mapGeometryHandler);
            _mapGeometryRegistered = true;
        }

        void OnMapRootGeometryChanged(GeometryChangedEvent evt)
        {
            if (evt.target != _mapRoot)
                return;
            ApplyHubNodePositions();
        }

        void ApplyHubNodePositions()
        {
            if (_mapRoot == null || _hubNodeLayouts.Count == 0)
                return;
            var w = _mapRoot.layout.width;
            var h = _mapRoot.layout.height;
            if (w < 2f || h < 2f)
                return;

            var cx = w * 0.5f;
            var cy = h * 0.5f;
            foreach (var (el, pos) in _hubNodeLayouts)
            {
                if (el == null)
                    continue;
                el.style.position = Position.Absolute;
                // Same data as old uGUI (center anchor): +X right, +Y up. UITK: +top is downward.
                el.style.left = cx + pos.x - HubNodeWidth * 0.5f;
                el.style.top = cy - pos.y - HubNodeHeight * 0.5f;
                el.style.right = StyleKeyword.Auto;
                el.style.bottom = StyleKeyword.Auto;
                el.style.translate = new Translate(0f, 0f);
            }
        }

        void BuildMap()
        {
            EnsureUiDocument();
            if (_mapRoot == null)
                _mapRoot = _uiDocument.rootVisualElement.Q("map-root");
            if (_mapRoot == null || _configuration.Nodes == null || _nodeTemplateUxml == null)
                return;

            _hubNodeLayouts.Clear();
            _mapRoot.Clear();

            foreach (var node in _configuration.Nodes)
            {
                if (string.IsNullOrEmpty(node.LinkedGameId))
                    continue;

                var unlocked = node.UnlockRule == null ||
                               node.UnlockRule.IsUnlocked(AppContext.Progress);

                var el = _nodeTemplateUxml.CloneTree();
                var label = el.Q<Label>("node-label");
                var key = _configuration.Catalog != null && _configuration.Catalog.TryFind(node.LinkedGameId, out var entry)
                    ? entry.DisplayNameKey
                    : node.LinkedGameId;
                if (label != null)
                    label.text = AppContext.Localization.Get(key, node.LinkedGameId);

                if (!unlocked)
                {
                    el.style.opacity = 0.65f;
                    el.pickingMode = PickingMode.Ignore;
                    el.style.backgroundColor = new Color(0.55f, 0.55f, 0.55f);
                }
                else
                {
                    el.style.backgroundColor = new Color(0.35f, 0.75f, 0.45f);
                    el.pickingMode = PickingMode.Position;
                    var gid = node.LinkedGameId;
                    el.RegisterCallback<ClickEvent>(_ => StartCoroutine(LoadMiniGameRoutine(gid)));
                }

                _mapRoot.Add(el);
                _hubNodeLayouts.Add((el, node.AnchoredUiPosition));
            }

            RegisterMapGeometryForLayout();
            ApplyHubNodePositions();
            _mapRoot.schedule.Execute(ApplyHubNodePositions).ExecuteLater(0);
        }

        void HandleExitToHub() => StartCoroutine(UnloadMiniGamesRoutine());

        IEnumerator UnloadMiniGamesRoutine()
        {
            for (var i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var s = SceneManager.GetSceneAt(i);
                if (s.name.StartsWith("MG_", System.StringComparison.Ordinal))
                    yield return SceneManager.UnloadSceneAsync(s);
            }

            if (_uiDocument != null && _uiDocument.rootVisualElement != null)
                _uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            if (AppContext.IsInitialized)
                AppContext.Input.SetGameplayInputEnabled(true);

            ApplyHubNodePositions();
            if (_mapRoot != null)
                _mapRoot.schedule.Execute(ApplyHubNodePositions).ExecuteLater(0);
        }

        IEnumerator LoadMiniGameRoutine(string gameId)
        {
            if (_configuration?.Catalog == null || !_configuration.Catalog.TryFind(gameId, out var entry))
                yield break;

            if (_uiDocument != null && _uiDocument.rootVisualElement != null)
                _uiDocument.rootVisualElement.style.display = DisplayStyle.None;
            if (AppContext.IsInitialized)
                AppContext.Input.SetGameplayInputEnabled(true);

            MiniGameSessionHub.RequestExitToHub = HandleExitToHub;
            var op = SceneManager.LoadSceneAsync(entry.AdditiveSceneName, LoadSceneMode.Additive);
            yield return op;
        }
    }
}
