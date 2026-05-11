using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityFramework.MiniGames.Core;
using UnityFramework.MiniGames.Data;

namespace UnityFramework.MiniGames.Progress
{
    public sealed class ProgressService : MonoBehaviour, IProgressService
    {
        const string ProgressFileName = "progress_v1.json";

        [SerializeField] int _profileSlot = 1;
        [SerializeField] string _profileSubFolder = "profiles";

        string _rootPath;
        ProgressDataV1 _model = new();

        public int ProfileSlot
        {
            get => _profileSlot;
            set
            {
                _profileSlot = Mathf.Clamp(value, 1, 30);
                RebuildRootPath();
            }
        }

        public string ProfileRootPath => _rootPath;

        void Awake()
        {
            RebuildRootPath();
            TryLoadProgressModel();
        }

        void RebuildRootPath()
        {
            var baseDir = Path.Combine(Application.persistentDataPath, _profileSubFolder, $"slot_{_profileSlot:D2}");
            _rootPath = baseDir;
        }

        public void SaveTextAtomic(string relativePath, string contents)
        {
            if (string.IsNullOrEmpty(relativePath))
                return;
            RebuildRootPath();
            Directory.CreateDirectory(_rootPath);
            var full = Path.Combine(_rootPath, relativePath);
            var dir = Path.GetDirectoryName(full);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            var temp = full + ".tmp";
            File.WriteAllText(temp, contents ?? string.Empty);
            if (File.Exists(full))
                File.Delete(full);
            File.Move(temp, full);
        }

        public bool TryLoadText(string relativePath, out string contents)
        {
            contents = null;
            if (string.IsNullOrEmpty(relativePath))
                return false;
            RebuildRootPath();
            var full = Path.Combine(_rootPath, relativePath);
            if (!File.Exists(full))
                return false;
            contents = File.ReadAllText(full);
            return true;
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public int GetCompletedSessions(string miniGameId)
        {
            var row = FindGame(miniGameId);
            return row?.sessionsCompleted ?? 0;
        }

        public void RecordAnswer(string miniGameId, bool correct, string[] conceptKeys, float latencySeconds)
        {
            if (string.IsNullOrEmpty(miniGameId))
                return;
            var row = FindOrCreateGame(miniGameId);
            var keys = conceptKeys == null || conceptKeys.Length == 0
                ? new[] { $"{miniGameId}:session" }
                : conceptKeys;
            foreach (var key in keys)
            {
                if (string.IsNullOrEmpty(key))
                    continue;
                var c = FindOrCreateConcept(row, key);
                if (correct)
                    c.correct++;
                else
                    c.wrong++;
                c.timeSeconds += Math.Max(0f, latencySeconds);
            }
        }

        public void NotifySessionCompleted(string miniGameId)
        {
            var row = FindOrCreateGame(miniGameId);
            row.sessionsCompleted++;
            SaveProgressModel();
        }

        public string ExportProgressJson() => JsonUtility.ToJson(_model, true);

        public void ImportProgressJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return;
            try
            {
                var parsed = JsonUtility.FromJson<ProgressDataV1>(json);
                if (parsed != null && parsed.games != null)
                    _model = parsed;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Progress import failed: {e.Message}");
            }
        }

        public void SaveProgressModel() => SaveTextAtomic(ProgressFileName, ExportProgressJson());

        public bool TryLoadProgressModel()
        {
            if (!TryLoadText(ProgressFileName, out var json))
                return false;
            ImportProgressJson(json);
            return true;
        }

        GameProgressRow FindGame(string gameId) =>
            _model.games?.FirstOrDefault(g => g.gameId == gameId);

        GameProgressRow FindOrCreateGame(string gameId)
        {
            _model.games ??= new List<GameProgressRow>();
            var row = FindGame(gameId);
            if (row != null)
                return row;
            row = new GameProgressRow { gameId = gameId, sessionsCompleted = 0, concepts = new List<ConceptStatRow>() };
            _model.games.Add(row);
            return row;
        }

        static ConceptStatRow FindOrCreateConcept(GameProgressRow row, string key)
        {
            foreach (var c in row.concepts)
            {
                if (c.key == key)
                    return c;
            }

            var n = new ConceptStatRow { key = key };
            row.concepts.Add(n);
            return n;
        }

        /// <summary>Teacher-friendly CSV: one row per concept aggregate.</summary>
        public string ExportTeacherCsv() => ProgressExportUtility.ToTeacherCsv(_model);
    }
}
