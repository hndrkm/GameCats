using Fusion;

namespace CatGame
{
    public interface IContextBehaviour 
    {
        BaseContext Context { get; set; }
    }
    public class ContextBehaviour : NetworkBehaviour, IContextBehaviour
    {
        public BaseContext Context { get; set;}
    }
    public abstract class ContextSimulationBehaviour: SimulationBehaviour, IContextBehaviour 
    {
        public BaseContext Context { get; set; }
    }
}
