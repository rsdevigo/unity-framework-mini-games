using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFramework.MiniGames.Progress
{
    [Serializable]
    public sealed class ConceptStatRow
    {
        public string key;
        public int correct;
        public int wrong;
        public float timeSeconds;
    }

    [Serializable]
    public sealed class GameProgressRow
    {
        public string gameId;
        public int sessionsCompleted;
        public List<ConceptStatRow> concepts = new();
    }

    [Serializable]
    public sealed class ProgressDataV1
    {
        public int schemaVersion = 1;
        public List<GameProgressRow> games = new();
    }
}
