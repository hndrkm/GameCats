using UnityEngine;
using System;
using Fusion;

namespace CatGame
{
    [Serializable]
    [CreateAssetMenu(fileName = "GlobalSettings", menuName ="CatGame/Global Settings")]
    public class GlobalSettings : ScriptableObject
    {
        public NetworkRunner RunnerPrefab;
        public string LoadingScene = "LoadingScene";
        public string MenuScene = "Menu";

        public AgentSettings Agent;
        public MapSettings Map;
        public NetworkSettings Network;
        public OptionsData DefaultOptions;
    }
}
