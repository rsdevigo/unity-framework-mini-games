using System;

namespace UnityFramework.MiniGames.Core
{
    /// <summary>
    /// Application-wide service access after bootstrap registration.
    /// </summary>
    public static class AppContext
    {
        static AppServices _services;

        public static bool IsInitialized { get; private set; }

        public static IAudioDirector Audio => Require().Audio;
        public static IInputRouter Input => Require().Input;
        public static IProgressService Progress => Require().Progress;
        public static ILocalizationService Localization => Require().Localization;

        public static void Initialize(AppServices services)
        {
            if (IsInitialized)
                throw new InvalidOperationException("AppContext is already initialized.");
            _services = services;
            IsInitialized = true;
        }

        public static void Clear()
        {
            _services = default;
            IsInitialized = false;
        }

        static AppServices Require()
        {
            if (!IsInitialized)
                throw new InvalidOperationException("AppContext has not been initialized.");
            return _services;
        }
    }
}
