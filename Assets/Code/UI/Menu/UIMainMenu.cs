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
            _btnChangeNickname.onClick.AddListener(OnChangeNickNameButton);
            _btnQuit.onClick.AddListener(OnQuitButton);
            _btnPlayer.onClick.AddListener(OnPlayerButton);
            _btnSettings.onClick.AddListener(OnSettingButton);
        }
        protected override void OnDeinitialize()
        {
            _btnStart.onClick.RemoveListener(OnStartButton);
            _btnChangeNickname.onClick.RemoveListener(OnChangeNickNameButton);
            _btnQuit.onClick.RemoveListener(OnQuitButton);
            _btnPlayer.onClick.RemoveListener(OnPlayerButton);
            _btnSettings.onClick.RemoveListener(OnSettingButton);
            base.OnDeinitialize();
        }
        protected override void OnOpen()
        {
            base.OnOpen();
            UpdatePlayer();
            Global.PlayerService.PlayerDataChanged += OnPlayerDataChanged;
            
        }
        protected override void OnClose() 
        {
            Global.PlayerService.PlayerDataChanged -= OnPlayerDataChanged;
            base.OnClose();
        }
        private void OnStartButton() 
        {
            OpenView<UIMultiplayer>();
        }
        private void OnChangeNickNameButton()
        {
            var changeNickname = OpenView<UIChangeNickname>();
            changeNickname.SetData("Cambiar Nickname", false);
        }
        private void OnQuitButton()
        {

        }
        private void OnSettingButton()
        {

        }
        private void OnPlayerButton()
        {

        }
        private void OnPlayerDataChanged(PlayerData playerData) 
        {
            UpdatePlayer();
        }
        private void UpdatePlayer() 
        {
            _txtPlayerNickName.text = Context.PlayerData.Nickname;
            var setup = Context.Settings.Agent.GetAgentSetup(Context.PlayerData.AgentID);
            _txtAgentName.text = setup != null ? setup.DisplayName : string.Empty;
        } 
    }
}
