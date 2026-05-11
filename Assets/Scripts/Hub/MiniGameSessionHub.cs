using System;

namespace UnityFramework.MiniGames.Hub
{
    /// <summary>
    /// Lightweight bridge so additive mini-game scenes can request return to the hub without a hard scene reference.
    /// </summary>
    public static class MiniGameSessionHub
    {
        public static Action RequestExitToHub;
    }
}
