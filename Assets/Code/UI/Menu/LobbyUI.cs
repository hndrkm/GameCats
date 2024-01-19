using Fusion.Photon.Realtime;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Timeline.TimelinePlaybackControls;

namespace CatGame
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField]
        MenuUI _menuUI;
        [SerializeField]
        GameObject _gameLobby;
        [SerializeField]
        private List<SessionInfo> _sessionInfo = new List<SessionInfo>(32);
        private void Start()
        {
            _menuUI.Context.Matchmaking.SessionListUpdated += OnSessionListUpdated;
            _menuUI.Context.Matchmaking.LobbyJoined += OnLobbyJoined;
            _menuUI.Context.Matchmaking.LobbyJoinFailed += OnLobbyJoinFailed;
            _menuUI.Context.Matchmaking.LobbyLeft += OnLobbyLeft;
            
        }
        private void OnEnable()
        {
            TryJoinLobby(true);
        }

        private void TryJoinLobby(bool force)
        {
            if (PhotonAppSettings.Global.AppSettings.AppIdFusion.HasValue() == true)
            {
                _menuUI.Context.Matchmaking.JoinLobby(force);
            }
            else
            {
                Debug.LogWarning("ocurrio un error al intentar unirse");
#if UNITY_EDITOR
                    UnityEditor.Selection.activeObject = PhotonAppSettings.Global;
                    UnityEditor.EditorGUIUtility.PingObject(PhotonAppSettings.Global);
#endif
                
            }
        }

        private void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionInfo)
        {
            

            _sessionInfo.Clear();

            for (int i = 0; i < sessionInfo.Count; i++)
            {
                var session = sessionInfo[i];

                if (session.IsValid == false || session.IsOpen == false || session.IsVisible == false)
                    continue;
                _sessionInfo.Add(session);
            }

            
        }

        private void OnLobbyJoined()
        {
            Debug.Log("aceptado en room");
        }

        private void OnLobbyJoinFailed(string region)
        {
            var regionInfo = _menuUI.Context.Settings.Network.GetRegionInfo(region);

            var regionText = regionInfo != null ? $"{regionInfo.DisplayName} ({regionInfo.Region})" : "Unknown";
            Debug.Log($"fallo al unirse a la region {regionText}");
        }

        private void OnLobbyLeft()
        {
            ClearSessions();
        }

        private void ClearSessions()
        {
            _sessionInfo.Clear();
        }
    }
}
