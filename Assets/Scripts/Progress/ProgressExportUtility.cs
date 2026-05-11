using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityFramework.MiniGames.Progress
{
    public static class ProgressExportUtility
    {
        public static string ToTeacherCsv(ProgressDataV1 model)
        {
            var lines = new List<string> { "gameId,conceptKey,correct,wrong,timeSeconds,sessionsCompleted" };
            foreach (var g in model.games ?? Enumerable.Empty<GameProgressRow>())
            {
                foreach (var c in g.concepts ?? Enumerable.Empty<ConceptStatRow>())
                    lines.Add($"{Escape(g.gameId)},{Escape(c.key)},{c.correct},{c.wrong},{c.timeSeconds:0.###},{g.sessionsCompleted}");
            }

            return string.Join("\n", lines);
        }

        static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";
            if (s.Contains(',') || s.Contains('"'))
                return $"\"{s.Replace("\"", "\"\"")}\"";
            return s;
        }
    }
}
