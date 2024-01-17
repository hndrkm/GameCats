using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace CatGame
{
    public struct PlayerStatistics : INetworkStruct
    { 
        public PlayerRef PlayerRef;
        public short Extralives;
        public short Kills;
        public short Deaths;
        public short Score;
        public TickTimer RespawnTimer;
        public byte Position;

        public bool IsValid => PlayerRef.IsRealPlayer;
        public bool IsAlive { get { return _flags.IsBitSet(0); } set { _flags.SetBit(0, value); } }
        public bool IsEliminated { get { return _flags.IsBitSet(1); } set { _flags.SetBit(1, value); } }
        private byte _flags;
    }
    public class Player : ContextBehaviour
    {
        public bool IsInitialized { get; private set; }
        public string UserID { get; private set; }
        [Networked , Capacity(24)]
        public string Nickname { get; private set; }
    }
}
