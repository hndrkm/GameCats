using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    public class InvokeArea : Area
    {
        public float CastDespawnTime => _castDespawnTime;
        [SerializeField]
        private float _damage;
        [SerializeField]
        private float _detectionRadius;
        [SerializeField]
        private GameObject _invokeVisual;
        [SerializeField]
        private float _castDespawnTime = 2f;

        [SerializeField]
        private float _impactDespawnTime = 1f;

        [Networked]
        private InvokeData _data_Networked { get; set; }
        private InvokeData _data_Local;
        private InvokeData _data { get { return IsPredicted ? _data_Local : _data_Networked; } set { if (IsPredicted == true) _data_Local = value; else _data_Networked = value; } }

        private EHitType _hitType;
        private LayerMask _hitMask;
        private int _ownerObjectInstanceID;
        private Agent _owner;
        private List<LagCompensatedHit> _validHits = new List<LagCompensatedHit>(16);
        private bool _hasImpactedVisual;

        public override void AutoCast(Agent owner, Vector2 position, float speed, LayerMask hitMask, EHitType hitType)
        {
            if (Runner.IsResimulation == true)
                return;
            InvokeData data = default;
            _owner = owner;
            data.CastPosition = position;
            data.InitialVelocity = Vector2.right * speed;
            data.DespawnCooldown = TickTimer.CreateFromSeconds(Runner, _castDespawnTime);
            data.StartTick = Runner.Simulation.Tick;
            var posOwner2d = new Vector2(_owner.transform.position.x, _owner.transform.position.y);
            data.Radius = (posOwner2d - data.CastPosition).magnitude;
            _hitType = hitType;
            _hitMask = hitMask;
            
            _ownerObjectInstanceID = owner != null ? owner.gameObject.GetInstanceID() : 0;
            _data = data;
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
            if (data.HasImpacted == true && _hasImpactedVisual == false)
            {
                SpawnImpact(ref data, data.InpactPosition);
                _invokeVisual.SetActive(false);
            }
            bool isProxi = IsPredicted == false && IsProxy == true;
            if (data.IsFinished == false)
            {
                var simulation = Runner.Simulation;
                float floatTick = simulation.Tick + simulation.StateAlpha;
                if (isProxi == true)
                {
                    floatTick = simulation.InterpFrom.Tick + (simulation.InterpTo.Tick - simulation.InterpFrom.Tick) * simulation.InterpAlpha;
                }
                transform.position = GetInvokePosition(ref data, floatTick);
            }
            else 
            {
                transform.position = data.FinishedPosition;
            }
        }
        private void CalculateInkoke(ref InvokeData data) 
        {
            if (data.DespawnCooldown.Expired(Runner) == true)
            {
                if (data.HasStopped == false)
                {
                    data.FinishedPosition = GetInvokePosition(ref data, Runner.Simulation.Tick);
                    data.HasStopped = true;
                }
                if (data.HasImpacted == false)
                {
                    SpawnImpact(ref data, data.FinishedPosition);
                }
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
            if (AreaUtility.AreaCast(Runner, InputAuthority, _ownerObjectInstanceID, data.CastPosition, _detectionRadius, _hitMask, _validHits) == true)
            {
                ProcessHit(ref data, _validHits[0]);
            }
        }
        private void ProcessHit(ref InvokeData data, LagCompensatedHit hit)
        {
            data.FinishedPosition = hit.Point;
            float hitDamage = _damage;
            if (hitDamage > 0f)
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
            SpawnImpact(ref data,hit.Point);
            data.HasStopped = true;
            data.DespawnCooldown = TickTimer.CreateFromSeconds(Runner, _impactDespawnTime);

        }
        private void SpawnImpact(ref InvokeData data, Vector2 position) 
        {
            if (position == Vector2.zero)
                return;

            data.InpactPosition = position;
            data.HasImpacted = true;
            
            _hasImpactedVisual = true;
        }
        private Vector2 GetInvokePosition(ref InvokeData data, float tick) 
        {
            float time = (tick - data.StartTick) * Runner.DeltaTime;
            if (time <= 0f)
                return data.CastPosition;
            var posOwner2d = new Vector2(_owner.transform.position.x, _owner.transform.position.y);
            var velX = Mathf.Cos(data.InitialVelocity.magnitude * time) * data.Radius;
            var velY = Mathf.Sin(data.InitialVelocity.magnitude * time) * data.Radius;
            return (posOwner2d + new Vector2(velX,velY));
        }
        public struct InvokeData : INetworkStruct
        {
            public bool IsFinished => HasImpacted;
            public bool HasStopped { get { return State.IsBitSet(0); } set { State.SetBit(0, value); } }
            public bool HasImpacted { get { return State.IsBitSet(1); } set { State.SetBit(1, value); } }

            public byte State;
            public int StartTick;
            public TickTimer DespawnCooldown;
            public Vector2 CastPosition;
            public Vector2 InitialVelocity;
            public Vector2 FinishedPosition;
            public float Radius;
            [Networked]
            public Vector2 InpactPosition { get; set; }
        }
    }
}
