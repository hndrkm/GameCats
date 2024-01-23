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
        public Health Health => _health;
        [SerializeField]
        private GameObject _visualRoot;

        private AgentInput _agentInput;
        private Character _character;
        private Health _health;

        public override void Spawned()
        {
            name = Object.InputAuthority.ToString();    

            _visualRoot.SetActive(true);
            _character.OnSpawned(this);
            _health.OnSpawned(this);
        }
        public override void Despawned(NetworkRunner runner, bool hasState)
        {

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
                input = _agentInput.RenderInput;
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
            cmc.MoveCharacter(input.MoveDirection == Vector2.zero ? Vector2.zero : input.MoveDirection);

            _agentInput.SetFixedInput(input, false);
        }
    }
}
