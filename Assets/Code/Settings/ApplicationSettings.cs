using Fusion.Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    public static class ApplicationSettings
    {
        public static readonly bool IsHost;
        public static readonly bool IsServer;
        public static readonly bool IsClient;
        public static readonly bool IsVersus;
        public static readonly bool IsQuickPlay;
        public static readonly bool HasRegion;
        public static readonly string Region;
        public static readonly bool HasServerName;
        public static readonly string ServerName;
        public static readonly bool HasMaxPlayers;
        public static readonly int MaxPlayers;
        public static readonly bool HasSessionName;
        public static readonly string SessionName;
        public static readonly bool HasCustomLobby;
        public static readonly string CustomLobby;
        public static readonly bool HasCustomScene;
        public static readonly string CustomScene;
        public static readonly bool HasIPAddress;
        public static readonly string IPAddress;
        public static readonly bool HasPort;
        public static readonly int Port;
        public static readonly bool IsPublicBuild;
        public static readonly bool HasFrameRate;
        public static readonly int FrameRate;
        public static readonly bool UseRandomDeviceID;
        public static readonly bool HasCustomDeviceID;
        public static readonly string CustomDeviceID;
        public static readonly bool GenerateInput;

        static ApplicationSettings()
        {
            IsPublicBuild = PhotonAppSettings.Global.AppSettings.AppVersion.ToLowerInvariant().Contains("-public");
        }
    }
}
