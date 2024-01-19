using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace CatGame
{
    public struct PlayerStatistics : INetworkStruct
    { 
        public PlayerRef PlayerRef;
        public short Extralives;
        public short Kills;
        public short Deaths;
        public short Score;
        public TickTimer RespawnTimer;
        public byte Position;

        public bool IsValid => PlayerRef.IsRealPlayer;
        public bool IsAlive { get { return _flags.IsBitSet(0); } set { _flags.SetBit(0, value); } }
        public bool IsEliminated { get { return _flags.IsBitSet(1); } set { _flags.SetBit(1, value); } }
        private byte _flags;
    }
    public class Player : ContextBehaviour
    {
        public bool IsInitialized { get; private set; }
        public string UserID { get; private set; }
        [Networked, Capacity(24)]
        public string Nickname { get; private set; }

        [Networked]
        public PlayerStatistics Statistics { get; private set; }
        [Networked]
        public Agent ActiveAgent { get; private set; }
        [Networked]
        public NetworkPrefabId AgentPrefabID { get; set; }

        private PlayerRef _observedPlayer;

        public void SetActiveAgent(Agent agent)
        {
            ActiveAgent = agent;
            _observedPlayer = Object.InputAuthority;
        }
        public void UpdateStatistics(PlayerStatistics statistics)
        {
            Statistics = statistics;
        }
        public override void Spawned()
        {
            base.Spawned();
            _observedPlayer = Object.InputAuthority;
            if (Object.HasInputAuthority == true)
            {
                Context.LocalPlayerRef = _observedPlayer;
            }
            IsInitialized = true;
        }
        public override void FixedUpdateNetwork()
        {
            if (Object.IsProxy == true) { return; }

            //var observedPlayer = Context.NetworkGame.GetPlayer(_observedPlayer);
            //var observedAgent = observedPlayer != null && observedPlayer.ActiveAgent != null && observedPlayer.ActiveAgent.Object != null? observedPlayer.ActiveAgent : ActiveAgent;

            //if (Object.HasInputAuthority == true) 
            //{
            //    Context.ObservedAgent = observedAgent;
            //    Context.ObservedPlayerRef = observedAgent != null ? observedAgent.Object.InputAuthority : Object.InputAuthority;
            //    Context.LocalPlayerRef = Object.InputAuthority;
            //}

            if (IsInitialized == false && Object.HasInputAuthority == true && Runner.Stage == SimulationStages.Forward && Context.PlayerData != null)
            {
                RPC_Initialize(Context.PeerUserID,Context.PlayerData.Nickname,Context.PlayerData.AgentPrefabID);
            }
        }
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        private void RPC_Initialize(string userID, string nickname, NetworkPrefabId agentPrefabID)
        {
#if UNITY_EDITOR
            nickname += $"{Object.InputAuthority}";
#endif
            UserID = userID;
            Nickname = nickname;
            IsInitialized = true;
        }
    }
}
