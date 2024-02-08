//#define ENABLE_LOGS
using Fusion;

namespace CatGame
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor.SearchService;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.SceneManagement;
    using UnityScene = UnityEngine.SceneManagement.Scene;

    public class NetworkSceneManager : NetworkSceneManagerDefault { 
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

        public override void Initialize(NetworkRunner runner)
        {
            base.Initialize(runner);
            _runner = runner;
            _sceneObjects.Clear();
            _currentScene = SceneRef.None; 
            _gameplayScene = null;
            Log("inicio scene manager");
        }
        public override void Shutdown()
        {
            if (_loadingInstance == _instanceID)
            {
                Log($"parandola carga de la escena");

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
            base.Shutdown();
        }
        private void Awake()
        {
            _instanceID = GetInstanceID();
        }
        private void LateUpdate()
        {

        }
        protected override IEnumerator LoadSceneCoroutine(SceneRef sceneRef, NetworkLoadSceneParameters sceneParams)
        {
            Debug.Log($"cargando la Scena {sceneRef}");
            yield return base.LoadSceneCoroutine(sceneRef, sceneParams);
        }
        protected override IEnumerator OnSceneLoaded(SceneRef sceneRef, UnityScene scene, NetworkLoadSceneParameters sceneParams)
        {
            _currentScene = sceneRef;
            _gameplayScene = scene.GetComponent<Base>(true);
            _activationScene = _gameplayScene;
            _activationTimeout = Time.realtimeSinceStartup + 10.0f;
            Debug.Log($"cargo la Scena {scene.name}");
            float contextTimeout = 20.0f;

            while (_gameplayScene.ContextReady == false && contextTimeout > 0.0f)
            {
                Log($"Waiting for scene context");
                yield return null;
                contextTimeout -= Time.unscaledDeltaTime;
            }

            if (_gameplayScene.ContextReady == false)
            {
                _currentScene = SceneRef.None;
                _gameplayScene = null;

                Debug.LogError($"Scene context is not ready (timeout)!");
                //FinishSceneLoading();
                yield break;
            }
            var contextBehaviours = scene.GetComponents<IContextBehaviour>(true);
            foreach (var behaviurs in contextBehaviours)
            {

                behaviurs.Context = _gameplayScene.Context;
            }
            yield return base.OnSceneLoaded(sceneRef, scene, sceneParams);
            
            //_runner.RegisterSceneObjects
        }
        private void Log(string message)
        {
            Debug.Log($"[{Time.frameCount}] NetworkSceneManager({_instanceID}): {message}");
        }
    }
}
