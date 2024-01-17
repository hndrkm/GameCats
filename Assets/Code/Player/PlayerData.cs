using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace CatGame
{
    public interface IPlayer 
    {
        string UserID { get; }
        string Nickname { get; }
        NetworkPrefabId AgentPrefabID { get; }
    }
    public class PlayerData : IPlayer
    {
        public string UserID => _userID;
        public NetworkPrefabId AgentPrefabID => GetAgentPrefabID();
        public string Nickname { get { return _nickname; } set { _nickname = value; IsDirty = true; } }
        public string AgentID { get { return _agentID; } set { _agentID = value; IsDirty = true; } }

        public bool IsDirty { get; private set; }

        [SerializeField]
        private string _userID;
        [SerializeField]
        private string _nickname;
        [SerializeField]
        private string _agentID;

        [SerializeField]
        private bool _isLocked;
        [SerializeField]
        private int _lastProcessID;

        public PlayerData(string userID)
        {
            _userID = userID;
        }
        public void ClearDirty()
        {
            IsDirty = false;
        }

        public bool IsLocked(bool checkProcess = true)
        {
            if (_isLocked == false)
                return false;

            if (checkProcess == true)
            {
                try
                {
                    var process = Process.GetProcessById(_lastProcessID);
                }
                catch (Exception)
                {
                    // no corre
                    return false;
                }
            }

            return true;
        }

        public void Lock()
        {
            // Cuando ejecutamos varias instancias del juego en elmismo disp, queremos bloquear los datos del jugador usado

            _isLocked = true;
            _lastProcessID = Process.GetCurrentProcess().Id;
        }

        public void Unlock()
        {
            _isLocked = false;
        }


        private NetworkPrefabId GetAgentPrefabID()
        {
            if (_agentID.HasValue() == false)
                return default;

            var setup = Global.Settings.Agent.GetAgentSetup(_agentID);
            return setup != null ? setup.AgentPrefabId : default;
        }
    }
}
