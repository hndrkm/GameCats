using Fusion;
using Fusion.Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CatGame.UI
{
    public class UIMultiplayer : UICloseView
    {
        [SerializeField]
        private Button _createSessionBtn;
        [SerializeField]
        private GameObject _sessionContainer;
        [SerializeField]
        UISessionInfo _sessionPrefab;


        private List<SessionInfo> _sessionInfo = new List<SessionInfo>(32);
        private SessionInfo _selectSession;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _createSessionBtn.onClick.AddListener(OnCreateGameButton);
        }
        protected override void OnDeinitialize()
        {
            _createSessionBtn.onClick.RemoveListener(OnCreateGameButton);
            base.OnDeinitialize();
        }
        protected override void OnOpen()
        {
            base.OnOpen();
            Context.Matchmaking.SessionListUpdated += OnSessionListUpdated;
            Context.Matchmaking.LobbyJoined += OnLobbyJoined;
            TryJoinLobby(true);

        }
        protected override void OnClose()
        {
            Context.Matchmaking.SessionListUpdated -= OnSessionListUpdated;
            Context.Matchmaking.LobbyJoined -= OnLobbyJoined;

            Context.Matchmaking.LeaveLobby();
            base.OnClose();
        }
        private void TryJoinLobby(bool force) 
        {
            if (PhotonAppSettings.Global.AppSettings.AppIdFusion.HasValue() ==true)
            {
                
                Context.Matchmaking.JoinLobby(force);
            }
        }

        private void OnCreateGameButton() 
        {
            OpenView<UICreateSession>();
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
            UpdateSessionListUI();
        }
        private void OnLobbyJoined() 
        {
            
        }
        private void UpdateSessionListUI() 
        {
            var sessions = _sessionContainer.GetComponentsInChildren<UISessionInfo>();
            for (int i = 0; i < sessions.Length; i++)
            {
                Destroy(sessions[i]);
            }
            for (int i = 0; i < _sessionInfo.Count; i++)
            {
                var sessioninstance = Instantiate(_sessionPrefab,_sessionContainer.transform);
                sessioninstance.SetData(_sessionInfo[i].Name);
            }
        }
    }
}
