using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CatGame.UI
{
    public class UICreateSession : UICloseView
    {
        [SerializeField]
        private TMP_InputField _gameName;
        [SerializeField]
        private TMP_Dropdown _gameplay;
        [SerializeField]
        private TMP_InputField _maxPlayers;
        [SerializeField]
        private Button _createBtn;
        [SerializeField]
        private TMP_Dropdown _mapSelection;

        private List<MapSetup> _mapSetup = new List<MapSetup>(8);
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _createBtn.onClick.AddListener(OnCreateButton);
            PrepareMapData();
        }
        protected override void OnDeinitialize()
        {
            _createBtn.onClick.RemoveListener(OnCreateButton);
            base.OnDeinitialize();
        }
        protected override void OnOpen()
        {
            base.OnOpen();
            UpdateDropDowns();
            if (_gameName.text.Length <5)
            {
                _gameName.text = $"{Context.PlayerData.Nickname}Game";
                _maxPlayers.text = 2.ToString();
            }
        }
        private void OnCreateButton() 
        {
            var request = new SessionRequest
            {
                DisplayName = _gameName.text,
                GameMode = Fusion.GameMode.Host,
                MaxPlayers = System.Int32.Parse(_maxPlayers.text),
                GameplayType = (EGameplayType)(_gameplay.value + 1),
                ScenePath = _mapSetup[0].ScenePath,
            };
            Context.Matchmaking.CreateSession(request);
        }
        protected override void OnTick()
        {
            base.OnTick();
            _createBtn.interactable = CanCreateGame();
        }
        private bool CanCreateGame()
        {
            if(_gameName.text.Length < 5)
                return false;
            return true;
        }
        private void UpdateDropDowns() 
        {
            var options = ListPool.Get<string>(16);
            int i = 0;
            foreach (EGameplayType value in System.Enum.GetValues(typeof(EGameplayType)))
            {
                if (value == EGameplayType.None)
                    continue;
                options.Add(value.ToString());
                i++;
            }
            _gameplay.ClearOptions();
            _gameplay.AddOptions(options);
            ListPool.Return(options);
        }
        private void PrepareMapData() 
        {
            _mapSetup.Clear();
            var allMapSetups = Context.Settings.Map.Maps;
            for (int i = 0; i < allMapSetups.Length; i++)
            {
                var mapSetup = allMapSetups[i];
                if (mapSetup.ShowInMapSelection == true)
                {
                    _mapSetup.Add(mapSetup);
                }
            }
            List<string> optionsDatas = new List<string>(8);
            for (int i = 0; i < _mapSetup.Count; i++)
            {
                optionsDatas.Add(_mapSetup[i].DisplayName);
            }
            _mapSelection.ClearOptions();
            _mapSelection.AddOptions(optionsDatas);
        }
    }
}
