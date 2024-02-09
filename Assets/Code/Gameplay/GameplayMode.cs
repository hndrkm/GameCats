using Fusion;
using System;
namespace CatGame
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UIElements;
    using static Cinemachine.CinemachineTriggerAction.ActionSettings;

    public struct KillData : INetworkStruct
    {
        public PlayerRef KillerRef;
        public PlayerRef VictimRef;
        public EHitType HitType;
        private byte _flags;
    }
    public enum EGameplayType
    {
        None,
        Versus,
        Elimination,
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
        public float RespawnTime;
        public float TimeLimit;

        public EGameplayType Type => _type;
        public float Time => (Runner.Tick - _startTick) * Runner.DeltaTime;
        public float RemainingTime => _endTimer.IsRunning == true ? _endTimer.RemainingTime(Runner).Value : 0f;
        [Networked, HideInInspector]
        public EState State { get; private set; }
        public List<SpawnPoint> SpawnPoints => _allSpawnPoints;

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

        private List<SpawnPoint> _allSpawnPoints = new List<SpawnPoint>();
        private List<SpawnPoint> _availableSpawnPoints = new List<SpawnPoint>();

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

            Runner.SimulationUnityScene.GetComponents(_allSpawnPoints);

            for (int i = 0; i < _allSpawnPoints.Count; i++)
            {
                if (_allSpawnPoints[i].SpawnEnabled == true)
                {
                    _availableSpawnPoints.Add(_allSpawnPoints[i]);
                }
            }
            State = EState.Active;
            OnActivate();
        }

        public void AgentDeath(Agent victim, HitData hitData)
        {
            if (Runner.IsServer == false)
                return;
            if (State != EState.Active)
                return;

            var victimRef = victim.Object.InputAuthority;
            var victimPlayer = Context.NetworkGame.GetPlayer(victimRef);
            var victimStatistics = victimPlayer != null ? victimPlayer.Statistics : default;

            var respawnTime = GetRespawnTime(victimStatistics);
            if (respawnTime >= 0f)
            {
                var respawnTimer = TickTimer.CreateFromSeconds(Runner, respawnTime);
                victimStatistics.RespawnTimer = respawnTimer;

            }
            else
            {
                victimStatistics.IsEliminated = true;
            }
            victimStatistics.IsAlive = false;
            victimStatistics.Deaths += 1;
            victimStatistics.Score += ScorePerDeath;
            var killerRef = hitData.InstigatorRef;
            var killerPlayer = killerRef.IsValid == true ? Context.NetworkGame.GetPlayer(killerRef) : default;
            var killerStatistics = killerPlayer != null ? killerPlayer.Statistics : default;
            if (killerRef == victimRef)
            {
                victimStatistics.Score += ScorePerKill;
            }
            else
            {
                killerStatistics.Kills += 1;
                killerStatistics.Score += ScorePerKill;
            }

            AgentDeath(ref victimStatistics, ref killerStatistics);
            if (victimPlayer != null)
                victimPlayer.UpdateStatistics(victimStatistics);
            if (killerPlayer != null && killerPlayer != victimPlayer)
                killerPlayer.UpdateStatistics(killerStatistics);

            var killData = new KillData()
            {
                KillerRef = killerStatistics.PlayerRef,
                VictimRef = victimRef,
                HitType = hitData.HitType,
            };
            RPC_AgentDeath(killData);
            if (victimStatistics.IsEliminated == true)
            {
                RPC_PlayerEliminated(victimRef);
            }
            CheckWinCondition();

        }
        public Transform GetRandomSpawnPoint(float minDistanceFromAgents)
        {
            Debug.Log(_availableSpawnPoints.SafeCount());
            if (_availableSpawnPoints.SafeCount() == 0)
                return null;

            while (minDistanceFromAgents > 1.0f)
            {
                float minSqrDistanceFromAgents = minDistanceFromAgents * minDistanceFromAgents;

                for (int i = 0, count = Mathf.Min(5 + _availableSpawnPoints.Count, 25); i < count; ++i)
                {
                    Transform spawnPoint = _availableSpawnPoints.GetRandom().transform;
                    bool isValid = true;

                    foreach (var player in Context.NetworkGame.Players)
                    {
                        if (player == null)
                            continue;

                        var agent = player.ActiveAgent;
                        if (agent == null)
                            continue;

                        if (Vector3.SqrMagnitude(agent.transform.position - spawnPoint.position) < minSqrDistanceFromAgents)
                        {
                            isValid = false;
                            break;
                        }
                    }

                    if (isValid == true)
                        return spawnPoint;
                }

                minDistanceFromAgents *= 0.5f;
            }

            return _availableSpawnPoints.GetRandom().transform;
        }

        public void PlayerJoined(Player player)
        {
            var statistics = player.Statistics;

            statistics.PlayerRef = player.Object.InputAuthority;

            PreparePlayerStatistics(ref statistics);
            player.UpdateStatistics(statistics);

            if (statistics.IsEliminated == false)
            {
                TrySpawnAgent(player);
            }



            RPC_PlayerJoinedGame(player.Object.InputAuthority);
        }
        public void PlayerLeft(Player player)
        {
            if (Runner.IsServer == false)
                return;
            if (State == EState.Finished)
                return;

            player.DespawnAgent();

            RPC_PlayerLeftGame(player.Object.InputAuthority, player.Nickname);

            CheckWinCondition();
        }
        public void StopGame()
        {
            if (Object == null || Object.HasStateAuthority == false)
            {
                Global.Networking.StopGame();
                return;
            }
            StopGameRutine();
        }
        public override void Spawned()
        {
            Context.GameplayMode = this;

        }
        public override void FixedUpdateNetwork()
        {
            if (Object.HasStateAuthority == false)
            {
                return;
            }
            switch (State)
            {
                case EState.Active: break;
                case EState.Finished: break;
            }
        }

        protected virtual void OnActivate() { }
        protected virtual void TrySpawnAgent(Player player)
        {
            Transform spawnPoint = GetRandomSpawnPoint(100.0f);

            var spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            var spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            SpawnAgent(player.Object.InputAuthority, spawnPosition, spawnRotation);
        }
        protected virtual void AgentDeath(ref PlayerStatistics victimStatistic, ref PlayerStatistics killerStatistics) { }
        protected void CheckWinCondition() { }
        protected virtual float GetRespawnTime(PlayerStatistics playerStatistics)
        {
            return RespawnTime;
        }
        protected virtual void PreparePlayerStatistics(ref PlayerStatistics playerStatistics)
        {
        }
        protected Agent SpawnAgent(PlayerRef playerRef, Vector3 position, Quaternion rotation)
        {
            var player = Context.NetworkGame.GetPlayer(playerRef);

            if (player.AgentPrefabID.IsValid == false)
            {
                Debug.Log("error prefab");
                throw new InvalidOperationException(nameof(player.AgentPrefabID));
            }

            var agentObject = Runner.Spawn(player.AgentPrefabID, position, rotation, playerRef);
            var agent = agentObject.GetComponent<Agent>();

            Runner.SetPlayerAlwaysInterested(playerRef, agentObject, true);

            var statistics = player.Statistics;
            statistics.IsAlive = true;
            statistics.RespawnTimer = default;

            player.UpdateStatistics(statistics);
            player.SetActiveAgent(agent);

            return agent;
        }

        protected void FinishGamePlay()
        {
            if (State != EState.Active)
                return;
            if (Runner.IsServer == false)
                return;
            State = EState.Finished;
            Runner.SessionInfo.IsOpen = false;

        }

        private void FixedUpdateNetwork_Active() 
        {
            if(_endTimer.Expired(Runner) == true)
                FinishGamePlay();
        }
        private void FixedUpdateNetwork_Finished()
        {
            
        }
        private void StopGameRutine() 
        {
            RPC_StopGame();
            Global.Networking.StopGame();
        }
        private IEnumerator ShutdownCoroutine() 
        {
            yield return new WaitForSecondsRealtime(20.0f);
            Debug.LogWarning("Apagando ");
            Application.Quit();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_AgentDeath(KillData killData)
        {
            OnAgentDeath?.Invoke(killData);
        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_PlayerEliminated(PlayerRef playerRef)
        {
            OnPlayerEliminated?.Invoke(playerRef);
        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_PlayerJoinedGame(PlayerRef playerRef)
        {
            OnPlayerJoinedGame?.Invoke(playerRef);
        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_PlayerLeftGame(PlayerRef playerRef, string nickname)
        {
            OnPlayerLeftGame?.Invoke(nickname);
        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies, Channel = RpcChannel.Reliable)]
        private void RPC_StopGame()
        {
            Global.Networking.StopGame(Networking.STATUS_SERVER_CLOSED);
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
