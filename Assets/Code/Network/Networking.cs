#define ENABLE_LOGS

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

using UnityEngine.SceneManagement;
using Fusion;

using Fusion.Sockets;

using UnityScene = UnityEngine.SceneManagement.Scene;
using static System.Collections.Specialized.BitVector32;

namespace CatGame
{
    public struct SessionRequest
    {
        public string UserID;
        public GameMode GameMode;
        public string DisplayName;
        public string SessionName;
        public string ScenePath;
        public EGameplayType GameplayType;
        public int MaxPlayers;
        public int ExtraPeers;
        public string CustomLobby;
        public string IPAddress;
        public ushort Port;
    }
    public class Networking : MonoBehaviour
    {
        public const string DISPLAY_NAME_KEY = "name";
        public const string MAP_KEY = "map";
        public const string TYPE_KEY = "type";
        public const string MODE_KEY = "mode";
        public const string STATUS_SERVER_CLOSED = "Server Closed";

        public string Status { get; private set; }
        public string StatusDescription { get; private set; }
        public string ErrorStatus { get; private set; }

        public bool HasSession => _pendingSession != null || _currentSession != null;
        public bool IsConnecting => _pendingSession != null || _currentSession.IsConnected == false;
        public bool IsConnected => _currentSession != null && _pendingSession == null && _currentSession.IsConnected == true;

        public int PeerCount => _currentSession != null ? _currentSession.GamePeers.SafeCount() : 0;

        private Session _pendingSession;
        private Session _currentSession;
        private bool _stopGameOnDisconnect;
        private string _loadingScene;
        private Coroutine _coroutine;

        public void StartGame(SessionRequest request)
        {
            var session = new Session();

            if (request.ExtraPeers > 0 && NetworkProjectConfig.Global.PeerMode == NetworkProjectConfig.PeerModes.Single)
            {
                Debug.LogError("No se puede comenzar con varios Peers. PeerMode esta configurado en unico.");
                request.ExtraPeers = 0;
            }
            Debug.Log(request.ScenePath);
            SceneRef sceneRef = SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath(request.ScenePath));

            int totalPeers = 1 + request.ExtraPeers;
            session.GamePeers = new GamePeer[totalPeers];

            for (int i = 0; i < totalPeers; i++)
            {
                session.GamePeers[i] = new GamePeer(i)
                {
                    UserID = i == 0 ? request.UserID : $"{request.UserID}.{i}",
                    Scene = sceneRef,
                    GameMode = i == 0 ? request.GameMode : GameMode.Client,
                    Request = request,
                };
            }

            session.ConnectionRequested = true;

            _pendingSession = session;
            _stopGameOnDisconnect = false;

            ErrorStatus = null;

            Log($"StartGame() UserID:{request.UserID} GameMode:{request.GameMode} DisplayName:{request.DisplayName} SessionName:{request.SessionName} ScenePath:{request.ScenePath} GameplayType:{request.GameplayType} MaxPlayers:{request.MaxPlayers} ExtraPeers:{request.ExtraPeers} CustomLobby:{request.CustomLobby}");
        }
        public void StopGame(string errorStatus = null)
        {
            Log($"StopGame()");
            _pendingSession = null;
            _stopGameOnDisconnect = false;
            if (_currentSession != null)
            {
                _currentSession.ConnectionRequested = false;
            }
            ErrorStatus = errorStatus;
        }
        public void StopGameOnDisconnect()
        {
            Log($"StopGameOnDisconnect()");
            _stopGameOnDisconnect = true;
        }
        public void ClearErrorStatus()
        {
            ErrorStatus = null;
        }
        protected void Awake()
        {
            _loadingScene = Global.Settings.LoadingScene;
        }
        protected void Update()
        {
            if (_pendingSession != null)
            {
                if (_currentSession == null)
                {
                    _currentSession = _pendingSession;
                    _pendingSession = null;
                }
                else
                {
                    _currentSession.ConnectionRequested = false;
                }
            }

            UpdateCurrentSession();
            if (_coroutine == null && _currentSession != null && _currentSession.IsConnected == false)
            {
                if (_pendingSession == null)
                {
                    Log($"empezando LoadMenuCoroutine()");
                    _coroutine = StartCoroutine(LoadMenuCoroutine());
                }

                _currentSession = null;
            }
        }
        public void UpdateCurrentSession()
        {
            if (_currentSession == null)
            {
                Status = string.Empty;
                StatusDescription = string.Empty;
                return;
            }

            if (_coroutine != null)
                return;

            var peers = _currentSession.GamePeers;

            if (_stopGameOnDisconnect == true)
            {
                for (int i = 0; i < peers.Length; i++)
                {
                    if (_currentSession.ConnectionRequested == true && peers[i].IsConnected == false)
                    {
                        Log($"Stopping game after disconnect");
                        _stopGameOnDisconnect = false;
                        StopGame();
                        return;
                    }
                }
            }

            for (int i = 0; i < peers.Length; i++)
            {
                var peer = peers[i];
                bool isConnected = peer.IsConnected;

                if (_currentSession.ConnectionRequested == true && peer.Loaded == false && isConnected == false && peer.CanConnect == true)
                {
                    Status = peer.WasConnected == false ? "Starting" : "Reconnecting";
                    Log($"Starting ConnectPeerCoroutine() - {Status} - Peer {peer.ID}");
                    _coroutine = StartCoroutine(ConnectPeerCoroutine(peer));
                    return;
                }
                else if (_currentSession.ConnectionRequested == false && (isConnected == true || peer.Loaded == true))
                {
                    Status = "Saliendo";
                    Log($"Starting DisconnectPeerCoroutine() - {Status} - Peer {peer.ID}");
                    _coroutine = StartCoroutine(DisconnectPeerCoroutine(peer));
                    return;
                }
                else if (peer.Loaded == true && isConnected == false)
                {
                    // perdida

                    Status = "Connexion perdida";
                    Log($"Starting DisconnectPeerCoroutine() - {Status} - Peer {peer.ID}");
                    _coroutine = StartCoroutine(DisconnectPeerCoroutine(peer));
                    return;
                }
            }

            
        }
        private IEnumerator ConnectPeerCoroutine(GamePeer peer, float connectionTimeout = 10f, float loadTimeout = 45f)
        {
            peer.Loaded = true;

            if (peer.WasConnected == true)
                peer.ReconnectionTries--;
            else
                peer.ConnectionTries--;
            StatusDescription = "descargando scene actual";

            UnityScene activeScene = SceneManager.GetActiveScene();
            if (IsSameScene(activeScene.path, peer.Request.ScenePath) == false && activeScene.name != _loadingScene)
            {
                Log($"Mostrar loading scene");
                yield return ShowLoadingSceneCoroutine(true);

                var currentScene = activeScene.GetComponent<Base>();
                if (currentScene != null)
                {
                    Log($"desinit Scene");
                    currentScene.Deinitialize();
                }
                Log($"descargando scene {activeScene.name}");
                yield return SceneManager.UnloadSceneAsync(activeScene);
                yield return null;
            }

            float baseTime = Time.realtimeSinceStartup;
            float limitTime = baseTime + connectionTimeout;
            string peerName = $"{peer.GameMode}#{peer.ID}";

            Debug.LogWarning($"Starting {peerName} ...");
            StatusDescription = "Empezando connection network";

            yield return null;

            NetworkObjectProviderDefault pool = new NetworkObjectProviderDefault();

            NetworkRunner runner = Instantiate(Global.Settings.RunnerPrefab);
            runner.name = peerName;

            peer.Runner = runner;
            peer.SceneManager = runner.GetComponent<NetworkSceneManager>();
            peer.LoadedScene = default;
            NetworkSceneInfo sceneInfo = new NetworkSceneInfo();
            sceneInfo.AddSceneRef(peer.Scene,LoadSceneMode.Single);
            StartGameArgs startGameArgs = new StartGameArgs();
            startGameArgs.GameMode = peer.GameMode;
            startGameArgs.SessionName = peer.Request.SessionName;
            startGameArgs.Scene = sceneInfo;
            startGameArgs.EnableClientSessionCreation = false;
            startGameArgs.ObjectProvider = pool;
            startGameArgs.CustomLobbyName = peer.Request.CustomLobby;
            startGameArgs.SceneManager = peer.SceneManager;

            if (peer.Request.MaxPlayers > 0)
            {
                startGameArgs.PlayerCount = peer.Request.MaxPlayers;
            }

            if (peer.GameMode == GameMode.Server || peer.GameMode == GameMode.Host)
            {
                startGameArgs.SessionProperties = CreateSessionProperties(peer.Request);
            }
            Log($"NetworkRunner.StartGame()");
            var startGameTask = runner.StartGame(startGameArgs);

            while (startGameTask.IsCompleted == false)
            {
                yield return null;

                if (Time.realtimeSinceStartup >= limitTime)
                {
                    Debug.LogError($"{peerName} start timeout! IsCompleted: {startGameTask.IsCompleted} IsCanceled: {startGameTask.IsCanceled} IsFaulted: {startGameTask.IsFaulted}");
                    break;
                }

                if (_currentSession.ConnectionRequested == false)
                {
                    Log($"Detener la rutina (solicitada por el usuario)");
                    break;
                }
            }

            if (startGameTask.IsCanceled == true || startGameTask.IsFaulted == true || startGameTask.IsCompleted == false)
            {
                Debug.LogError($"{peerName} failed to start!");

                Log($"Starting DisconnectPeerCoroutine() - Peer {peer.ID}");
                yield return DisconnectPeerCoroutine(peer);

                _coroutine = null;
                yield break;
            }

            var result = startGameTask.Result;

            Log($"StartGame() Result: {result.ToString()} - Peer {peer.ID}");

            if (result.Ok == false)
            {
                Debug.LogError($"{peerName}Error al iniciar Result: {result}");

                if (Application.isBatchMode == false)
                {
                    StopGame();
                }

                if (peer.WasConnected == true && result.ShutdownReason == ShutdownReason.GameNotFound)
                {
                    ErrorStatus = STATUS_SERVER_CLOSED;
                }
                else
                {
                    ErrorStatus = StringToLabel(result.ShutdownReason.ToString());
                }

                Log($"Starting DisconnectPeerCoroutine() - Peer {peer.ID}");
                yield return DisconnectPeerCoroutine(peer);

                _coroutine = null;
                yield break;
            }

            limitTime += loadTimeout;

            Log($"Esperando la conexión - Peer {peer.ID}");
            StatusDescription = "Waiting for server connection";

            while (peer.IsConnected == false)
            {
                yield return null;

                if (Time.realtimeSinceStartup >= limitTime)
                {
                    Debug.LogError($"{peerName} start timeout! IsCloudReady: {runner.IsCloudReady} IsRunning: {runner.IsRunning}");

                    Log($"Starting DisconnectPeerCoroutine() - Peer {peer.ID}");
                    yield return DisconnectPeerCoroutine(peer);

                    _coroutine = null;
                    yield break;
                }
            }

            Log($"Cargando escena de juego - Peer {peer.ID}");
            StatusDescription = "Cargando escena de juego";

            while (runner.SimulationUnityScene.IsValid() == false || runner.SimulationUnityScene.isLoaded == false)
            {
                Log($"Esperando NetworkRunner.SimulationUnityScene - Peer {peer.ID}");
                yield return null;

                if (Time.realtimeSinceStartup >= limitTime)
                {
                    Debug.LogError($"{peerName} scene load timeout!");

                    Log($"Starting DisconnectPeerCoroutine() - Peer {peer.ID}");
                    yield return DisconnectPeerCoroutine(peer);

                    _coroutine = null;
                    yield break;
                }
            }

            Debug.LogWarning($"{peerName} started on {runner.SessionInfo.Region} in {(Time.realtimeSinceStartup - baseTime):0.00}s");

            peer.LoadedScene = runner.SimulationUnityScene;

            if (peer.ID == 0)
            {
                SceneManager.SetActiveScene(peer.LoadedScene);
            }

            StatusDescription = "Esperando que se cargue la escena del juego";

            var scene = peer.SceneManager.GameplayScene;
            while (scene == null)
            {
                Log($"Waiting for GameplayScene - Peer {peer.ID}");

                yield return null;
                scene = peer.SceneManager.GameplayScene;

                if (Time.realtimeSinceStartup >= limitTime)
                {
                    Debug.LogError($"{peerName} GameplayScene Tiempo vencido de consulta");

                    Log($"Starting DisconnectPeerCoroutine() - Peer {peer.ID}");
                    yield return DisconnectPeerCoroutine(peer);

                    _coroutine = null;
                    yield break;
                }
            }

            Log($"Scene.PrepareContext() - Peer {peer.ID}");
            scene.PrepareContext();

            var sceneContext = scene.Context;
            sceneContext.IsVisible = peer.ID == 0;
            sceneContext.HasInput = peer.ID == 0;
            sceneContext.Runner = peer.Runner;
            sceneContext.PeerUserID = peer.UserID;

            peer.Context = sceneContext;
            //pool.Context = sceneContext;

            StatusDescription = "Esperando juego en red";

            var networkGame = scene.GetComponentInChildren<NetworkGame>(true);

            while (networkGame.Object == null || networkGame.Context == null)
            {
                Log($"Esperando NetworkGame - Peer {peer.ID}");

                yield return null;

                if (Time.realtimeSinceStartup >= limitTime)
                {
                    Debug.LogError($"{peerName} tiempo de espera. El juego en red no se inició correctamente.");

                    Log($"Starting DisconnectPeerCoroutine() - Peer {peer.ID}");
                    yield return DisconnectPeerCoroutine(peer);

                    _coroutine = null;
                    yield break;
                }

                if (_currentSession.ConnectionRequested == false)
                {

                    Log($"Starting DisconnectPeerCoroutine() - Ya no se solicita conexión - Peer {peer.ID}");
                    yield return DisconnectPeerCoroutine(peer);

                    _coroutine = null;
                    yield break;
                }
            }

            StatusDescription = "Esperando que se cargue el juego";

            Log($"NetworkGame.Initialize() - Peer {peer.ID}");
            networkGame.Initialize(peer.Request.GameplayType);

            while (scene.Context.GameplayMode == null)
            {
                Log($"Esperando GameplayMode - Peer {peer.ID}");

                yield return null;

                if (Time.realtimeSinceStartup >= limitTime)
                {
                    Debug.LogError($"{peerName}  tiempo de espera El modo de juego no se inicio correctamente");

                    Log($"Starting DisconnectPeerCoroutine() - Peer {peer.ID}");
                    yield return DisconnectPeerCoroutine(peer);

                    _coroutine = null;
                    yield break;
                }
            }

            StatusDescription = "Activando scene";

            Log($"Scene.Initialize() - Peer {peer.ID}");
            scene.Initialize();

            Log($"Scene.Activate() - Peer {peer.ID}");
            yield return scene.Activate();

            StatusDescription = "Activando juego en red";

            Log($"NetworkGame.Activate() - Peer {peer.ID}");
            networkGame.Activate();

            if (SceneManager.GetSceneByName(_loadingScene).IsValid() == true)
            {

                yield return new WaitForSeconds(1f);

                Log($"Ocultar escena de carga");
                yield return ShowLoadingSceneCoroutine(false);
            }

            if (peer.WasConnected == true)
            {
                peer.ReconnectionTries++;
            }

            peer.WasConnected = true;

            _coroutine = null;

            Log($"ConnectPeerCoroutine() finished");
        }



        private IEnumerator DisconnectPeerCoroutine(GamePeer peer)
        {
            StatusDescription = "Desconectandose del servidor";
            UnityScene gameplayScene = default;

            try
            {
                if (peer.Runner != null)
                {
                    // Posible excepcion cuando el Runner intenta leer la configuración
                    gameplayScene = peer.Runner.SimulationUnityScene;
                    // Cierra y oculta la room
                    if (peer.Runner.IsServer == true && peer.Runner.SessionInfo != null)
                    {
                        Log($"cerrando la room");
                        peer.Runner.SessionInfo.IsOpen = false;
                        peer.Runner.SessionInfo.IsVisible = false;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            if (gameplayScene.IsValid() == false)
            {
                gameplayScene = peer.LoadedScene;
            }

            if (gameplayScene.IsValid() == true)
            {
                Base scene = gameplayScene.GetComponent<Base>(true);
                if (scene != null)
                {
                    try
                    {
                        Log($"Escena en desinicializacion");
                        scene.Deinitialize();
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception);
                    }
                }
            }

            Task shutdownTask = null;

            if (peer.Runner != null)
            {
                Debug.LogWarning($"Shutdown {peer.Runner.name} ...");

                try
                {
                    shutdownTask = peer.Runner.Shutdown(true);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }

            Log($"Mostrar escena de carga");
            yield return ShowLoadingSceneCoroutine(true);

            if (shutdownTask != null)
            {
                float operationTimeout = 10.0f;
                while (operationTimeout > 0.0f && shutdownTask.IsCompleted == false)
                {
                    yield return null;
                    operationTimeout -= Time.unscaledDeltaTime;
                }
            }

            StatusDescription = "Descargando escena de juego";

            yield return null;

            if (gameplayScene.IsValid() == true)
            {
                Debug.LogWarning($"escena de descarga {gameplayScene.name}");

                yield return SceneManager.UnloadSceneAsync(gameplayScene);
                yield return null;
            }

            peer.Loaded = default;
            peer.Runner = default;
            peer.SceneManager = default;
            peer.LoadedScene = default;

            _coroutine = null;

            Log($"DisconnectPeerCoroutine() Terminado");
        }
        private IEnumerator ShowLoadingSceneCoroutine(bool show, float additionalTime = 1f)
        {
            var loadingScene = SceneManager.GetSceneByName(_loadingScene);

            if (loadingScene.IsValid() == false)
            {
                yield return SceneManager.LoadSceneAsync(_loadingScene, LoadSceneMode.Additive);
                loadingScene = SceneManager.GetSceneByName(_loadingScene);
            }

            if (show == false && additionalTime > 0f)
            {
                // Espere un tiempo hasta que comience el desvanecimiento
                yield return new WaitForSeconds(additionalTime);
            }

            yield return null;

            var loadingSceneObject = loadingScene.GetComponent<LoadingScene>();
            if (loadingSceneObject != null)
            {
                if (show == true)
                {
                    loadingSceneObject.FadeIn();
                }
                else
                {
                    loadingSceneObject.FadeOut();
                }

                while (loadingSceneObject.IsFading == true)
                    yield return null;
            }
            Debug.Log("se o");
            if (show == true && additionalTime > 0f)
            {
                // Espere un tiempo que aparezca gradualmente
                yield return new WaitForSeconds(additionalTime);
            }

            if (show == false)
            {
                yield return SceneManager.UnloadSceneAsync(loadingScene);
            }
        }
        private IEnumerator LoadMenuCoroutine()
        {
            string menuSceneName = Global.Settings.MenuScene;

            if (SceneManager.sceneCount == 1 && SceneManager.GetSceneAt(0).name == menuSceneName)
            {
                _coroutine = null;
                yield break;
            }

            StatusDescription = "descargando escenas del gameplay";

            yield return ShowLoadingSceneCoroutine(true);

            for (int i = SceneManager.sceneCount - 1; i >= 0; --i)
            {
                var scene = SceneManager.GetSceneAt(i);

                if (scene.name != _loadingScene)
                {
                    yield return SceneManager.UnloadSceneAsync(scene);
                }
            }

            StatusDescription = "cargando escena del menu";
            yield return null;

            yield return SceneManager.LoadSceneAsync(menuSceneName, LoadSceneMode.Additive);
            yield return ShowLoadingSceneCoroutine(false);

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(menuSceneName));

            _coroutine = null;
        }

        private Dictionary<string, SessionProperty> CreateSessionProperties(SessionRequest request)
        {
            var dictionary = new Dictionary<string, SessionProperty>();

            dictionary[DISPLAY_NAME_KEY] = request.DisplayName;
            dictionary[MAP_KEY] = Global.Settings.Map.GetMapIndexFromScenePath(request.ScenePath);
            dictionary[TYPE_KEY] = (int)request.GameplayType;
            dictionary[MODE_KEY] = (int)request.GameMode;

            return dictionary;
        }

        [System.Diagnostics.Conditional("ENABLE_LOGS")]
        private void Log(string message)
        {
            Debug.Log($"[{Time.realtimeSinceStartup:F3}][{Time.frameCount}] Networking({GetInstanceID()}): {message}");
        }

        private static string StringToLabel(string myString)
        {
            var label = System.Text.RegularExpressions.Regex.Replace(myString, "(?<=[A-Z])(?=[A-Z][a-z])", " ");
            label = System.Text.RegularExpressions.Regex.Replace(label, "(?<=[^A-Z])(?=[A-Z])", " ");

            return label;
        }

        private static bool IsSameScene(string assetPath, string scenePath)
        {
            return assetPath == $"Assets/{scenePath}.unity";
        }
        private sealed class GamePeer
        {
            public int ID;
            public SceneRef Scene;
            public BaseContext Context;
            public GameMode GameMode;
            public NetworkRunner Runner;
            public NetworkSceneManager SceneManager;
            public UnityScene LoadedScene;
            public string UserID;
            public SessionRequest Request;
            public int ConnectionTries = 3;
            public int ReconnectionTries = 1;

            public bool Loaded;
            public bool WasConnected;
            public bool CanConnect => WasConnected == true ? ReconnectionTries > 0 : ConnectionTries > 0;

            public bool IsConnected
            {
                get
                {
                    if (Runner == null)
                        return false;

                    if (Request.GameMode == GameMode.Single)
                        return true;

                    if (Runner.IsCloudReady == false || Runner.IsRunning == false)
                        return false;

                    return GameMode == GameMode.Client ? Runner.IsConnectedToServer : true;
                }
            }

            public GamePeer(int id)
            {
                ID = id;
            }
        }
        private class Session
        {
            public bool ConnectionRequested;
            public GamePeer[] GamePeers;

            public bool IsConnected
            {
                get
                {
                    if (GamePeers.SafeCount() == 0)
                        return false;

                    for (int i = 0; i < GamePeers.Length; i++)
                    {
                        if (GamePeers[i].IsConnected == false)
                            return false;
                    }

                    return true;
                }
            }
        }
    }
}
