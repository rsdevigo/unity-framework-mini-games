using UnityEngine;
using UnityFramework.MiniGames.Audio;
using UnityFramework.MiniGames.Input;
using UnityFramework.MiniGames.Localization;
using UnityFramework.MiniGames.Progress;

namespace UnityFramework.MiniGames.Core
{
    /// <summary>
    /// Registers core services into <see cref="AppContext"/> before other gameplay systems wake.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class BootstrapScope : MonoBehaviour
    {
        void Awake()
        {
            var audio = GetComponent<AudioDirector>();
            var input = GetComponent<InputRouter>();
            var progress = GetComponent<ProgressService>();
            var localization = GetComponent<LocalizationService>();

            AppContext.Initialize(new AppServices(audio, input, progress, localization));
        }

        void OnDestroy()
        {
            if (AppContext.IsInitialized)
                AppContext.Clear();
        }
    }
}
