using UnityEngine;
using UnityEngine.UIElements;

namespace UnityFramework.MiniGames.UI
{
    /// <summary>
    /// Phase S proof-of-concept: loads <c>FoundationPoc.uxml</c> (title + mock back). Safe to drop in any scene; does not depend on uGUI.
    /// </summary>
    public sealed class UiToolkitFoundationPoc : MonoBehaviour
    {
        [SerializeField] UIDocument _document;

        void OnEnable()
        {
            if (_document == null)
                _document = GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
            EduUiToolkitDefaults.ApplyTo(_document);
            if (_document.visualTreeAsset == null)
                _document.visualTreeAsset = EduUiToolkitDefaults.LoadVisualTree("FoundationPoc");
            _document.sortingOrder = 5;
            var root = _document.rootVisualElement;
            root.Q<Button>("btn-back-mock")?.RegisterCallback<ClickEvent>(_ =>
                Debug.Log("UITK Foundation PoC: mock back clicked."));
        }
    }
}
