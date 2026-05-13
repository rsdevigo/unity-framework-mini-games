using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityFramework.MiniGames.UI
{
    /// <summary>
    /// Short visual feedback: scale pulse on a <see cref="VisualElement"/> (UI Toolkit).
    /// </summary>
    public sealed class FeedbackKit : MonoBehaviour
    {
        [SerializeField] float _pulseSeconds = 0.35f;
        [SerializeField] float _correctScale = 1.08f;
        [SerializeField] float _wrongScale = 0.96f;

        UIDocument _document;
        VisualElement _fallbackRoot;
        Coroutine _routine;

        void Awake()
        {
            _document = GetComponent<UIDocument>() ?? GetComponentInParent<UIDocument>();
            ResolveFallback();
        }

        void OnEnable() => ResolveFallback();

        void ResolveFallback()
        {
            if (_document != null && _document.rootVisualElement != null)
            {
                _fallbackRoot = _document.rootVisualElement.Q(className: "edu-feedback-root")
                                ?? _document.rootVisualElement;
            }
        }

        /// <summary>Play pulse on UITK element (or fallback root).</summary>
        public void Play(bool correct, VisualElement target)
        {
            if (_routine != null)
                StopCoroutine(_routine);
            var ve = target != null ? target : _fallbackRoot;
            if (ve == null)
                return;
            _routine = StartCoroutine(PulseVe(ve, correct));
        }

        IEnumerator PulseVe(VisualElement ve, bool correct)
        {
            var peak = correct ? _correctScale : _wrongScale;
            var t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / Mathf.Max(0.01f, _pulseSeconds);
                var s = Mathf.Lerp(1f, peak, Mathf.Sin(Mathf.PI * Mathf.Clamp01(t)));
                ve.style.scale = new Scale(new Vector2(s, s));
                yield return null;
            }

            ve.style.scale = new Scale(Vector2.one);
            _routine = null;
        }
    }
}
