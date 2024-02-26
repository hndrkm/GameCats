using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CatGame.UI
{
    public class UILobby : UIView
    {
        [SerializeField]
        private TextMeshProUGUI _idLobby;
        [SerializeField]
        private List<TextMeshProUGUI> _namePlayers;
        [SerializeField]
        private UIAgentList _agents;

        [SerializeField]
        private Button _ready;
        [SerializeField]
        private Button _startGame;


        private List<AgentSetup> _agentSetups = new List<AgentSetup>(16);
        private bool _uiPrepared;
        private void ChangeAgentPlayer(string agetID)
        {
            Context.PlayerData.AgentID = agetID;
            var prefabId = Context.PlayerData.AgentPrefabID;
            var localPlayer = Context.NetworkGame.GetPlayer(Context.Runner.LocalPlayer);
            if (localPlayer == null) { return; }
            localPlayer.UpdateAgentID(prefabId);
        }
        private void ChangeReadyPlayer()
        {
            var localPlayer = Context.NetworkGame.GetPlayer(Context.Runner.LocalPlayer);
            if (localPlayer == null) { return; }
            localPlayer.UpdateReady(true);
        }
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _agents.UpdateContent += OnUpdateAgentContent;
            _agents.SelectionChanged += OnAgentSelectionChanged;
            _startGame.onClick.AddListener(OnStartButton);
            _ready.onClick.AddListener(ChangeReadyPlayer);
            PrepareAgentData();
        }
        
        protected override void OnDeinitialize()
        {
            _agents.UpdateContent -= OnUpdateAgentContent;
            _agents.SelectionChanged -= OnAgentSelectionChanged;
            _startGame.onClick.RemoveListener(OnStartButton);
            base.OnDeinitialize();
        }
        protected override void OnOpen()
        {
            base.OnOpen();
            if (_uiPrepared == false)
            {
                _agents.Refresh(_agentSetups.Count);
                _agents.Selection = 0;
                OnAgentSelectionChanged(0);
                _idLobby.text = Context.Runner.LobbyInfo.Name;
                _uiPrepared = true;
            }
            

        }
        private void OnStartButton()
        {
            ((VersusGameplayMode)Context.GameplayMode).StartImmediately();
        }
        protected override void OnTick()
        {
            base.OnTick();
            _startGame.interactable = CanStartGame();
        }
        private bool CanStartGame()
        {
            if (Context.NetworkGame == null)
            {
                return false;
            }
            var playersOnGame = Context.NetworkGame.Players;
            if (playersOnGame.Length<=0)
            {
                return false;
            }
            for (int i = 0; i < playersOnGame.Length; i++)
            {
                if (playersOnGame[i] == null)
                {
                    continue;
                }
                _namePlayers[i].text = playersOnGame[i].Nickname;
                _namePlayers[i].color = Color.green;
                if (playersOnGame[i].IsReady == false)
                {
                    _namePlayers[i].color = Color.red;
                    return false;
                }
            }
            return true;
        }
        
        private void OnAgentSelectionChanged(int index)
        {
            if (index >= 0)
            {
                var agentSetup = _agentSetups[index];
                ChangeAgentPlayer(agentSetup.ID);
            }

        }
        private void OnUpdateAgentContent(int index, UIAgentItem content)
        {
            content.SetData(_agentSetups[index]);
        }
        private void PrepareAgentData()
        {
            _agentSetups.Clear();
            var allAgentsSetups = Context.Settings.Agent.Agents;
            for (int i = 0; i < allAgentsSetups.Length; i++)
            {
                var agentSetup = allAgentsSetups[i];
                _agentSetups.Add(agentSetup);
            }
        }

    }
}
