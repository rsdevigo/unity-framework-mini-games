using System;
using UnityEngine;

namespace UnityFramework.MiniGames.Core.Events
{
    public readonly struct MiniGameSessionEvent
    {
        public MiniGameSessionEvent(string gameId, string phase)
        {
            GameId = gameId;
            Phase = phase;
        }

        public string GameId { get; }
        public string Phase { get; }
    }

    [CreateAssetMenu(menuName = "Edu/Events/MiniGame Session Event", fileName = "MiniGameSessionEvent")]
    public sealed class MiniGameSessionEventChannelSO : ScriptableObject
    {
        public event Action<MiniGameSessionEvent> Raised;

        public void Raise(string gameId, string phase) =>
            Raised?.Invoke(new MiniGameSessionEvent(gameId, phase));
    }
}
