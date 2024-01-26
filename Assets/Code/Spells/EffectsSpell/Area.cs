using System;
using Fusion;
using UnityEngine;

namespace CatGame
{
    [Serializable]
    public class  AreaDamage
    {
        public float Damage = 10f;
        public float MaxDistance = 10f;

        public float GetDamage(float distance) 
        {
            return Damage;
        }
    }
    public abstract class Area : ContextBehaviour
    {
        public PlayerRef PlayerInputAuthority { get; set; }
        public bool IsPredicted => Object == null;
        public PlayerRef InputAuthority => IsPredicted == true ? PlayerInputAuthority : Object.InputAuthority; 
        public abstract void AutoCast(Agent owner, Vector2 position, float distance, LayerMask hitMask,EHitType hitType);
    }
}
