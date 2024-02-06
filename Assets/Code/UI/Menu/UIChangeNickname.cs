using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CatGame.UI
{
    public class UIChangeNickname : UICloseView
    {
        [SerializeField]
        private TextMeshProUGUI _caption;
        [SerializeField]
        private TMP_InputField _name;
        [SerializeField]
        private Button _confirmBtn;
        [SerializeField]
        private int _minCaracteres;

        public void SetData(string caption, bool nameRequierd) 
        {
            _caption.text = caption;
            CloseButton.gameObject.SetActive(nameRequierd==false);
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _confirmBtn.onClick.AddListener(OnConfrimBtn);
        }
        protected override void OnDeinitialize()
        {
            _confirmBtn.onClick.RemoveListener(OnConfrimBtn);
            base.OnDeinitialize();
        }
        protected override void OnOpen()
        {
            base.OnOpen();
            string currentNickname =  Context.PlayerData.Nickname;
            if (currentNickname.HasValue() == false) 
            {
                _name.text = "Jugador"+Random.Range(10000,100000);
            }
            else 
            {
                _name.text = Context.PlayerData.Nickname;
            }
        }
        protected override void OnTick()
        {
            base.OnTick();
            _confirmBtn.interactable = _name.text.Length >= _minCaracteres && _name.text != Context.PlayerData.Nickname;
        }
        private void OnConfrimBtn() 
        {
            Context.PlayerData.Nickname = _name.text;
            Close();
        }
    }
}
