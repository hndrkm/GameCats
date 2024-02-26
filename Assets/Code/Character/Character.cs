using Fusion;
using System;

namespace CatGame
{

    public class Character : ContextSimulationBehaviour
    {
        
        public CharacterMoveController CharacteController => _characterMoveController;
        public CharacterMoveController AnimationController => _characterMoveController;

        private Agent _agent;
        private CharacterMoveController _characterMoveController;
        private CharacterAnimationController _animationController;
        private float _baseSpeed = 2f;
        public void OnSpawned(Agent agent)
        {
            _agent = agent;
            _baseSpeed = _characterMoveController.MaxSpeed;
        }
        public void OnDespawn() 
        {
            
        }
       
        public void OnFixedUpdate()
        {
            _characterMoveController.MaxSpeed = _baseSpeed + _agent.Powerups.CharacterStats.ExtraSpeed;
        }
        private void Awake()
        {
            _characterMoveController = GetComponent<CharacterMoveController>();
            _animationController = GetComponent<CharacterAnimationController>();
        }
    }
}
