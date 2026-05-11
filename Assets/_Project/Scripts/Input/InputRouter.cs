using UnityEngine;
using UnityEngine.InputSystem;
using UnityFramework.MiniGames.Core;

namespace UnityFramework.MiniGames.Input
{
    /// <summary>
    /// Semantic pointer / click / cancel from the Input System (UI map when an asset is assigned).
    /// </summary>
    public sealed class InputRouter : MonoBehaviour, IInputRouter
    {
        [SerializeField] InputActionAsset _inputActions;

        InputActionMap _uiMap;
        InputAction _point;
        InputAction _click;
        InputAction _cancel;

        Vector2 _lastPointer;
        bool _lastPressed;
        bool _gameplayEnabled = true;

        public bool IsReady { get; private set; }

        public Vector2 PointerScreenPosition { get; private set; }

        public bool PrimaryClickPressedThisFrame { get; private set; }
        public bool PrimaryClickReleasedThisFrame { get; private set; }
        public bool PrimaryPressed { get; private set; }
        public Vector2 DragScreenDelta { get; private set; }
        public bool CancelPressedThisFrame { get; private set; }

        void OnEnable()
        {
            if (_inputActions != null)
            {
                _uiMap = _inputActions.FindActionMap("UI", true);
                _point = _uiMap.FindAction("Point", true);
                _click = _uiMap.FindAction("Click", true);
                _cancel = _uiMap.FindAction("Cancel", true);
                _uiMap.Enable();
            }

            IsReady = true;
        }

        void OnDisable()
        {
            if (_uiMap != null)
                _uiMap.Disable();
            IsReady = false;
        }

        public void SetGameplayInputEnabled(bool enabled) => _gameplayEnabled = enabled;

        void Update()
        {
            PrimaryClickPressedThisFrame = false;
            PrimaryClickReleasedThisFrame = false;
            CancelPressedThisFrame = false;
            DragScreenDelta = Vector2.zero;

            if (!_gameplayEnabled)
            {
                PointerScreenPosition = Vector2.zero;
                PrimaryPressed = false;
                return;
            }

            if (_point != null && _click != null)
            {
                PointerScreenPosition = _point.ReadValue<Vector2>();
                var pressed = _click.IsPressed();
                PrimaryClickPressedThisFrame = pressed && !_lastPressed;
                PrimaryClickReleasedThisFrame = !pressed && _lastPressed;
                PrimaryPressed = pressed;
                if (pressed)
                    DragScreenDelta = PointerScreenPosition - _lastPointer;
                _lastPointer = PointerScreenPosition;
                _lastPressed = pressed;
                if (_cancel != null)
                    CancelPressedThisFrame = _cancel.WasPressedThisFrame();
                return;
            }

            FallbackFromDevices();
        }

        void FallbackFromDevices()
        {
            if (Mouse.current != null)
            {
                PointerScreenPosition = Mouse.current.position.ReadValue();
                var pressed = Mouse.current.leftButton.isPressed;
                PrimaryClickPressedThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
                PrimaryClickReleasedThisFrame = Mouse.current.leftButton.wasReleasedThisFrame;
                PrimaryPressed = pressed;
                if (pressed)
                    DragScreenDelta = PointerScreenPosition - _lastPointer;
                _lastPointer = PointerScreenPosition;
                _lastPressed = pressed;
            }
            else if (Touchscreen.current != null)
            {
                var t = Touchscreen.current.primaryTouch;
                PointerScreenPosition = t.position.ReadValue();
                var pressed = t.press.isPressed;
                PrimaryClickPressedThisFrame = t.press.wasPressedThisFrame;
                PrimaryClickReleasedThisFrame = t.press.wasReleasedThisFrame;
                PrimaryPressed = pressed;
                if (pressed)
                    DragScreenDelta = PointerScreenPosition - _lastPointer;
                _lastPointer = PointerScreenPosition;
                _lastPressed = pressed;
            }

            if (Keyboard.current != null)
                CancelPressedThisFrame = Keyboard.current.escapeKey.wasPressedThisFrame;
        }
    }
}
