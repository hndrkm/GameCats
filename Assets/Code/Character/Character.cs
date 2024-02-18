using Fusion;
namespace CatGame
{
    public class Character : ContextSimulationBehaviour
    {
        public CharacterMoveController CharacteController => _characterMoveController;

        private Agent _agent;
        private CharacterMoveController _characterMoveController;
        private CharacterAnimationController _animationController;

        public void OnSpawned(Agent agent)
        {
            _agent = agent;
        }
        public void OnDespawn() 
        {
            
        }

        public void OnFixedUpdate()
        {
            
        }
        private void Awake()
        {
            _characterMoveController = GetComponent<CharacterMoveController>();
            _animationController = GetComponent<CharacterAnimationController>();
        }
    }
}
