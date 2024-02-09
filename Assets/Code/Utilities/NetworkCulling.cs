namespace CatGame
{
    using System;
    using Fusion;
    using UnityEngine;
    public class NetworkCulling : NetworkBehaviour
    {
        public bool IsCulled => _isCulled;
        public Action<bool> Updated;
        [Networked]
        private NetworkBool _keepAlive { get; set; }

        private int _ticRate;
        private bool _isCulled;

        public override sealed void Spawned()
        {
            _ticRate = Runner.Config.Simulation.TickRate;
            _isCulled= false;
        }
        public override sealed void Despawned(NetworkRunner runner, bool hasState)
        {
            _isCulled = false;
        }
        public override sealed void FixedUpdateNetwork()
        {
            if (Runner == null || Runner.IsForward)
                return;
            int simulationTick = Runner.Simulation.Tick;
            if (simulationTick % _ticRate == 0)
            {
                _keepAlive = !_keepAlive;
            }
            bool isCulled = false;
            if (Object.IsProxy == true && Object.LastReceiveTick > 0)
            {
                int lastResiveTickThreshold = simulationTick - _ticRate * 2;
                SimulationSnapshot serverState = Runner.Simulation.LatestServerState;
                if (serverState != null)
                {
                    lastResiveTickThreshold = serverState.Tick - _ticRate * 2;
                }
                if (Object.LastReceiveTick < lastResiveTickThreshold)
                    isCulled = true;
            }
            if (_isCulled != isCulled)
            {
                _isCulled = isCulled;
                if (Updated != null)
                {
                    try 
                    {
                        Updated(isCulled);
                    }
                    catch(Exception ex) 
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }
    }
}
