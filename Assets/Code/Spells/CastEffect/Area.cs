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

        void IPredictedSpawnBehaviour.PredictedSpawnSpawned()
        {
            Spawned();
        }
        void IPredictedSpawnBehaviour.PredictedSpawnUpdate()
        {
            FixedUpdateNetwork();
        }
        void IPredictedSpawnBehaviour.PredictedSpawnRender()
        {
            Render();
        }
        void IPredictedSpawnBehaviour.PredictedSpawnFailed()
        {
            Despawned(Runner,false);
            Runner.Despawn(Object, true);
        }
        public void PredictedSpawnSuccess()
        {
            //throw new NotImplementedException();
        }

        
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            PredictedInputAuthority = PlayerRef.None;
        }
    }
}
