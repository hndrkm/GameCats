using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    public class NetworkObjectPool : NetworkObjectProviderDefault
    {
        public BaseContext Context { get; set; } 
        protected override NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab)
        {
            Debug.Log($"instancia ...{prefab.Name}");
            var instance = base.InstantiatePrefab(runner, prefab);
            AssingContext(instance);
            return instance;
        }

        private void AssingContext(NetworkObject instance) 
        {
            for (int i = 0; i < instance.NetworkedBehaviours.Length; i++)
            {
                if (instance.NetworkedBehaviours[i] is IContextBehaviour behaviour)
                {
                    Debug.Log(Context == null);
                    behaviour.Context = Context;
                }
            }
        }
    }
}
