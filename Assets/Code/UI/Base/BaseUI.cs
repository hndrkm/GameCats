using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame.UI
{
    public class BaseUI : BaseService
    {
        [SerializeField]
        private UIView[] _defaultViews;
        protected UIView[] _views;
        protected virtual void OnInitializeInternal() { }
        protected virtual void OnDeinitializeInternal() { }
        protected virtual void OnTickInternal() { }
        protected virtual bool OnBackAction() { return false; }
        protected virtual void OnViewOpened(UIView view) { }
        protected virtual void OnViewClosed(UIView view) { }

        public T GetView<T>() where T : UIView
        { 
            if (_views == null)
                return null;
            for (int i = 0; i < _views.Length; i++)
            {
                T view = _views[i] as T;
                if (view != null)
                    return view;
            }
            return null;
        }
        public T OpenView<T>() where T : UIView
        {
            if (_views == null)
                return null;
            for (int i = 0; i < _views.Length; i++)
            {
                T view = _views[i] as T;
                if (view != null) 
                {
                    Open_View(view);
                    return view;
                } 
            }
            return null;
        }
        public void OpenView(UIView view) 
        {
            if (view == null)
                return;
            int index = Array.IndexOf(_views,view);
            if (index < 0)
                return;
            Open_View(view);
        }
        public T CloseView<T>() where T : UIView
        {
            if (_views == null)
                return null;
            for (int i = 0; i < _views.Length; i++)
            {
                T view = _views[i] as T;
                if (view !=  null)
                {
                    view.Close();
                    return view;
                }
            }
            return null;
        }
        public void CloseView(UIView view) 
        {
            if (_views == null)
                return;
            int index = Array.IndexOf(_views,view);
            if (index < 0)
                return;
            Close_View(view);
        }
        protected override void OnInitialize()
        {
            _views = GetComponentsInChildren<UIView>(true);
            for (int i = 0; i < _views.Length; i++)
            {
                UIView view = _views[i];
                view.Initialize(this);
                view.gameObject.SetActive(false);
            }
            OnInitializeInternal();
        }
        protected override void OnDeinitialize()
        {
            OnDeinitializeInternal();
            if (_views != null)
            {
                for (int i = 0; i < _views.Length; i++)
                {
                    _views[i].Deinitialize();
                }
                _views = null; 
            }
        }
        protected override void OnActivate()
        {
            base.OnActivate();
            for (int i = 0; i < _defaultViews.SafeCount(); i++)
            {
                OpenView(_defaultViews[i]);
            }
        }
        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            for (int i = 0; i < _views.SafeCount(); i++)
            {
                CloseView(_views[i]);
            }
        }
        protected override void OnTick()
        {
            if (_views!=null)
            {
                for (int i = 0; i < _views.Length; i++)
                {
                    UIView view = _views[i];
                    if (view.IsOpen == true)
                    {
                        view.Tick();
                    }
                }
            }
            OnTickInternal();
        }
        private void Open_View(UIView view) 
        {
            if (view == null)
                return;
            if (view.IsOpen == true)
                return;
            view.Open_Internal();
            OnViewOpened(view);
        }
        private void Close_View(UIView view)
        {
            if (view == null)
                return;
            if (view.IsOpen == false)
                return;
            view.Close_Internal();
            OnViewClosed(view);
        }
    }
}
