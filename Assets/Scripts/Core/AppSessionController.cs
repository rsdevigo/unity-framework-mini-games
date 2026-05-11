using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityFramework.MiniGames.Core
{
    /// <summary>
    /// Session orchestration: lab clock, focus pause, and initial navigation after bootstrap.
    /// </summary>
    public sealed class AppSessionController : MonoBehaviour
    {
        [SerializeField] string _hubSceneName = "Hub";
        [SerializeField] bool _loadHubOnStart = true;

        float _sessionClockActive;
        bool _pausedForFocus;

        public float SessionElapsedSeconds => _sessionClockActive;
        public bool IsPausedForFocus => _pausedForFocus;

        public event Action<bool> FocusPauseChanged;

        void Start()
        {
            DontDestroyOnLoad(gameObject);
            if (_loadHubOnStart && !string.IsNullOrWhiteSpace(_hubSceneName))
                StartCoroutine(LoadHubRoutine());
        }

        void OnApplicationFocus(bool hasFocus)
        {
            _pausedForFocus = !hasFocus;
            FocusPauseChanged?.Invoke(_pausedForFocus);
        }

        void Update()
        {
            if (_pausedForFocus)
                return;
            _sessionClockActive += Time.unscaledDeltaTime;
        }

        IEnumerator LoadHubRoutine()
        {
            yield return LoadSceneSingleAsync(_hubSceneName);
        }

        /// <summary>
        /// Loads a scene in single mode (replaces currently active scene stack except DDOL roots).
        /// </summary>
        public static AsyncOperation LoadSceneSingleAsync(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                throw new ArgumentException("Scene name is empty.", nameof(sceneName));
            return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }
    }
}
