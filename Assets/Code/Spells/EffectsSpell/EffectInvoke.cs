using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CatGame.EffectArea;

namespace CatGame
{
    public class EffectInvoke : Area
    {
        [SerializeField]
        private float _damage;
        [SerializeField]
        private float _EnemyRadio;
        [SerializeField]
        private GameObject _invokeVisual;
        [SerializeField]
        private float _castDespawnTime = 2f;
        [SerializeField]
        private float _impactDespawnTime = 1f;
        private InvokeData _data { get; set; }

        private EHitType _hitType;
        private LayerMask _hitMask;
        private int _ownerObjectInstanceID;
        private List<LagCompensatedHit> _validHits = new List<LagCompensatedHit>(16);

        public override void AutoCast(Agent owner, Vector2 position, float distance, LayerMask hitMask, EHitType hitType)
        {
            if (Runner.IsResimulation == true)
                return;
            InvokeData adata = default;
            adata.CastPosition = position;
            //_data.InitialVelocity
            adata.DespawnCooldown = TickTimer.CreateFromSeconds(Runner, _castDespawnTime);
            adata.StartTick = Runner.Simulation.Tick;
            _hitType = hitType;
            _hitMask = hitMask;

            _ownerObjectInstanceID = owner != null ? owner.gameObject.GetInstanceID() : 0;
            _data = adata;
        }
        public void SetDespawnCooldown(float cooldown)
        {
            var data = _data;
            data.DespawnCooldown = TickTimer.CreateFromSeconds(Runner, cooldown);
            _data = data;
        }
        public override void Spawned()
        {
            base.Spawned();
            _invokeVisual.SetActiveSafe(true);
        }
        public override void FixedUpdateNetwork()
        {

            if (IsProxy == false)
            {
                var data = _data;
                CalculateInkoke(ref data);
                _data = data;
            }
        }
        public override void Render()
        {
            RenderInvoke(_data);
        }
        private void RenderInvoke(InvokeData data) 
        {
            float tick = Runner.Tick;
            transform.position = GetInvokePosition(ref data, tick);
        }
        private void CalculateInkoke(ref InvokeData data) 
        {
            if (data.DespawnCooldown.Expired(Runner) == true)
            {
                Runner.Despawn(Object);
                return;
            }
            if (data.IsFinished == true)
                return;
            var newPosition = GetInvokePosition(ref data, Runner.Tick);
            var previousPosition = GetInvokePosition(ref data, Runner.Tick - 1);
            
            var direction = newPosition - previousPosition;
            float distance = direction.magnitude;

            if (distance <= 0)
                return;
            direction/=distance;
            if (AreaUtility.AreaCast(Runner, InputAuthority, _ownerObjectInstanceID, data.CastPosition, _EnemyRadio, _hitMask, _validHits) == true)
            {
                ProcessHit(ref data, _validHits[0]);
            }
        }
        private void ProcessHit(ref InvokeData data, LagCompensatedHit hit)
        {
            data.InpactPosition = hit.Point;
            float hitDamage = _damage;
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
            //SpawnImpact(ref data);
            data.DespawnCooldown = TickTimer.CreateFromSeconds(Runner, _impactDespawnTime);

        }
        private Vector2 GetInvokePosition(ref InvokeData data, float tick) 
        {
            float time = (tick - data.StartTick) * Runner.DeltaTime;
            if (time <= 0f)
                return data.CastPosition;
            var velX = 3 * (Mathf.Cos(5 * time));
            var velY = 3 * (Mathf.Sin(5 * time));
            return data.CastPosition + new Vector2(velX, velY);  
        }
        public struct InvokeData : INetworkStruct
        {
            public bool IsFinished => HasImpacted;
            public bool HasImpacted { get { return State.IsBitSet(1); } set { State.SetBit(1, value); } }

            public byte State;
            public int StartTick;
            public TickTimer DespawnCooldown;
            public Vector2 CastPosition;
            public Vector2 InitialVelocity;
            public Vector2 FinishedPosition;
            [Networked]
            public Vector2 InpactPosition { get; set; }
        }
    }
}
