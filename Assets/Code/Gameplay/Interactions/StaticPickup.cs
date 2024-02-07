using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    public class StaticPickup : NetworkBehaviour
    {
        [Networked]
        private NetworkBool _consumed { get; set; }
        [Networked]
        private NetworkBool _isDisabled { get; set; }


        public bool TryConsume(Agent agent,out string result) 
        {
            if (Object == null) 
            {
                result = "no esta en la red";
                return false;
            }
            if (_isDisabled == true || _consumed == true)
            {
                result = "item invalido";
                return false;
            }
            result = "";
            return true;
        }
    }
}
