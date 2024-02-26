namespace CatGame.UI
{
    using System;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class UIAgentItem : MonoBehaviour
    {
        public int ID { get; set; }

        public bool IsSelectable => _button != null;
        public bool IsSelected { get { return _isSelected; } set { SetIsSelected(value); } }
        public bool IsInteractable { get { return GetIsInteractable(); } set { SetIsInteractable(value); } }
        public Action<int> Cliked;

        [SerializeField]
        private Button _button;
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private string _selectedAnimatorParameter = "IsSelected";
        [SerializeField]
        private CanvasGroup _selectedGroup;
        [SerializeField]
        private CanvasGroup _deselectedGroup;

        [SerializeField]
        private TextMeshProUGUI _nameAgent;
        [SerializeField]
        private Image _iconAgent;


        private bool _isSelected;

        public void SetData(AgentSetup agent)
        {
            _nameAgent.text = agent.DisplayName;
            _iconAgent.sprite = agent.Icon;
            
        }
        protected virtual void Awake()
        {
            SetIsSelected(false, true);

            if (_button != null)
            {
                _button.onClick.AddListener(OnClick);
            }

            if (_button != null && _button.transition == Selectable.Transition.Animation)
            {
                _animator = _button.animator;
            }
        }

        protected virtual void OnDestroy()
        {
            Cliked = null;

            if (_button != null)
            {
                _button.onClick.RemoveListener(OnClick);
            }
        }
        private void SetIsSelected(bool value, bool force = false)
        {
            if (_isSelected == value && force == false)
                return;

            _isSelected = value;

            _selectedGroup.alpha = value == true ? 1f : 0;
            _selectedGroup.interactable = value;
            _selectedGroup.blocksRaycasts = value;
            value = value == false;
            _deselectedGroup.alpha = value == true ? 1f : 0;
            _deselectedGroup.interactable = value;
            _deselectedGroup.blocksRaycasts = value;

            UpdateAnimator();
        }

        private bool GetIsInteractable()
        {
            return _button != null ? _button.interactable : false;
        }

        private void SetIsInteractable(bool value)
        {
            if (_button == null)
                return;

            _button.interactable = value;
        }

        private void OnClick()
        {
            Cliked?.Invoke(ID);
        }

        private void UpdateAnimator()
        {
            if (_animator == null)
                return;

            if (_selectedAnimatorParameter.HasValue() == false)
                return;

            if (_animator != null)
            {
                _animator.SetBool(_selectedAnimatorParameter, _isSelected);
            }
        }
    }
}
