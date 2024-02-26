using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{

    public class RandomPickup : StaticPickup
    {

        protected override bool Consume(Agent agent, out string result)
        {
            result = "Poder activo";
            if(agent.Powerups.IsActive == true)
                return false;

            Debug.Log("Consumido");
            agent.Powerups.RandomActivate();
            result = string.Empty;
            return true;
        }
    }
}
