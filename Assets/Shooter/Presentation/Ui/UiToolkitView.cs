using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shooter.Presentation.Ui
{
    public abstract class UiToolkitView : MonoBehaviour, IUiView
    {
        [SerializeField] private UIDocument document;

        protected VisualElement Root => ResolveDocument().rootVisualElement;

        public abstract void Bind(IUiViewModel viewModel);

        public abstract void Unbind();

        protected T QueryRequired<T>(string elementName) where T : VisualElement
        {
            var element = Root.Q<T>(elementName);
            return element ?? throw new InvalidOperationException($"UI element '{elementName}' of type {typeof(T).Name} was not found in {name}.");
        }

        private UIDocument ResolveDocument()
        {
            document ??= GetComponent<UIDocument>();
            return document != null ? document : throw new InvalidOperationException($"{GetType().Name} requires a UIDocument on the same GameObject.");
        }
    }
}
