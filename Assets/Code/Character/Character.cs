using Fusion;
namespace CatGame
{
    public class Character : ContextSimulationBehaviour
    {
        public CharacterMoveController CharacteController => _characterMoveController;
        private Agent _agent;
        private CharacterMoveController _characterMoveController;

        public void OnSpawned(Agent agent)
        {
            _agent = agent;
        }

        public void OnFixedUpdate()
        {
            
        }
        private void Awake()
        {
            _characterMoveController = GetComponent<CharacterMoveController>();
        }
    }
}
