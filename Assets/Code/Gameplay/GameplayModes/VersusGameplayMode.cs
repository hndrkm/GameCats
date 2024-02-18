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
            if (Object.HasStateAuthority == true)
            {
                StartGameMode();
            }
            
        }

        protected override void OnActivate()
        {
            _waitForPlayers = TickTimer.CreateFromSeconds(Runner, _waitForPlayersTime);
        }
        private void StartGameMode() 
        {
            HasStarted = true;
            _startCooldown = TickTimer.CreateFromSeconds(Runner,_delayStart);
        }
        private void StopGameLobby()
        {
            LobbyActive = false;
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

            if (HasStarted == true && LobbyActive == false)
            {
                
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


    }
}
