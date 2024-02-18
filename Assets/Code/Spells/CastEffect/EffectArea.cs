using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CatGame
{
    public class EffectArea : Area
    {
        public float CastDespawnTime => _castDespawnTime;


        [SerializeField]
        private AreaDamage _damage;
        [SerializeField]
        private float _castDespawnTime = 2f;
        [SerializeField]
        private float _impactDespawnTime = 1f;
        
        [SerializeField]
        private GameObject _impactEffect;

        [SerializeField]
        private GameObject _areaVisual;

        [Networked]
        private AreaData _data_Networked { get; set; }
        private AreaData _data_Local;
        private AreaData _data { get { return IsPredicted ? _data_Local : _data_Networked; } set { if (IsPredicted == true) _data_Local = value; else _data_Networked = value; } }
        
        private EHitType _hitType;
        private LayerMask _hitMask;
        private int _ownerObjectInstanceID;
        private List<LagCompensatedHit> _validHits =  new List<LagCompensatedHit>(16);
        private bool _hasInpactedVisual;


        public override void AutoCast(Agent owner, Vector2 position, float radius, LayerMask hitMask, EHitType hitType)
        {
            if (IsPredicted == true && Runner.IsResimulation == true)
                return;
            AreaData adata = default;

            adata.CastPosition = position;
            adata.DespawnCooldown = TickTimer.CreateFromSeconds(Runner,_castDespawnTime);
            adata.StartTick = Runner.Simulation.Tick;
            adata.Radius = radius;

            _hitType = hitType;
            _hitMask = hitMask;
            
            _ownerObjectInstanceID = owner != null? owner.gameObject.GetInstanceID():0;
            _data = adata;
        }
        public void SetDespawnCooldown(float cooldown) 
        {
            var data = _data;
            data.DespawnCooldown = TickTimer.CreateFromSeconds(Runner,cooldown);
            _data = data;  
        }

        public override void Spawned()
        {
            base.Spawned();
            _areaVisual.SetActiveSafe(true);
            _hasInpactedVisual = false;
        }
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
        }
        public override void FixedUpdateNetwork()
        {
            bool isProxy = IsPredicted == false && IsProxy == true;
            if (isProxy == false)
            {
                var data = _data;
                CalculateArea(ref data);
                _data = data;
            }
        }
        public override void Render()
        {
            RenderArea(_data);
        }
        private void RenderArea(AreaData data) 
        {
            if (data.HasImpacted == true && _hasInpactedVisual == false)
            {
                SpawnImpact(ref data);
                _areaVisual.SetActiveSafe(false);
            }
            bool isProxy = IsPredicted == false && IsProxy == true;
            if (data.IsFinished == false)
            {
                var simulation = Runner.Simulation;
                float floatTick = simulation.Tick + simulation.StateAlpha;
                if (isProxy == true)
                {
                    floatTick = simulation.InterpFrom.Tick + (simulation.InterpTo.Tick - simulation.InterpFrom.Tick) * simulation.InterpAlpha;
                }
                transform.position = GetAreaPosition(ref data, floatTick);
            }
            else 
            {
                transform.position = data.FinishedPosition;
            }
        }

        private void CalculateArea(ref AreaData data) 
        {
            if (data.DespawnCooldown.Expired(Runner) == true) 
            {
                Debug.Log(data.CastPosition);
                if (data.HasStopped == false)
                {
                    data.FinishedPosition = GetAreaPosition(ref data, Runner.Simulation.Tick);
                    data.HasStopped = true;
                }
                if (data.HasImpacted == false)
                    SpawnImpact(ref data);

                Runner.Despawn(Object,true);
                return;
            }
            if (data.IsFinished == true)
                return;
            var newPosition = GetAreaPosition(ref data, Runner.Simulation.Tick);

            Debug.DrawLine(data.CastPosition, data.CastPosition + new Vector2(data.Radius, 0));
            if (AreaUtility.AreaCast(Runner, InputAuthority, _ownerObjectInstanceID, newPosition, data.Radius, _hitMask, _validHits) == true)
            {
                ProcessHit(ref data, _validHits[0]);
            }
        }
        private void ProcessHit(ref AreaData data,LagCompensatedHit hit) 
        {
            data.FinishedPosition = data.CastPosition;
            data.InpactPosition = hit.Point;

            float hitDamage = _damage.GetDamage(0);
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
            SpawnImpact(ref data);
            data.HasStopped = true;
            data.DespawnCooldown = TickTimer.CreateFromSeconds(Runner,_impactDespawnTime);

        }
        private void SpawnImpact(ref AreaData data)
        {

            data.HasImpacted = true;
            _hasInpactedVisual = true;
        }
        private Vector2 GetAreaPosition(ref AreaData data, float tick) 
        {
            return data.CastPosition;
        }




        public struct AreaData : INetworkStruct
        {
            public bool IsFinished => HasImpacted || HasStopped;
            public bool HasStopped { get { return State.IsBitSet(0); } set { State.SetBit(0, value); } }
            public bool HasImpacted { get { return State.IsBitSet(1); }  set { State.SetBit(1,value); } }

            public byte State;
            public int StartTick;
            public TickTimer DespawnCooldown;
            public Vector2 CastPosition;
            public float Radius;

            public Vector2 FinishedPosition;

            [Networked,Accuracy(0.01f)]
            public Vector2 InpactPosition { get; set; }

        }
    }
}
