using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    
    public class Agent : ContextBehaviour
    {
        public bool IsLocal => Object != null && Object.HasInputAuthority == true;
        public bool IsObserved => Context != null && Context.ObservedAgent == this;

        public Character Character => _character;
        public Health Health => _health;
        [SerializeField]
        private GameObject _visualRoot;

        private Character _character;
        private Health _health;

        public override void Spawned()
        {
            name = Object.InputAuthority.ToString();    

            

            _visualRoot.SetActive(true);

            _character.OnSpawned(this);
            _health.OnSpawned(this);

            

            void GenerateRandomInput(NetworkRunner runner, NetworkInput networkInput)
            {
                // Used for batch testing

                GameplayInput gameplayInput = new GameplayInput();
                gameplayInput.MoveDirection = new Vector2(UnityEngine.Random.value * 2.0f - 1.0f, UnityEngine.Random.value > 0.25f ? 1.0f : -1.0f).normalized;
                gameplayInput.LookRotationDelta = new Vector2(UnityEngine.Random.value * 2.0f - 1.0f, UnityEngine.Random.value * 2.0f - 1.0f);
                gameplayInput.Attack = UnityEngine.Random.value > 0.99f;
                gameplayInput.Reload = UnityEngine.Random.value > 0.99f;
                gameplayInput.Interact = UnityEngine.Random.value > 0.99f;
                gameplayInput.Weapon = (byte)(UnityEngine.Random.value > 0.99f ? (UnityEngine.Random.value > 0.25f ? 2 : 1) : 0);

                networkInput.Set(gameplayInput);
            }


        }
        public override void Despawned(NetworkRunner runner, bool hasState)
        {

            if (_health != null)
            {
                _health.OnDespawned();
            }
        }
        private void Awake()
        {
            
            _character = GetComponent<Character>();
            _health = GetComponent<Health>();

            
        }
    }
}
