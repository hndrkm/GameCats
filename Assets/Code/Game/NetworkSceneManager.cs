//#define ENABLE_LOGS
using Fusion;

namespace CatGame
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.SceneManagement;
    using UnityScene = UnityEngine.SceneManagement.Scene;

    public class NetworkSceneManager : Fusion.Behaviour, INetworkSceneManager, INetworkSceneManagerObjectResolver
    { 
        public Base GameplayScene => _gameplayScene;

        private NetworkRunner _runner;
        private Dictionary<Guid, NetworkObject> _sceneObjects = new Dictionary<Guid, NetworkObject>();
        private SceneRef _currentScene;
        private Base _gameplayScene;
        private int _instanceID;

        private static int _loadingInstance;
        private static Coroutine _loadingCoroutine;
        private static Base _activationScene;
        private static float _activationTimeout;

        void INetworkSceneManager.Initialize(NetworkRunner runner)
        {
            _runner = runner;
            _sceneObjects.Clear();
            _currentScene = SceneRef.None; 
            _gameplayScene = null;
            Log("inicio scene manager");
        }
        void INetworkSceneManager.Shutdown(NetworkRunner runner)
        {
            if (_loadingInstance == _instanceID)
            {
                Log($"parando carga de escena");

                try
                {
                    if (_loadingCoroutine != null)
                    {
                        Log($"Stopping coroutine");
                        StopCoroutine(_loadingCoroutine);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }

                _loadingInstance = default;
                _loadingCoroutine = default;
            }

            Log($"Shutdown");

            _runner = null;
            _sceneObjects.Clear();
            _currentScene = SceneRef.None;
            _gameplayScene = null;
        }
        bool INetworkSceneManager.IsReady(Fusion.NetworkRunner runner) 
        {
            if (_loadingInstance == _instanceID)
                return false;
            if(_gameplayScene == null||_gameplayScene.ContextReady == false)
                return false;
            if(_currentScene != _runner.CurrentScene)
                return false;
            return true;
        }
        bool INetworkSceneManagerObjectResolver.TryResolveSceneObject(NetworkRunner runner, Guid sceneObjectGuid, out NetworkObject instance) 
        {
            if (_sceneObjects.TryGetValue(sceneObjectGuid, out instance) == false) 
            {
                return false;
            }
            return true;
        }
        private void Awake()
        {
            _instanceID = GetInstanceID();
        }
        private void LateUpdate()
        {
            if (_runner == null)
                return;
            if (_loadingCoroutine != null)
                return;
            if (_currentScene == _runner.CurrentScene)
                return;
            if (Time.realtimeSinceStartup < _activationTimeout && _activationScene != null && _activationScene.IsActive == false)
                return;
            _activationScene = null;
            _loadingInstance = _instanceID;
            _loadingCoroutine = StartCoroutine(SwitchSceneCoroutine(_runner, _currentScene,_runner.CurrentScene));
        }
        private IEnumerator SwitchSceneCoroutine(NetworkRunner runner,SceneRef fromScene, SceneRef toScene)
        {
            _currentScene = SceneRef.None;
            _gameplayScene = null;

            try 
            {
                runner.InvokeSceneLoadStart();
            }catch (Exception ex) 
            {
                Debug.LogException(ex);
                yield break;
            }

            if (runner.Config.PeerMode == NetworkProjectConfig.PeerModes.Single)
            {
                UnityScene loadedScene = default;
                UnityScene activeScene = SceneManager.GetActiveScene();

                bool canTakeOverActiveScene = fromScene == default && IsScenePathOrNameEqual(activeScene, toScene);
                if (canTakeOverActiveScene == true)
                {
                    loadedScene = activeScene;
                }
                else 
                {
                    if (TryGetScenePathFromBuildSetting(toScene, out string scenePath) == false)
                    {
                        Debug.LogError($"scene no encontrada {toScene}");
                        FinishSceneLoading();
                        yield break;
                    }
                    UnityAction<UnityScene, LoadSceneMode> onSceneLoaded = (scene, loadSceneMode) =>
                    {
                        if (loadedScene == default && IsScenePathOrNameEqual(scene, scenePath) == true)
                            loadedScene = scene;
                    };
                    SceneManager.sceneLoaded += onSceneLoaded;
                    yield return SceneManager.LoadSceneAsync(scenePath, new LoadSceneParameters(LoadSceneMode.Additive));
                    float timeout = 2.0f;
                    while (timeout > 0.0f && loadedScene.IsValid() == false)
                    { 
                        yield return null;
                        timeout -= Time.unscaledDeltaTime;
                    }
                    SceneManager.sceneLoaded -= onSceneLoaded;

                    if (loadedScene.IsValid() == false)
                    {
                        FinishSceneLoading();
                        yield break;
                    }
                    Log($"Scena Cargada{loadedScene.name}");
                    SceneManager.SetActiveScene( loadedScene);
                }
                FindNetworkObjects(loadedScene,true,false);
            }

            _currentScene = runner.CurrentScene;
            _gameplayScene = runner.SimulationUnityScene.GetComponent<Base>(true);
            _activationScene = _gameplayScene;
            _activationTimeout = Time.realtimeSinceStartup + 10.0f;

            float contextTimeout = 20.0f;

            while (_gameplayScene.ContextReady == false && contextTimeout > 0.0f)
            {
                Log($"Esperando el contexto de escena");    
                yield return null;
                contextTimeout -= Time.unscaledDeltaTime;
            }

            if (_gameplayScene.ContextReady == false)
            {
                _currentScene = SceneRef.None;
                _gameplayScene = null;

                Debug.LogError($"Scene context is not ready (timeout)!");
                FinishSceneLoading();
                yield break;
            }
            var contextBehaviours = runner.SimulationUnityScene.GetComponents<IContextBehaviour>(true);
            foreach (var behaviurs in contextBehaviours)
            {
                behaviurs.Context = _gameplayScene.Context;
            }
            try
            {
                runner.RegisterSceneObjects(_sceneObjects.Values);
                runner.InvokeSceneLoadDone();
            }
            catch(Exception ex) 
            {
                Debug.LogException(ex);
                FinishSceneLoading();
                yield break;
            }
            FinishSceneLoading();
        }
        private void FindNetworkObjects(UnityScene scene, bool disable, bool addVisibilityNodes) 
        {
            _sceneObjects.Clear();
            List<NetworkObject> networkObjects = new List<NetworkObject>();
            foreach (GameObject rootGameObjects in scene.GetRootGameObjects())
            {
                networkObjects.Clear();
                rootGameObjects.GetComponentsInChildren(true, networkObjects);
                foreach (NetworkObject networkObject in networkObjects)
                {
                    if (networkObject.Flags.IsSceneObject() == true)
                    {
                        if (networkObject.gameObject.activeInHierarchy == true || networkObject.Flags.IsActivatedByUser() == true)
                        {
                            _sceneObjects.Add(networkObject.NetworkGuid, networkObject);
                            Log($"objeto encontrado en la scena {networkObject.name}({networkObject.NetworkGuid})");
                        }
                    }
                }
                if (addVisibilityNodes == true) 
                {
                    RunnerVisibilityNode.AddVisibilityNodes(rootGameObjects,_runner);
                }
            }
            if (disable == true)
            {
                foreach (NetworkObject sceneObject in _sceneObjects.Values)
                {
                    sceneObject.gameObject.SetActive(false);
                }
            }
        }
        private void FinishSceneLoading() 
        {
            Log("Carga de Escena finalizada");
            _loadingInstance = default;
            _loadingCoroutine = default;
        }

        private void Log(string message)
        {
            Debug.Log($"[{Time.frameCount}] NetworkSceneManager({_instanceID}): {message}");
        }
        private static bool IsScenePathOrNameEqual(UnityScene scene, string nameOrPath) 
        {
            return scene.path == nameOrPath || scene.name== nameOrPath;
        }
        private static bool IsScenePathOrNameEqual(UnityScene scene, SceneRef sceneRef) 
        {
            return TryGetScenePathFromBuildSetting(sceneRef,out var path) == true ? IsScenePathOrNameEqual(scene,path) : false;
        }
        private static bool TryGetScenePathFromBuildSetting(SceneRef sceneRef,out string path)
        {
            if (sceneRef.IsValid == true)
            {
                path = SceneUtility.GetScenePathByBuildIndex(sceneRef);
                if(string.IsNullOrEmpty(path) == false)
                    return true;
            }
            path = string.Empty;
            return false;
        }
    }
}
