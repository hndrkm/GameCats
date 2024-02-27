using CatGame.UI;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    public class VersusGameplayMode : GameplayMode
    {
        public int ScoreLimit;

        public bool HasStarted { get { return _state.IsBitSet(0); } set { _state = _state.SetBitNoRef(0, value); } }
        public bool LobbyActive { get { return _state.IsBitSet(1); } set { _state = _state.SetBitNoRef(1, value); } }
        public float DelayTime => _startCooldown.IsRunning == true ? _startCooldown.RemainingTime(Runner).Value : 0f;
        public float WaitTime => _waitForPlayers.IsRunning == true ? _waitForPlayers.RemainingTime(Runner).Value : 0f;
        [SerializeField]
        private float _delayStart = 5f;
        [SerializeField]
        private float _waitForPlayersTime = 5f;

        [Networked]
        private byte _state { get; set; }
        [Networked]
        private TickTimer _waitForPlayers { get; set; }
        [Networked]
        private TickTimer _startCooldown { get; set; }

        public void StartImmediately() 
        {
            _waitForPlayers = TickTimer.None;
            if (Object.HasStateAuthority == true)
            {
                StartGameMode();
            }            
        }
       
        private void StartGameMode()
        {
            HasStarted = true;
            RPC_StartVSGame();
            SpawnAgents();
            _startCooldown = TickTimer.CreateFromSeconds(Runner, _delayStart);

        }
        private void StopGameLobby()
        {
            RPC_StartGame();
            LobbyActive = false;

        }
        public void SpawnAgents()
        {
            foreach (var player in Context.NetworkGame.Players)
            {
                if (player == null)
                {
                    continue;
                }
                TrySpawnAgent(player);
            }
        }
        protected override void OnActivate()
        {
            _waitForPlayers = TickTimer.CreateFromSeconds(Runner, _waitForPlayersTime);
            LobbyActive = true;
        }

        public override void PlayerJoined(Player player) 
        {
            var statistics = player.Statistics;

            statistics.PlayerRef = player.Object.InputAuthority;

            PreparePlayerStatistics(ref statistics);
            player.UpdateStatistics(statistics);
            RecalculatePositions();
            RPC_PlayerJoinedGame(player.Object.InputAuthority);
        }
        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            if (State != EState.Active)
                return;
            if (Object.HasStateAuthority == false)
                return;
            if (HasStarted == false && _waitForPlayers.ExpiredOrNotRunning(Runner) == true)
            {
                StartGameMode();
            }

            if (HasStarted == true && LobbyActive == true && _startCooldown.ExpiredOrNotRunning(Runner) == true)
            {
                StopGameLobby();
            }

        }

        protected override void AgentDeath(ref PlayerStatistics victimStatistic, ref PlayerStatistics killerStatistics)
        {
            base.AgentDeath(ref victimStatistic, ref killerStatistics);
            if (killerStatistics.IsValid == true && victimStatistic.PlayerRef != killerStatistics.PlayerRef)
            {
                if (killerStatistics.Score >= ScoreLimit)
                {
                    FinishGamePlay();
                }
            }
        }
        protected override void CheckWinCondition()
        {
            foreach (var player in Context.NetworkGame.Players)
            {
                if (player == null)
                {
                    return;
                }
                if(player.Statistics.Score >= ScoreLimit) 
                {
                    FinishGamePlay();
                    return;
                }
            }
        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_StartVSGame()
        {
            Context.UI.CloseView<UILobby>();
            Context.UI.OpenView<UIStartVs>(); 
        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_StartGame()
        {
            Context.UI.CloseView<UIStartVs>(); 
            Context.UI.OpenView<UIGameplay>();
            Context.UI.OpenView<UIMobileInput>();
        }
    }
}
