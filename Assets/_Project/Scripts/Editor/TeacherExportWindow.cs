#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityFramework.MiniGames.Progress;

namespace UnityFramework.MiniGames.EditorTools
{
    public sealed class TeacherExportWindow : EditorWindow
    {
        int _slot = 1;
        string _status;

        [MenuItem("Edu Framework/Teacher Export…")]
        public static void Open() => GetWindow<TeacherExportWindow>(true, "Teacher export");

        void OnGUI()
        {
            GUILayout.Label("Exports progress from a local profile slot (same paths as the runtime build).", EditorStyles.wordWrappedLabel);
            _slot = EditorGUILayout.IntSlider("Profile slot", _slot, 1, 30);
            if (GUILayout.Button("Export CSV"))
            {
                var jsonPath = Path.Combine(Application.persistentDataPath, "profiles", $"slot_{_slot:D2}", "progress_v1.json");
                if (!File.Exists(jsonPath))
                {
                    _status = "No progress file at: " + jsonPath;
                    return;
                }

                var json = File.ReadAllText(jsonPath);
                var model = JsonUtility.FromJson<ProgressDataV1>(json);
                if (model == null)
                {
                    _status = "Could not parse JSON.";
                    return;
                }

                var csv = ProgressExportUtility.ToTeacherCsv(model);
                var outPath = EditorUtility.SaveFilePanel("Save teacher CSV", "", $"progress_slot{_slot:D2}.csv", "csv");
                if (!string.IsNullOrEmpty(outPath))
                {
                    File.WriteAllText(outPath, csv);
                    _status = "Saved: " + outPath;
                }
            }

            if (!string.IsNullOrEmpty(_status))
                EditorGUILayout.HelpBox(_status, MessageType.Info);
        }
    }
}
#endif
