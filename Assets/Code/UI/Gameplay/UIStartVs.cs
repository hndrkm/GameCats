using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CatGame.UI
{
    public class UIStartVs : UIView
    {
        [SerializeField]
        private TextMeshProUGUI _nickP1;
        [SerializeField]
        private TextMeshProUGUI _nickP2;
        [SerializeField]
        private TextMeshProUGUI _P1;
        [SerializeField]
        private TextMeshProUGUI _P2;

        [SerializeField]
        private Image _imgP1;
        [SerializeField]
        private Image _imgP2;

        private List<Player> _players = new List<Player>(2);
        private List<AgentSetup> _agents;
        protected override void OnOpen()
        {
            base.OnOpen();
            int i = 0;
            foreach (var player in Context.NetworkGame.Players)
            {
                if (player == null)
                {
                    continue;
                }
                _players.Add(player);
                i++;
            }
            foreach (var agent in Context.Settings.Agent.Agents) 
            {
                if (_players[0] == null)
                {
                    continue;
                }
                if (_players[0].AgentPrefabID == agent.AgentPrefabId)
                {
                    _imgP1.sprite = agent.Icon;
                }
            }
            foreach (var agent in Context.Settings.Agent.Agents)
            {
                if (_players.Count < 2)
                    continue;
                if (_players[1] == null)
                {
                    continue;
                }
                if (_players[1].AgentPrefabID == agent.AgentPrefabId)
                {
                    _imgP2.sprite = agent.Icon;
                }
            }

            _nickP1.text = _players[0].Nickname;
            if (_players.Count < 2)
                return;
            _nickP2.text = _players[1].Nickname;
            
        }
    }
}
