#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityFramework.MiniGames.UI;

namespace UnityFramework.MiniGames.EditorTools
{
    /// <summary>
    /// Creates versioned <see cref="PanelSettings"/> and optional UITK foundation test scene (not added to builds by default).
    /// </summary>
    public static class EduUiToolkitFoundationEditor
    {
        public const string DefaultPanelSettingsPath =
            "Assets/EduFramework/UI Toolkit/Resources/EduUI/EduDefaultPanelSettings.asset";

        public const string FoundationTestScenePath = "Assets/Scenes/UITK_FoundationTest.unity";

        [MenuItem("Edu Framework/UI Toolkit/Ensure Default PanelSettings")]
        public static void MenuEnsurePanelSettings() => EnsureDefaultPanelSettingsAsset();

        [MenuItem("Edu Framework/UI Toolkit/Create Foundation Test Scene")]
        public static void MenuCreateFoundationTestScene() => EnsureFoundationTestScene();

        /// <summary>
        /// Idempotent: creates <see cref="PanelSettings"/> aligned to 1920×1080 reference (matches former CanvasScaler intent).
        /// </summary>
        public static void EnsureDefaultPanelSettingsAsset()
        {
            if (AssetDatabase.LoadAssetAtPath<PanelSettings>(DefaultPanelSettingsPath) != null)
                return;
            var dir = Path.GetDirectoryName(DefaultPanelSettingsPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            var ps = ScriptableObject.CreateInstance<PanelSettings>();
            ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            ps.referenceResolution = new Vector2Int(1920, 1080);
            ps.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            ps.match = 0.5f;
            ps.renderMode = PanelRenderMode.ScreenSpaceOverlay;
            AssetDatabase.CreateAsset(ps, DefaultPanelSettingsPath);
            AssetDatabase.SaveAssets();
        }

        public static void EnsureFoundationTestScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(FoundationTestScenePath) != null)
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(FoundationTestScenePath));
                return;
            }

            var sceneDir = Path.GetDirectoryName(FoundationTestScenePath);
            if (!string.IsNullOrEmpty(sceneDir))
                Directory.CreateDirectory(sceneDir);

            var sc = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var go = new GameObject("UITK_FoundationPoc");
            go.AddComponent<UIDocument>();
            go.AddComponent<UiToolkitFoundationPoc>();
            EditorSceneManager.SaveScene(sc, FoundationTestScenePath);
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(FoundationTestScenePath));
        }
    }
}
#endif
