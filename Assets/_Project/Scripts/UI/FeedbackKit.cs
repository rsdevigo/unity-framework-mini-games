using System.Collections;
using UnityEngine;

namespace UnityFramework.MiniGames.UI
{
    /// <summary>
    /// Short, non-punitive visual feedback (scale pulse). Pair with audio from the active mini-game.
    /// </summary>
    public sealed class FeedbackKit : MonoBehaviour
    {
        [SerializeField] float _pulseSeconds = 0.35f;
        [SerializeField] float _correctScale = 1.08f;
        [SerializeField] float _wrongScale = 0.96f;

        Coroutine _routine;

        public void Play(bool correct, RectTransform target)
        {
            if (_routine != null)
                StopCoroutine(_routine);
            var rt = target != null ? target : GetComponent<RectTransform>();
            if (rt == null)
                return;
            _routine = StartCoroutine(Pulse(rt, correct));
        }

        IEnumerator Pulse(RectTransform rt, bool correct)
        {
            var baseScale = rt.localScale;
            var peak = correct ? _correctScale : _wrongScale;
            var t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / Mathf.Max(0.01f, _pulseSeconds);
                var s = Mathf.Lerp(1f, peak, Mathf.Sin(Mathf.PI * Mathf.Clamp01(t)));
                rt.localScale = baseScale * s;
                yield return null;
            }

            rt.localScale = baseScale;
            _routine = null;
        }
    }
}
