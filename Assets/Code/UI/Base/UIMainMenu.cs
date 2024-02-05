using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CatGame.UI
{
    public class UIMainMenu : UIView
    {
        [SerializeField]
        private Button _btnStart;
        [SerializeField]
        private Button _btnChangeNickname;
        [SerializeField]
        private Button _btnQuit;
        [SerializeField]
        private Button _btnPlayer;
        [SerializeField]
        private Button _btnSettings;
        [SerializeField]
        private TextMeshProUGUI _txtAgentName;
        [SerializeField]
        private TextMeshProUGUI _txtPlayerNickName;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _btnStart.onClick.AddListener(OnStartButton);
        }
        private void OnStartButton() 
        {
            
        }
    }
}
