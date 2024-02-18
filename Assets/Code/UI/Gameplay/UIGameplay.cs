using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CatGame.UI
{
    public class UIGameplay : UIView
    {

        [SerializeField]
        private TextMeshProUGUI _nickTex;
        private Agent _localAgent;
        private NetworkBehaviourId _localAgentId;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            ClearLocalAgent();
        }
        protected override void OnTick()
        {
            base.OnTick();
            
            if (Context.Runner == null || Context.Runner.IsRunning == false)
                return;
            
            if (Context.GameplayMode == null || Context.Runner.Exists(Context.GameplayMode.Object) == false)
                return;
            
            if (_localAgent != Context.ObservedAgent || (Context.ObservedAgent != null && _localAgentId != Context.ObservedAgent.Id)) 
            {
                
                if (Context.ObservedAgent == null)
                {
                    ClearLocalAgent();
                }
                else
                {
                    
                    var player = Context.NetworkGame.GetPlayer(Context.ObservedPlayerRef);
                    if (player == null)
                    {
                        ClearLocalAgent();
                    }
                    else
                    {
                        SetLocalAgent(BaseUI.Context.ObservedAgent, player, Context.LocalPlayerRef == Context.ObservedPlayerRef);
                    }
                }
            }
        }
        private void SetLocalAgent(Agent agent,Player player, bool isLocalPlayer) 
        {
            if (_localAgent != null) 
            {
                
            }
            _localAgent = agent;
            _localAgentId = player.Id;

            _nickTex.text = player.Nickname;
            
        }
        private void ClearLocalAgent() 
        {
            if (_localAgent != null)
            {
                _localAgent = null;
                _localAgentId = default;
            }
        }
    }
}
