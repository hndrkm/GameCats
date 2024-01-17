//#define ENABLE_LOGS
using Fusion;

namespace CatGame
{
    using CatGame;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.SceneManagement;

    using UnityScene = UnityEngine.SceneManagement.Scene;

    public sealed class NetworkSceneManager : NetworkSceneManagerDefault { 
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

        
    }
}
