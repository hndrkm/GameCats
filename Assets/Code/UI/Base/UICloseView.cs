using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CatGame.UI
{
    public class UICloseView : UIView
    {
        public UIView BackView { get; set; }
        public Button CloseButton => _closeButton;

        [SerializeField]
        private Button _closeButton;
        public void CloseWithBack()
        {
            OnCloseButton();
        }
        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (_closeButton != null) 
            {
                _closeButton.onClick.AddListener(OnCloseButton);
            }
        }
        protected override void OnDeinitialize()
        {
            
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(OnCloseButton);
            }
            base.OnDeinitialize();
        }
        protected virtual void OnCloseButton() 
        {
            Close();
            if (BackView != null)
            {
                OpenView(BackView);
                BackView = null;
            }
        }
    }
}
