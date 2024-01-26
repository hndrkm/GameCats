using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    public class EffectArea : Area
    {
        [SerializeField]
        private AreaDamage _damage;
        [SerializeField]
        private float _castDespawnTime =2f;
        [SerializeField]
        private float _impactDespawnTime = 1f;

        [SerializeField]
        private GameObject _areaVisual;

        private AreaData _aData { get; set; }
        
        private EHitType _hitType;
        private LayerMask _hitMask;
        private int _ownerObjectInstanceID;
        private List<LagCompensatedHit> _validHits =  new List<LagCompensatedHit>(16);


        public override void AutoCast(Agent owner, Vector2 position, float distance, LayerMask hitMask, EHitType hitType)
        {
            if (Runner.IsResimulation == true)
                return;
            AreaData adata = default;
            adata.CastPosition = position;
            adata.DespawnCooldown = TickTimer.CreateFromSeconds(Runner,_castDespawnTime);
            adata.StartTick = Runner.Tick;
            _hitType = hitType;
            _hitMask = hitMask;
            
            _ownerObjectInstanceID = owner != null? owner.gameObject.GetInstanceID():0;
            _aData = adata;
        }
        public void SetDespawnCooldown(float cooldown) 
        {
            var data = _aData;
            data.DespawnCooldown = TickTimer.CreateFromSeconds(Runner,cooldown);
            _aData = data;  
        }

        public override void Spawned()
        {
            base.Spawned();
            _areaVisual.SetActiveSafe(true);
        }
        public override void FixedUpdateNetwork()
        {
            if (IsProxy == true)
            {
                var data = _aData;
                CalculateArea(ref data);
                _aData = data;
            }
        }


        private void CalculateArea(ref AreaData data) 
        {
            if (data.DespawnCooldown.Expired(Runner) == true) 
            {
                Runner.Despawn(Object);
                return;
            }
            if (data.IsFinished == true)
                return;
            if (AreaUtility.AreaCast(Runner, InputAuthority, _ownerObjectInstanceID, data.CastPosition, _damage.MaxDistance, _hitMask, _validHits) == true)
                ProcessHit(ref data, _validHits[0]);
        
        }
        private void ProcessHit(ref AreaData data,LagCompensatedHit hit) 
        {
            data.InpactPosition = hit.Point;
            float hitDamage = _damage.GetDamage(0);
            if (hitDamage > 0) 
            {
                var player = Context.NetworkGame.GetPlayer(InputAuthority);
                var owner = player != null ? player.ActiveAgent : null;
                if (owner != null)
                {
                    HitUtility.ProcessHit(owner, hit.Point, hit, hitDamage, _hitType, out HitData hitData);
                }
                else 
                {
                    HitUtility.ProcessHit(InputAuthority, hit.Point, hit, hitDamage, _hitType, out HitData hitData);
                }
            }
            SpawnImpact(ref data);
            data.DespawnCooldown = TickTimer.CreateFromSeconds(Runner,_impactDespawnTime);

        }
        private void SpawnImpact(ref AreaData data) 
        {
            data.HasImpacted = true;
        }

        public struct AreaData : INetworkStruct
        {
            public bool IsFinished => HasImpacted;
            public bool HasImpacted { get { return State.IsBitSet(1); }  set { State.SetBit(1,value); } }
            public byte State;
            public int StartTick;
            public TickTimer DespawnCooldown;
            public Vector2 CastPosition;
            [Networked]
            public Vector2 InpactPosition { get; set; }

        }
    }
}
