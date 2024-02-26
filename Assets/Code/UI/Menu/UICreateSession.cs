using System;
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
        private UIMapList _maps;
        [SerializeField]
        private TextMeshProUGUI _displayMapName;
        [SerializeField]
        private TextMeshProUGUI _descripcionMap;
        [SerializeField]
        private TextMeshProUGUI _recomendedPlayers;


        [SerializeField]
        private TMP_InputField _gameName;
        [SerializeField]
        private TMP_Dropdown _gameplay;
        [SerializeField]
        private TMP_InputField _maxPlayers;
        [SerializeField]
        private Button _createBtn;
        [SerializeField]
        private Toggle _privateToggle;

        private List<MapSetup> _mapSetups = new List<MapSetup>(8);
        private bool _uiPrepared;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _maps.UpdateContent += OnUpdateMapContent;
            _maps.SelectionChanged += OnMapSelectionChanged;
            _createBtn.onClick.AddListener(OnCreateButton);
            PrepareMapData();
        }
        protected override void OnDeinitialize()
        {
            _maps.UpdateContent -= OnUpdateMapContent;
            _maps.SelectionChanged -= OnMapSelectionChanged;
            _createBtn.onClick.RemoveListener(OnCreateButton);
            base.OnDeinitialize();
        }
        protected override void OnOpen()
        {
            base.OnOpen();
            if (_uiPrepared == false)
            {
                UpdateDropDowns();
                _maps.Refresh(_mapSetups.Count);
                _maps.Selection = 0;
                OnMapSelectionChanged(0);

                if (_gameName.text.Length < 5)
                {
                    _gameName.text = $"{Context.PlayerData.Nickname}Game";
                    _maxPlayers.text = 2.ToString();
                }
                _uiPrepared = true;
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
                ScenePath = _mapSetups[_maps.Selection].ScenePath,
                isPrivate = !_privateToggle.isOn,
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
            if (_maps.Selection < 0)
                return false;
            var mapSetup = _mapSetups[_maps.Selection];
            if (mapSetup == null)
                return false;
            if (System.Int32.TryParse(_maxPlayers.text, out int maxPlayers) == false)
                return false;

            if (maxPlayers < 2 || maxPlayers > mapSetup.MaxPlayers)
                return false;
            if (_gameName.text.Length < 5)
                return false;
            return true;
        }
        private void UpdateDropDowns() 
        {
            var options = ListPool.Get<string>(16);
            var defaultOption = 0;
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
            _gameplay.SetValueWithoutNotify(defaultOption);
            ListPool.Return(options);
        }
        private void OnMapSelectionChanged(int index) 
        {
            if (index >= 0)
            {
                var mapSetup = _mapSetups[index];

                _displayMapName.text=(mapSetup.DisplayName);
                _descripcionMap.text=(mapSetup.Description);
                _recomendedPlayers.text = mapSetup.RecommendedPlayers.ToString();

                _maxPlayers.text = mapSetup.RecommendedPlayers.ToString();
            }
            
        }
        private void OnUpdateMapContent(int index, UIMapItem content) 
        {
            content.SetData(_mapSetups[index]);
        }
        private void PrepareMapData() 
        {
            _mapSetups.Clear();
            var allMapSetups = Context.Settings.Map.Maps;
            for (int i = 0; i < allMapSetups.Length; i++)
            {
                var mapSetup = allMapSetups[i];
                if (mapSetup.ShowInMapSelection == true)
                {
                    _mapSetups.Add(mapSetup);
                }
            }
        }
    }
}
