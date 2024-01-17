using System;
using UnityEngine;

namespace CatGame
{
    [Serializable]
    public class RegionInfo
    {
        public string DisplayName;
        public string Region;
        public Sprite Icon;
    }
    [Serializable]
    [CreateAssetMenu(fileName = "NetworkSettings", menuName = "CatGame/Network Settings")]
    public class NetworkSettings : ScriptableObject
    {
        public RegionInfo[] Regions;
        public RegionInfo GetRegionInfo(string region) 
        {
            return Regions.Find(x => x.Region == region);
        }
    }
}
