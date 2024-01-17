using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    public class Character : ContextSimulationBehaviour
    {
        private Agent _agent;
        public void OnSpawned(Agent agent)
        {
            _agent = agent;
            
        }
    }
}
