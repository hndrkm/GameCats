using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame.UI
{
    public class UIView: MonoBehaviour
    {
        public event Action HasOpened;
        public event Action HasClosed;

        public bool IsOpen { get; private set; }
        public bool IsVisible {  get; private set; }
        protected bool IsInitalized { get; private set; }
        protected BaseUI BaseUI {  get; private set; }
        protected BaseContext Context { get { return BaseUI.Context; } }
        public void Open() 
        {
            BaseUI.OpenView(this);
        }
        public void Close() 
        {
            if (BaseUI == null)
            {
                Close_Internal();
            }
            else 
            { 
                BaseUI.CloseView(this);
            }
        }
        protected T OpenView<T>() where T : UIView
        {
            return BaseUI.OpenView<T>();
        }
        protected void OpenView(UIView view)
        {
            BaseUI.OpenView(view);
        }

        internal void Open_Internal()
        {
            if (IsOpen == true)
                return;
            IsOpen = true;
            gameObject.SetActive(true);
            OnOpen();
            if (HasOpened != null)
            {
                HasOpened();
                HasOpened = null;
            }
        }
        internal void Close_Internal()
        {
            if (IsOpen == false)
                return;
            IsOpen = false;
            OnClose();
            gameObject.SetActive(false);            
            if (HasClosed != null)
            {
                HasClosed();
                HasClosed = null;
            }
        }

        internal void Initialize(BaseUI baseUI) 
        {
            if (IsInitalized == true)
                return;
            BaseUI =baseUI;
            OnInitialize();
            IsInitalized = true;
            if (gameObject.activeInHierarchy == true)
                Visible();
        }
        internal void Deinitialize() 
        {
            if (IsInitalized == false)
                return;
            
            OnDeinitialize();
            IsInitalized = false;
            BaseUI = null;

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
            OnVisible();
        }
        internal void Hidden() 
        {
            if (IsVisible == false)
                return;
            IsVisible = false;
            OnHiden();

        }
        internal void Tick() 
        {
            if (IsInitalized == false)
                return;
            if (IsVisible == false)
                return;
            OnTick();
        }
        protected void OnEnable()
        {
            Visible();
        }
        protected void OnDisable()
        {
            Hidden();
        }
        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }
        protected virtual void OnInitialize() { }
        protected void OnDeinitialize() 
        {
            Close_Internal();
            HasOpened = null; 
            HasClosed = null;
        }
        protected virtual void OnVisible () { }
        protected virtual void OnHiden() { }
        protected virtual void OnTick() { }
    }
}
