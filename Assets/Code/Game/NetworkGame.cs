using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using Unity.VisualScripting;

namespace CatGame
{
    public class NetworkGame : ContextBehaviour, IPlayerJoined, IPlayerLeft
    {
        [Networked, HideInInspector, Capacity(byte.MaxValue)]
        public NetworkArray<Player> Players => default;
        public int ActivePlayerCount { get; private set; }

        [SerializeField]
        private Player _playerPrefab;
        [SerializeField]
        private GameplayMode[] _modePrefabs;


        private PlayerRef _localPlayer;
        private Dictionary<PlayerRef, Player> _pendingPlayers = new Dictionary<PlayerRef, Player>();
        private Dictionary<string, Player> _disconnectedPlayers = new Dictionary<string, Player>();
        private FusionCallbackHandler _fusionCallbacks = new FusionCallbackHandler();
        private GameplayMode _gameplayMode;
        private bool _isActive;

        public void Initialize(EGameplayType gameplayType)
        {
            if (Object.HasStateAuthority==true) 
            {
                var prefab = _modePrefabs.Find(t=>t.Type == gameplayType);
                _gameplayMode = Runner.Spawn(prefab,onBeforeSpawned:OnSpawGamePlayMode);
            }
            _localPlayer = Runner.LocalPlayer;
            _fusionCallbacks.DisconnectedFromServer -= OnDisconnectedFromServer;
            _fusionCallbacks.DisconnectedFromServer += OnDisconnectedFromServer;
            Runner.RemoveCallbacks(_fusionCallbacks);
            Runner.AddCallbacks(_fusionCallbacks);
            ActivePlayerCount = 0;

        }
        private void OnSpawGamePlayMode(NetworkRunner runner,NetworkObject gameMode) 
        {
            gameMode.GetComponent<GameplayMode>().Context = Context;
            Context.GameplayMode = gameMode.GetComponent<GameplayMode>();
        }
        public void Activate() 
        {
            _isActive = true;
            if (Object.HasStateAuthority == false)
            {
                return;
            }
            _gameplayMode.Activate();
            foreach (var playerRef in Runner.ActivePlayers)
            {
               // SpawnPlayer(playerRef);
            }
        }
        public Player GetPlayer(PlayerRef playerRef) 
        {
            if (playerRef.IsRealPlayer == false)
                return null;
            if (Object == null)
                return null;
            return Players[playerRef.PlayerId];
        }
        public int GetActivePlayerCoout() 
        {
            int players = 0;
            foreach (Player player in Players) 
            {
                if(player == null)
                    continue;
                var statistics =player.Statistics;
                if (statistics.IsValid == false)
                {
                    continue;
                }
                players++;
            }
            return players;
        }

        public override void FixedUpdateNetwork()
        {
            ActivePlayerCount = GetActivePlayerCoout();
            if (Object.HasStateAuthority == false)
                return;
            if (_pendingPlayers.Count == 0)
                return;
            var playersToRemove = ListPool.Get<PlayerRef>(128);
            foreach (var playerPair in _pendingPlayers)
            {
                var playerRef = playerPair.Key;
                var player = playerPair.Value;
                if (player.IsInitialized == false)
                    continue;
                playersToRemove.Add(playerRef);
                if (_disconnectedPlayers.TryGetValue(player.UserID, out Player disconnectedPlayer) == true)
                {
                    Runner.Despawn(player.Object);
                    _disconnectedPlayers.Remove(player.UserID);
                    player = disconnectedPlayer;
                    player.Object.AssignInputAuthority(playerRef);
                }
                Players.Set(playerRef.PlayerId,player);
#if UNITY_EDITOR
                player.gameObject.name = $"Player {player.Nickname}";
#endif
                _gameplayMode.PlayerJoined(player);
            }
            for (int i = 0; i < playersToRemove.Count; i++)
            {
                _pendingPlayers.Remove(playersToRemove[i]);
            }
            ListPool.Return(playersToRemove);
        }

        public void PlayerJoined(PlayerRef playerRef)
        {
            if (Runner.IsServer == false) return;
            if (_isActive) return;
            SpawnPlayer(playerRef);
        }

        public void PlayerLeft(PlayerRef playerRef)
        {
            if (playerRef.IsRealPlayer == false) return;
            if (Runner.IsServer ==false) return;
            if (_isActive == false) return;
            Player player = Players[playerRef.PlayerId];
            Players.Set(playerRef.PlayerId, null);
            if (player != null)
            {
                if (player.UserID.HasValue() == true)
                {
                    _disconnectedPlayers[player.UserID] = player;
                    
                    player.Object.RemoveInputAuthority();
#if UNITY_EDITOR
                    player.gameObject.name = $"{player.gameObject.name}(Desconectado)";
#endif
                }
                else
                {
                    Runner.Despawn(player.Object);
                }
            }

        }
        private void SpawnPlayer(PlayerRef playerRef) 
        {
            
            if (Players[playerRef.PlayerId] != null || _pendingPlayers.ContainsKey(playerRef) ==true) 
            {
                Log.Error($"El Player {playerRef} esta ya en el juego!");
                return;
            }
            var player = Runner.Spawn(_playerPrefab,inputAuthority: playerRef);
            Runner.SetPlayerAlwaysInterested(playerRef, player.Object, true);
            _pendingPlayers[playerRef] = player;
#if UNITY_EDITOR
            player.gameObject.name = $"player (pendiente)";
#endif
        }
        private void OnDisconnectedFromServer(NetworkRunner runner)
        {
            if (runner != null) 
            {
                
            }
            
        }
    }
}
