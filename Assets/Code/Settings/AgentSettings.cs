using UnityEngine;
using System;
using Fusion;
using ExitGames.Client.Photon.StructWrapping;

namespace CatGame
{
    [Serializable]
    [CreateAssetMenu(fileName = "AgentSettings", menuName = "CatGame/Agent Settings")]
    public class AgentSettings : ScriptableObject
    {
        public AgentSetup[] Agents => _agents;

        [SerializeField]
        private AgentSetup[] _agents;

        public AgentSetup GetAgentSetup(string agentID)
        {
            if (agentID.HasValue() == false)
                return null;

            return _agents.Find(t => t.ID == agentID);
        }

        public AgentSetup GetAgentSetup(NetworkPrefabId prefabId)
        {
            if (prefabId.IsValid == false)
                return null;

            return _agents.Find(t => t.AgentPrefabId == prefabId);
        }

        public AgentSetup GetRandomAgentSetup()
        {
            return _agents[UnityEngine.Random.Range(0, _agents.Length)];
        }
    }
    [Serializable]
    public class AgentSetup
    {
        public string ID => _id;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public GameObject AgentPrefab => _agentPrefab;
        public GameObject MenuAgentPrefab => _menuAgentPrefab;

        public NetworkPrefabId AgentPrefabId
        {
            get
            {
                var Guid = NetworkProjectConfig.Global.PrefabTable.GetGuid(_agentPrefab.GetComponent<NetworkObject>().NetworkTypeId.AsPrefabId);
                var id = NetworkProjectConfig.Global.PrefabTable.GetId(Guid);
                if (_agentPrefabId.IsValid == false && id.IsValid == true)
                {
                    _agentPrefabId = id;
                }

                return _agentPrefabId;
            }
        }

        [SerializeField]
        private string _id;
        [SerializeField]
        private string _displayName;
        [SerializeField, TextArea(3, 6)]
        private string _description;
        [SerializeField]
        private Sprite _icon;
        [SerializeField]
        private GameObject _agentPrefab;
        [SerializeField]
        private GameObject _menuAgentPrefab;

        [NonSerialized]
        private NetworkPrefabId _agentPrefabId;
    }
}
