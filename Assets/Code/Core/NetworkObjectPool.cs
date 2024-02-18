using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    public class NetworkObjectPool : INetworkObjectPool
    {
        public BaseContext Context { get; set; }
        private Dictionary<NetworkPrefabId, Stack<NetworkObject>> _cached = new Dictionary<NetworkPrefabId, Stack<NetworkObject>>(32);
        private Dictionary<NetworkObject, NetworkPrefabId> _borrowed = new Dictionary<NetworkObject, NetworkPrefabId>();
        NetworkObject INetworkObjectPool.AcquireInstance(Fusion.NetworkRunner runner, Fusion.NetworkPrefabInfo info)
        {
            if (_cached.TryGetValue(info.Prefab, out var objects)== false)
            {
                objects = _cached[info.Prefab] = new Stack<NetworkObject>();
            }
            if (objects.Count > 0)
            { 
                var oldInstance = objects.Pop();
                _borrowed[oldInstance] = info.Prefab;
                if (oldInstance == null)
                {
                    oldInstance.gameObject.SetActive(true);
                }
                return oldInstance;
            }
            if (runner.Config.PrefabTable.TryGetPrefab(info.Prefab,out var original) == false)
                return null;

            var instance = Object.Instantiate(original);
            _borrowed[instance] = info.Prefab;

            AssingContext(instance);
            for (int i = 0; i < instance.NestedObjects.Length; i++)
            {
                AssingContext(instance.NestedObjects[i]);
            }
            return instance;
        }

        void INetworkObjectPool.ReleaseInstance(Fusion.NetworkRunner runner, Fusion.NetworkObject instance, bool isSceneObject) 
        {
            if (isSceneObject == false && runner.IsShutdown == false)
            {
                if (_borrowed.TryGetValue(instance, out var prefabID) == true)
                {
                    _borrowed.Remove(instance);
                    _cached[prefabID].Push(instance);

                    if (instance != null)
                    {
                        instance.gameObject.SetActive(false);
                        instance.transform.parent = null;
                        instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    }
                }
                else
                {
                    Object.Destroy(instance.gameObject);
                }
            }
            else 
            {
                Object.Destroy(instance.gameObject);
            }
        }
        private void AssingContext(NetworkObject instance) 
        {
            for (int i = 0; i < instance.NetworkedBehaviours.Length; i++)
            {
                if (instance.NetworkedBehaviours[i] is IContextBehaviour behaviour)
                {
                    behaviour.Context = Context;
                }
            }
            for (int i = 0; i < instance.SimulationBehaviours.Length; i++)
            {
                if (instance.SimulationBehaviours[i] is IContextBehaviour behaviour)
                {
                    behaviour.Context = Context;
                }
            }
        }
    }
}
