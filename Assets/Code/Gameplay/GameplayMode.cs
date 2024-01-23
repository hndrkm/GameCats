using Fusion;
using System;
namespace CatGame
{

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
        }
       
        public void AgentDeath(Agent victim, HitData hitData)
        {
            if (Runner.IsServer == false)
                return;
            if (State != EState.Active)
                return;

        }
        public Transform GetRandomSpawnPoint(float minDistanceFromAgents)
        {
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
        public override void FixedUpdateNetwork()
        {
            if (Object.HasStateAuthority == false)
            {
                return;
            }
            
        }
        public override void Spawned()
        {
            //Context.GameplayMode = this;
        }
        protected virtual void OnActivate() { }
        protected virtual void TrySpawnAgent(Player player)
        {
            Transform spawnPoint = GetRandomSpawnPoint(100.0f);

            var spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            var spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            SpawnAgent(player.Object.InputAuthority, spawnPosition, spawnRotation);
        }
        protected void CheckWinCondition() { }
        protected virtual void PreparePlayerStatistics(ref PlayerStatistics playerStatistics)
        {
        }
        protected Agent SpawnAgent(PlayerRef playerRef, Vector3 position, Quaternion rotation)
        {
            var player = Context.NetworkGame.GetPlayer(playerRef);
            Debug.Log(player.AgentPrefabID);
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
