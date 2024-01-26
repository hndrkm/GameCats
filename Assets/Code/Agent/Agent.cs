using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace CatGame
{
    
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

        private AgentInput _agentInput;
        private Character _character;
        private Spells _spells;
        private Health _health;

        public override void Spawned()
        {
            name = Object.InputAuthority.ToString();    

            _visualRoot.SetActive(true);
            _character.OnSpawned(this);
            _spells.OnSpawned();
            _health.OnSpawned(this);
        }
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (_spells != null)
            {
                
            }
            if (_health != null)
            {
                _health.OnDespawned();
            }
        }
        public override void FixedUpdateNetwork()
        {
            OnEarlyRender();
            OnEarlyFixedUpdate();
            OnLateFixedUpdate();
        }

        private void Awake()
        {
            _agentInput = GetComponent<AgentInput>();
            _character = GetComponent<Character>();
            _spells = GetComponent<Spells>();
            _health = GetComponent<Health>();

            
        }
        private void OnEarlyRender() 
        {
            ProcessRenderInput();
        }
        private void OnLateRender()
        {

        }
        private void OnEarlyFixedUpdate()
        {
            Profiler.BeginSample(nameof(Agent));
            ProcessFixedInput();
            _character.OnFixedUpdate();
            Profiler.EndSample();
        }
        private void OnLateFixedUpdate() 
        {
            
            _health.OnFixedUpdate();

            if (_health.IsAlive == true && Object.IsProxy == false ) 
            {
                bool attackWasActivated = _agentInput.WasActivated(EGameplayInputAction.Attack);
                //Debug.Log($"{attackWasActivated} ----- {_agentInput.FixedInput.Attack}");
                TryCast(attackWasActivated,_agentInput.FixedInput.Attack);
            }

            if (Object.IsProxy == false)
            {
                _agentInput.SetLastKnownInput(_agentInput.FixedInput, true);
            }
        }
        private void ProcessRenderInput() 
        {
            if (Object.HasInputAuthority == false) 
            {
                return;
            }
            CharacterMoveController cmc = _character.CMC;


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
                Debug.Log(_agentInput.RenderInput.AimLocation);
                input.Aim &= CanAin();
            }
            cmc.MoveCharacter(input.MoveDirection == Vector2.zero ? Vector2.zero : input.MoveDirection);

        }
        private void ProcessFixedInput()
        {
            if (Object.IsProxy == true)
                return;

            CharacterMoveController cmc = _character.CMC;

            GameplayInput input = default;

            if (_health.IsAlive == true)
            {
                input = _agentInput.FixedInput;
            }
            if (input.Aim == true)
            {
                input.Aim &= CanAin();
            }
            cmc.MoveCharacter(input.MoveDirection == Vector2.zero ? Vector2.zero : input.MoveDirection);

            _agentInput.SetFixedInput(input, false);
        }
        private void TryCast(bool attack,bool hold) 
        {
            if (hold == false)
                return;
            if (_spells.CanCastSpell(attack,0))
                return;
            
            if (_spells.Cast(0))
                Debug.Log("Casteo"); 
        }
        private bool CanAin() 
        {
            return _spells.CanAim(0);
        }

    }
}
