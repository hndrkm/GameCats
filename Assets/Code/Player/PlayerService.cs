using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace CatGame
{
    public class PlayerService : IGlobalService
    {
        public Action<PlayerData> PlayerDataChanged;

        public PlayerData PlayerData { get; private set; }
        async void IGlobalService.Initialize()
        {
            PlayerData = LoadPlayer();
            PlayerData.Lock();
            SavePlayer();
        }

        void IGlobalService.Tick()
        {
            if (PlayerData.IsDirty == true)
            {
                SavePlayer();
                PlayerData.ClearDirty();

                PlayerDataChanged?.Invoke(PlayerData);
            }
        }

        void IGlobalService.Deinitialize()
        {
            PlayerData.Unlock();
            SavePlayer();

            PlayerDataChanged = null;
        }

        private PlayerData LoadPlayer()
        {
            var baseUserID = GetUserID();
            var userID = baseUserID;

            var playerData = PersistentStorage.GetObject<PlayerData>($"PlayerData-{userID}");

            if (Application.isMobilePlatform == false || Application.isEditor == true)
            {
                int clientIndex = 1;
                while (playerData != null && playerData.IsLocked() == true)
                {

                    userID = $"{baseUserID}.{clientIndex}";
                    playerData = PersistentStorage.GetObject<PlayerData>($"PlayerData-{userID}");

                    clientIndex++;
                }
            }

            if (playerData == null)
            {
                playerData = new PlayerData(userID);
                playerData.AgentID = Global.Settings.Agent.GetRandomAgentSetup().ID;
            };

            return playerData;
        }

        private void SavePlayer()
        {
            PersistentStorage.SetObject($"PlayerData-{PlayerData.UserID}", PlayerData, true);
        }

        private string GetUserID()
        {
            var userID = SystemInfo.deviceUniqueIdentifier;

            if (ApplicationSettings.UseRandomDeviceID == true)
            {
                userID = Guid.NewGuid().ToString();
            }
            if (ApplicationSettings.HasCustomDeviceID == true)
            {
                userID = ApplicationSettings.CustomDeviceID;
            }

#if UNITY_EDITOR
            userID = $"{userID}_{Application.dataPath.GetHashCode()}";
#endif

            return userID;
        }

       
    }
}
