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
    public abstract class Area : ContextBehaviour, IPredictedSpawnBehaviour
    {
        public PlayerRef PredictedInputAuthority { get; set; }
        public bool IsPredicted => Object == null || Object.IsPredictedSpawn;
        public PlayerRef InputAuthority => IsPredicted == true ? PredictedInputAuthority : Object.InputAuthority; 
        public abstract void AutoCast(Agent owner, Vector2 position, float distance, LayerMask hitMask,EHitType hitType);

        public void PredictedSpawnFailed()
        {
            Despawned(Runner,false);
            Runner.Despawn(Object, true);
        }

        public void PredictedSpawnRender()
        {
            Render();
        }

        public void PredictedSpawnSpawned()
        {
            Spawned();
        }

        public void PredictedSpawnSuccess()
        {
            //throw new NotImplementedException();
        }

        public void PredictedSpawnUpdate()
        {
            FixedUpdateNetwork();
        }
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            PredictedInputAuthority = PlayerRef.None;
        }
    }
}
