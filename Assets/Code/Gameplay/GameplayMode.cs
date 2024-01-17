using Fusion;
using System;
using UnityEngine.SocialPlatforms.Impl;

namespace CatGame
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    public struct KillData : INetworkStruct
    {
        public PlayerRef KillerRef;
        public PlayerRef VictimRef;

        private byte _flags;
    }
    public enum EGameplayType
    {
        None,
        Versus,
    }
    public class GameplayMode : ContextBehaviour
    {
        public enum EState
        {
            None,
            Active,
            Finished,
        }
        public const byte MAX_PLAYERS = 50;

        public string GameplayName;

        public int MaxPlayers;
        public short ScorePerKill;
        public short ScorePerDeath;
        public float TimeLimit;

        public EGameplayType Type => _type;
        public float Time => (Runner.Tick - _startTick) * Runner.DeltaTime;
        public float RemainingTime => _endTimer.IsRunning == true ? _endTimer.RemainingTime(Runner).Value : 0f;
        [Networked, HideInInspector]
        public EState State { get; private set; }

        public Action<PlayerRef> OnPlayerJoinedGame;
        public Action<string> OnPlayerLeftGame;
        public Action<KillData> OnAgentDeath;
        public Action<PlayerRef> OnPlayerEliminated;


        [Networked, HideInInspector]
        protected int _startTick { get; set; }
        [Networked, HideInInspector]
        protected TickTimer _endTimer { get; private set; }


        [SerializeField]
        private EGameplayType _type;

        public void Activate()
        {
            if (Runner.IsServer == false)
                return;
            if (State != EState.None)
                return;
            _startTick = Runner.Tick;
            if (TimeLimit > 0f)
            {
                _endTimer = TickTimer.CreateFromSeconds(Runner, TimeLimit);
            }
            //Runner.SimulationUnityScene.GetComponents();
            State = EState.Active;


        }
        public void AgentDeath(Agent victim, HitData hitData)
        {
            if (Runner.IsServer == false)
                return;
            if (State != EState.Active)
                return;

        }

        public void PlayerJoined(Player player)
        {

        }
        protected virtual void OnActivate() { }


        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_AgentDeath(KillData killData)
        {
            OnAgentDeath?.Invoke(killData);
        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_PlayerEliminated(KillData killData)
        {

        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_PlayerJoinedGame(KillData killData)
        {

        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_PlayerLeftGame(KillData killData)
        {

        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies, Channel = RpcChannel.Reliable)]
        private void RPC_StopGame(KillData killData)
        {

        }
        

        private class DefaultPlayerComparer : IComparer<PlayerStatistics>
        {
            public int Compare(PlayerStatistics x, PlayerStatistics y)
            { 
                return y.Score.CompareTo(x.Score);
            }
        }

    }
}
