using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace UnityFramework.MiniGames.UI
{
    public static class GameplayUiUtility
    {
        static Font _builtinRuntimeFont;

        /// <summary>
        /// Built-in font for runtime-created uGUI <see cref="Text"/> (Unity 6+: use LegacyRuntime.ttf; Arial.ttf was removed).
        /// </summary>
        public static Font BuiltinRuntimeFont =>
            _builtinRuntimeFont ??= Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null)
                return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
            Object.DontDestroyOnLoad(go);
        }

        public static Canvas CreateOverlayCanvas(string name, Transform parent = null)
        {
            var root = new GameObject(name);
            if (parent != null)
                root.transform.SetParent(parent, false);
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static Button CreateChoiceButton(Transform parent, string label, Sprite image)
        {
            var go = new GameObject("Choice", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(220, 220);
            var img = go.GetComponent<Image>();
            img.sprite = image;
            img.color = image != null ? Color.white : new Color(0.85f, 0.9f, 1f);
            var btn = go.GetComponent<Button>();
            var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var trt = (RectTransform)textGo.transform;
            trt.anchorMin = new Vector2(0, 0);
            trt.anchorMax = new Vector2(1, 0.25f);
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            var tx = textGo.GetComponent<Text>();
            tx.font = BuiltinRuntimeFont;
            tx.alignment = TextAnchor.MiddleCenter;
            tx.resizeTextForBestFit = true;
            tx.resizeTextMinSize = 10;
            tx.resizeTextMaxSize = 32;
            tx.text = label;
            tx.color = Color.black;
            return btn;
        }
    }
}
