using System;
using UnityEngine;

namespace UnityFramework.MiniGames.Core.Events
{
    public readonly struct AnswerEvaluatedEvent
    {
        public AnswerEvaluatedEvent(string miniGameId, bool correct, string[] conceptKeys, float latencySeconds)
        {
            MiniGameId = miniGameId;
            Correct = correct;
            ConceptKeys = conceptKeys ?? Array.Empty<string>();
            LatencySeconds = latencySeconds;
        }

        public string MiniGameId { get; }
        public bool Correct { get; }
        public string[] ConceptKeys { get; }
        public float LatencySeconds { get; }
    }

    [CreateAssetMenu(menuName = "Edu/Events/Answer Evaluated", fileName = "AnswerEvaluatedEvent")]
    public sealed class AnswerEvaluatedEventChannelSO : ScriptableObject
    {
        public event Action<AnswerEvaluatedEvent> Raised;

        public void Raise(in AnswerEvaluatedEvent payload) => Raised?.Invoke(payload);
    }
}
