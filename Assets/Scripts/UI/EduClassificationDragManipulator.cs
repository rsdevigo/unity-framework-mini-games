using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityFramework.MiniGames.UI
{
    /// <summary>
    /// Drags a token in panel space; on release, picks a parent with class <c>edu-drop-bin</c> and <see cref="VisualElement.userData"/> as category id.
    /// </summary>
    public sealed class EduClassificationDragManipulator : PointerManipulator
    {
        readonly Action<VisualElement, string> _onReleasedOnBin;
        Vector2 _grabOffset;
        float _homeLeft;
        float _homeTop;

        public EduClassificationDragManipulator(Action<VisualElement, string> onReleasedOnBin)
        {
            _onReleasedOnBin = onReleasedOnBin;
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnDown);
            target.RegisterCallback<PointerMoveEvent>(OnMove);
            target.RegisterCallback<PointerUpEvent>(OnUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnDown);
            target.UnregisterCallback<PointerMoveEvent>(OnMove);
            target.UnregisterCallback<PointerUpEvent>(OnUp);
        }

        void OnDown(PointerDownEvent e)
        {
            if (!CanStartManipulation(e))
                return;
            var parent = target.hierarchy.parent;
            if (parent == null)
                return;
            target.CapturePointer(e.pointerId);
            target.BringToFront();
            var local = parent.WorldToLocal(e.position);
            var layout = target.layout;
            _homeLeft = layout.x;
            _homeTop = layout.y;
            _grabOffset = local - new Vector2(layout.x, layout.y);
            target.style.position = Position.Absolute;
            target.style.left = layout.x;
            target.style.top = layout.y;
            e.StopPropagation();
        }

        void OnMove(PointerMoveEvent e)
        {
            if (!target.HasPointerCapture(e.pointerId))
                return;
            var parent = target.hierarchy.parent;
            if (parent == null)
                return;
            var local = parent.WorldToLocal(e.position);
            var pos = local - _grabOffset;
            target.style.left = pos.x;
            target.style.top = pos.y;
            e.StopPropagation();
        }

        void OnUp(PointerUpEvent e)
        {
            if (!target.HasPointerCapture(e.pointerId))
                return;
            target.ReleasePointer(e.pointerId);
            var panel = target.panel;
            var binCategory = string.Empty;
            if (panel != null)
            {
                var picked = panel.Pick(e.position);
                for (var ve = picked; ve != null; ve = ve.hierarchy.parent)
                {
                    if (ve.ClassListContains("edu-drop-bin") && ve.userData is string s)
                    {
                        binCategory = s;
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(binCategory))
                _onReleasedOnBin?.Invoke(target, binCategory);
            else
                EduDragDropVisuals.ResetTokenHome(target, _homeLeft, _homeTop);
            e.StopPropagation();
        }
    }

    static class EduDragDropVisuals
    {
        public static void ResetTokenHome(VisualElement token, float left, float top)
        {
            token.style.left = left;
            token.style.top = top;
        }
    }
}
