using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using static UnityEditor.Timeline.TimelinePlaybackControls;

namespace CatGame
{
    [System.Serializable]
    public class BaseContext
    {

        [HideInInspector]
        public string PeerUserID;
        [HideInInspector]
        public PlayerData PlayerData;

        public MenuUI UI;

        public NetworkGame NetworkGame;

        [HideInInspector]
        public GlobalSettings Settings;
        [HideInInspector]
        public RuntimeSettings RuntimeSettings;


        [HideInInspector]
        public bool IsVisible;
        [HideInInspector]
        public bool HasInput;

        [HideInInspector]
        public NetworkRunner Runner;
        [HideInInspector]
        public GameplayMode GameplayMode;
        [HideInInspector]
        public PlayerRef LocalPlayerRef;
        [HideInInspector]
        public PlayerRef ObservedPlayerRef;
        [HideInInspector]
        public Agent ObservedAgent;
        [HideInInspector]
        public Transform WaitingAgentTransform;

        public Matchmaking Matchmaking;
    }
    public class Base : MonoBehaviour
    {
        public bool ContextReady { get; private set; }
        public bool IsActive { get; private set; }
        public BaseContext Context => _context;

        [SerializeField]
        private bool _selfInitialize;
        [SerializeField]
        private BaseContext _context;

        private bool _isInitialized;
        private List<BaseService> _services = new List<BaseService>();

        public void PrepareContext()
        {
            
            OnPrepareContext(_context);
        }

        public void Initialize()
        {
            if (_isInitialized == true)
                return;

            if (ContextReady == false)
            {
                OnPrepareContext(_context);
            }

            OnInitialize();

            _isInitialized = true;
        }

        public IEnumerator Activate()
        {
            if (_isInitialized == false)
                yield break;

            yield return OnActivate();

            IsActive = true;
        }

        public void Deactivate()
        {
            if (IsActive == false)
                return;

            OnDeactivate();

            IsActive = false;
        }

        public void Deinitialize()
        {
            if (_isInitialized == false)
                return;

            Deactivate();

            OnDeinitialize();

            ContextReady = false;
            _isInitialized = false;
        }

        public T GetService<T>() where T : BaseService
        {
            for (int i = 0, count = _services.Count; i < count; i++)
            {
                if (_services[i] is T service)
                    return service;
            }

            return null;
        }

        public void Quit()
        {
            Deinitialize();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
        }
        protected void Awake()
        {
            if (_selfInitialize == true)
            {
                Initialize();
            }
        }

        protected IEnumerator Start()
        {
            if (_isInitialized == false)
                yield break;

            if (_selfInitialize == true && IsActive == false)
            {
                AddService(_context.UI);

                yield return Activate();
            }
        }

        protected virtual void Update()
        {
            if (IsActive == false)
                return;

            OnTick();
        }

        protected virtual void LateUpdate()
        {
            if (IsActive == false)
                return;

            OnLateTick();
        }

        protected void OnDestroy()
        {
            Deinitialize();
        }

        protected void OnApplicationQuit()
        {
            Deinitialize();
        }

        protected virtual void OnPrepareContext(BaseContext context)
        {
            context.PlayerData = Global.PlayerService.PlayerData;
            context.Settings = Global.Settings;
            context.RuntimeSettings = Global.RuntimeSettings;

            context.HasInput = true;
            context.IsVisible = true;

            ContextReady = true;
        }

        protected virtual void OnInitialize()
        {
            CollectServices();
        }

        protected virtual IEnumerator OnActivate()
        {
            for (int i = 0; i < _services.Count; i++)
            {
                _services[i].Activate();
            }

            yield break;
        }

        protected virtual void OnTick()
        {
            for (int i = 0, count = _services.Count; i < count; i++)
            {
                _services[i].Tick();
            }
        }

        protected virtual void OnLateTick()
        {
            for (int i = 0, count = _services.Count; i < count; i++)
            {
                _services[i].LateTick();
            }
        }

        protected virtual void OnDeactivate()
        {
            for (int i = 0; i < _services.Count; i++)
            {
                _services[i].Deactivate();
            }
        }

        protected virtual void OnDeinitialize()
        {
            for (int i = 0; i < _services.Count; i++)
            {
                _services[i].Deinitialize();
            }

            _services.Clear();
        }

        protected virtual void CollectServices()
        {
            var services = GetComponentsInChildren<BaseService>(true);

            foreach (var service in services)
            {
                Debug.Log(service.ToString());
                AddService(service);
            }
        }

        protected void AddService(BaseService service)
        {
            if (service == null)
            {
                Debug.LogError($"Ervicio perdido");
                return;
            }

            if (_services.Contains(service) == true)
            {
                Debug.LogError($"Service {service.gameObject.name} ya montado");
                return;
            }
            
            service.Initialize(this, Context);

            _services.Add(service);
        }

    }
}
