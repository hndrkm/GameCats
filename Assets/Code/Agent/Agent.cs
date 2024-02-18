using Fusion;
using TMPro;

namespace CatGame
{
    using UnityEngine;
    using UnityEngine.Profiling;

    public class Agent : ContextBehaviour
    {
        public bool IsLocal => Object != null && Object.HasInputAuthority == true;
        public bool IsObserved => Context != null && Context.ObservedAgent == this;

        public AgentInput AgentInput => _agentInput;
        public Character Character => _character;
        public Spells Spells => _spells;
        public Health Health => _health;



        [SerializeField]
        private GameObject _visualRoot;
        [SerializeField]
        private TextMeshPro _textNickname;
        [SerializeField]
        private TextMeshPro _textHealth;

        private AgentInput _agentInput;
        private Character _character;
        private Spells _spells;
        private Health _health;
        private NetworkCulling _networkCulling;

        public override void Spawned()
        {
            name = Object.InputAuthority.ToString();
            
            var earlyAgentController = GetComponent<EarlyAgentController>();
            earlyAgentController.SetDelegates(OnEarlyFixedUpdate, OnEarlyRender);

            var lateAgentController = GetComponent<LateAgentController>();
            lateAgentController.SetDelegates(OnLateFixedUpdate, OnLateRender);
            
            _textNickname.text=Context.NetworkGame.GetPlayer(Object.InputAuthority).Nickname;
            
            _visualRoot.SetActive(true);
            
            _character.OnSpawned(this);
            _spells.OnSpawned();
            _health.OnSpawned(this);
            
        }
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (_spells != null)
            {
                _spells.OnDespawned();
            }
            if (_health != null)
            {
                _health.OnDespawned();
            }
            
            var earlyAgentController = GetComponent<EarlyAgentController>();
            earlyAgentController.SetDelegates(null, null);

            var lateAgentController = GetComponent<LateAgentController>();
            lateAgentController.SetDelegates(null, null);
            
        }
        public override void FixedUpdateNetwork()
        {
            
        }
        public override void Render()
        {

        }
        private void Awake()
        {
            _agentInput = GetComponent<AgentInput>();
            _character = GetComponent<Character>();
            _spells = GetComponent<Spells>();
            _health = GetComponent<Health>();
            _networkCulling = GetComponent<NetworkCulling>();

            _networkCulling.Updated += OnCullingUpdated;
        }
        
        private void OnEarlyFixedUpdate()
        {
            if (_networkCulling.IsCulled == true)
                return;

            Profiler.BeginSample(nameof(Agent));
            
            ProcessFixedInput();

            _spells.OnFixedUpdate();
            _character.OnFixedUpdate();
            
            Profiler.EndSample();
        }
        private void OnLateFixedUpdate()
        {
            if (_networkCulling.IsCulled == true)
                return;

            if (_health.IsAlive == true && Object.IsProxy == false)
            {
                bool attackWasActivated = _agentInput.WasActivated(EGameplayInputAction.Attack);
                bool powerWasActivated = _agentInput.WasActivated(EGameplayInputAction.Power);
                //Debug.Log($"{attackWasActivated} ----- {_agentInput.FixedInput.Attack}");
                TryCast(attackWasActivated, _agentInput.FixedInput.Attack);
                TryPower(powerWasActivated, _agentInput.FixedInput.Power);
            }

            _health.OnFixedUpdate();
            _spells.OnLateFixedUpdate();
            if (Object.IsProxy == false)
            {
                _agentInput.SetLastKnownInput(_agentInput.FixedInput, true);
            }
        }
        private void OnEarlyRender() 
        {
            if (_networkCulling.IsCulled == true)
                return;
            ProcessRenderInput();
            _spells.OnRender();
        }
        private void OnLateRender()
        {
            if(Health != null)
                _textHealth.text = Health.CurrentHealth.ToString();
            if (_networkCulling.IsCulled == true)
                return;
        }
        private void ProcessFixedInput()
        {
            if (Object.IsProxy == true)
                return;

            CharacterMoveController cmc = _character.CharacteController;

            GameplayInput input = default;

            if (_health.IsAlive == true)
            {
                input = _agentInput.FixedInput;
            }
            if (input.Aim == true)
            {
                _spells.Aim();
                input.Aim &= CanAin();
            }
            cmc.MoveCharacter(input.MoveDirection == Vector2.zero ? Vector2.zero : input.MoveDirection);

            _agentInput.SetFixedInput(input, false);
        }
        private void ProcessRenderInput() 
        {
            if (Object.HasInputAuthority == false) 
            {
                return;
            }
            CharacterMoveController cmc = _character.CharacteController;


            GameplayInput input = default;

            if (_health.IsAlive == true)
            {
                var cachedInput = _agentInput.CachedInput;
                input = _agentInput.RenderInput;
                input.Aim = cachedInput.Aim;
                input.AimLocation = cachedInput.AimLocation;

            }
            if (input.Aim == true)
            {
                input.Aim &= CanAin();
            }
            cmc.MoveCharacter(input.MoveDirection == Vector2.zero ? Vector2.zero : input.MoveDirection);

        }
        private void OnCullingUpdated(bool isCulled) 
        {
            bool isActive = isCulled == false;
            //_visualRoot.SetActive(isActive);
            //if(_character.CharacteController.Collider != null)
                //_character.CharacteController.Collider.enabled = isActive;
        }
        
        
        private void TryPower(bool cast,bool hold) 
        {
            if (hold == false)
                return;
            if (_spells.CanCastSpell(cast, 1) == false)
                return;

            if (_spells.Cast(1))
                Debug.Log("CastPower");
        }
        private void TryCast(bool attack,bool hold) 
        {
            if (hold == false)
                return;

            if (_spells.CanCastSpell(attack,0)==false)
                return;


            if (_spells.Cast(0)) 
            {
                Debug.Log("Castarea");
            } 
        }

        private bool CanAin() 
        {
            return _spells.CanAim(0);
        }
        
    }
}
