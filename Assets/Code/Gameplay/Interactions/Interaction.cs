using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    public interface  Interaction 
    {
        public string Name { get; }
        public string Description { get; }
        public bool IsActive { get; }

    }
    public interface IPickup : Interaction 
    { 
    }
}
