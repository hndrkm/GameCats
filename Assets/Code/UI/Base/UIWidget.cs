using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame.UI
{
    public abstract class UIWidget : MonoBehaviour
    {


        public bool IsVisible { get; private set; }

        // PROTECTED MEMBERS

        protected bool IsInitalized { get; private set; }
        protected BaseUI SceneUI { get; private set; }
        protected BaseContext Context { get { return SceneUI.Context; } }
        protected UIWidget Owner { get; private set; }



        private List<UIWidget> _children = new List<UIWidget>(16);


        internal void Initialize(BaseUI sceneUI, UIWidget owner)
        {
            if (IsInitalized == true)
                return;

            SceneUI = sceneUI;
            Owner = owner;

            _children.Clear();
            GetChildWidgets(transform, _children);

            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].Initialize(sceneUI, this);
            }

            OnInitialize();

            IsInitalized = true;

            if (gameObject.activeInHierarchy == true)
            {
                Visible();
            }
        }

        internal void Deinitialize()
        {
            if (IsInitalized == false)
                return;

            Hidden();

            OnDeinitialize();

            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].Deinitialize();
            }

            _children.Clear();

            IsInitalized = false;

            SceneUI = null;
            Owner = null;
        }

        internal void Visible()
        {
            if (IsInitalized == false)
                return;

            if (IsVisible == true)
                return;

            if (gameObject.activeSelf == false)
                return;

            IsVisible = true;

            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].Visible();
            }

            OnVisible();
        }

        internal void Hidden()
        {
            if (IsVisible == false)
                return;

            IsVisible = false;

            OnHidden();

            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].Hidden();
            }
        }

        internal void Tick()
        {
            if (IsInitalized == false)
                return;

            if (IsVisible == false)
                return;

            OnTick();

            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].Tick();
            }
        }

        internal void AddChild(UIWidget widget)
        {
            if (widget == null || widget == this)
                return;

            if (_children.Contains(widget) == true)
            {
                Debug.LogError($"Widget {widget.name} is already added as child of {name}");
                return;
            }

            _children.Add(widget);

            widget.Initialize(SceneUI, this);
        }

        internal void RemoveChild(UIWidget widget)
        {
            int childIndex = _children.IndexOf(widget);

            if (childIndex < 0)
            {
                Debug.LogError($"Widget {widget.name} is not child of {name} and cannot be removed");
                return;
            }

            widget.Deinitialize();

            _children.RemoveAt(childIndex);
        }

        // MONOBEHAVIOR

        protected void OnEnable()
        {
            Visible();
        }

        protected void OnDisable()
        {
            Hidden();
        }

        // UIWidget INTERFACE

        public virtual bool IsActive() { return true; }

        protected virtual void OnInitialize() { }
        protected virtual void OnDeinitialize() { }
        protected virtual void OnVisible() { }
        protected virtual void OnHidden() { }
        protected virtual void OnTick() { }

        // PRIVATE MEMBERS

        private static void GetChildWidgets(Transform transform, List<UIWidget> widgets)
        {
            foreach (Transform child in transform)
            {
                var childWidget = child.GetComponent<UIWidget>();

                if (childWidget != null)
                {
                    widgets.Add(childWidget);
                }
                else
                {
                    // Continue searching deeper in hierarchy
                    GetChildWidgets(child, widgets);
                }
            }
        }
    }
}
