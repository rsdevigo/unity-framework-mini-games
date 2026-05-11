using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityFramework.MiniGames.Core;
using UnityFramework.MiniGames.Data;
using UnityFramework.MiniGames.UI;

namespace UnityFramework.MiniGames.Hub
{
    /// <summary>
    /// Interactive hub map: spawns nodes from <see cref="HubConfigurationSO"/> and loads additive mini-game scenes.
    /// </summary>
    public sealed class HubWorldController : MonoBehaviour
    {
        [SerializeField] HubConfigurationSO _configuration;
        [SerializeField] RectTransform _mapRoot;
        [SerializeField] Canvas _hubCanvas;
        [SerializeField] int _canvasSortOrder = 20;

        void Start()
        {
            GameplayUiUtility.EnsureEventSystem();
            EnsureCanvas();
            MiniGameSessionHub.RequestExitToHub = HandleExitToHub;
            if (_configuration != null)
                BuildMap();
        }

        void OnDestroy()
        {
            if (MiniGameSessionHub.RequestExitToHub == HandleExitToHub)
                MiniGameSessionHub.RequestExitToHub = null;
        }

        void EnsureCanvas()
        {
            if (_hubCanvas != null)
            {
                _hubCanvas.sortingOrder = _canvasSortOrder;
                if (_mapRoot == null)
                    _mapRoot = _hubCanvas.GetComponentInChildren<RectTransform>();
                return;
            }

            _hubCanvas = GameplayUiUtility.CreateOverlayCanvas("HubCanvas", null);
            _hubCanvas.sortingOrder = _canvasSortOrder;
            _mapRoot = new GameObject("MapRoot", typeof(RectTransform)).GetComponent<RectTransform>();
            _mapRoot.SetParent(_hubCanvas.transform, false);
            var rt = _mapRoot;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        void BuildMap()
        {
            foreach (Transform c in _mapRoot)
                Destroy(c.gameObject);

            if (_configuration.Nodes == null)
                return;

            foreach (var node in _configuration.Nodes)
            {
                if (string.IsNullOrEmpty(node.LinkedGameId))
                    continue;

                var unlocked = node.UnlockRule == null ||
                                 node.UnlockRule.IsUnlocked(AppContext.Progress);

                var go = new GameObject($"node_{node.NodeId}", typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(_mapRoot, false);
                var irt = (RectTransform)go.transform;
                irt.sizeDelta = new Vector2(180, 180);
                irt.anchoredPosition = node.AnchoredUiPosition;
                var img = go.GetComponent<Image>();
                img.color = unlocked ? new Color(0.35f, 0.75f, 0.45f) : new Color(0.55f, 0.55f, 0.55f);
                var btn = go.GetComponent<Button>();
                btn.interactable = unlocked;
                var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
                labelGo.transform.SetParent(go.transform, false);
                var lrt = (RectTransform)labelGo.transform;
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.offsetMin = Vector2.zero;
                lrt.offsetMax = Vector2.zero;
                var tx = labelGo.GetComponent<Text>();
                tx.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                tx.alignment = TextAnchor.MiddleCenter;
                tx.color = Color.white;
                tx.resizeTextForBestFit = true;
                tx.resizeTextMinSize = 10;
                tx.resizeTextMaxSize = 28;
                var key = _configuration.Catalog != null && _configuration.Catalog.TryFind(node.LinkedGameId, out var entry)
                    ? entry.DisplayNameKey
                    : node.LinkedGameId;
                tx.text = AppContext.Localization.Get(key, node.LinkedGameId);

                var gid = node.LinkedGameId;
                btn.onClick.AddListener(() => StartCoroutine(LoadMiniGameRoutine(gid)));
            }
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

            if (_hubCanvas != null)
                _hubCanvas.enabled = true;
            if (AppContext.IsInitialized)
                AppContext.Input.SetGameplayInputEnabled(true);
        }

        IEnumerator LoadMiniGameRoutine(string gameId)
        {
            if (_configuration?.Catalog == null || !_configuration.Catalog.TryFind(gameId, out var entry))
                yield break;

            if (_hubCanvas != null)
                _hubCanvas.enabled = false;
            if (AppContext.IsInitialized)
                AppContext.Input.SetGameplayInputEnabled(true);

            MiniGameSessionHub.RequestExitToHub = HandleExitToHub;
            var op = SceneManager.LoadSceneAsync(entry.AdditiveSceneName, LoadSceneMode.Additive);
            yield return op;
        }
    }
}
