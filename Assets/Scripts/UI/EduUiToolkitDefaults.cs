using UnityEngine;
using UnityEngine.UIElements;

namespace UnityFramework.MiniGames.UI
{
    /// <summary>
    /// Shared UI Toolkit defaults (PanelSettings + resource paths under Resources/EduUI/).
    /// </summary>
    public static class EduUiToolkitDefaults
    {
        public const string ResourcesFolderPrefix = "EduUI/";

        static PanelSettings _cachedPanelSettings;

        public static PanelSettings PanelSettings
        {
            get
            {
                if (_cachedPanelSettings != null)
                    return _cachedPanelSettings;
                _cachedPanelSettings = Resources.Load<PanelSettings>($"{ResourcesFolderPrefix}EduDefaultPanelSettings");
                return _cachedPanelSettings;
            }
        }

        public static void ApplyTo(UIDocument doc)
        {
            if (doc == null)
                return;
            var ps = PanelSettings;
            if (ps != null)
                doc.panelSettings = ps;
        }

        public static VisualTreeAsset LoadVisualTree(string nameWithoutPath)
        {
            return Resources.Load<VisualTreeAsset>($"{ResourcesFolderPrefix}{nameWithoutPath}");
        }
    }
}
