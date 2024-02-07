using Fusion;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace CatGame
{
    public class InvokeSpell : Spell
    {
        public bool IsCasting { get { return _state.IsBitSet(0); } set { _state = _state.SetBitNoRef(0, value); } }
        public bool IsReloading { get { return _state.IsBitSet(1); } set { _state = _state.SetBitNoRef(1, value); } }
        public float ReloadTime => _reloadTime;
        public float Cooldown => _cooldown.ExpiredOrNotRunning(Runner) == false ? _cooldown.RemainingTime(Runner).Value : 0;


        [SerializeField]
        private EffectInvoke _invokeSpell;
        [SerializeField]
        private int _invokesPerCast = 3;
        [SerializeField]
        private float _areaDespawnTime = 0.5f;
        [SerializeField]
        private int _reloadTime = 2;
        [Networked]
        private byte _state { get; set; }
        [Networked]
        private TickTimer _cooldown { get; set; }

        private int _castTicks;

        public override void Spawned()
        {
            base.Spawned();
            _castTicks = 1;
        }
        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            if (Object.IsProxy == true)
            {
                //if(Object.LastReceiveTick < Runner.Tick - 2.0f * Runner.TickRate)
                return;
            }
            int? cooldownTargetTick = _cooldown.TargetTick;
            if (cooldownTargetTick.HasValue == true && cooldownTargetTick.Value <= Runner.Tick)
            {
                IsCasting = false;
            }

        }
        public override bool IsBusy()
        {
            return IsCasting || IsReloading;
        }
        public override bool CanCast(bool inputDown)
        {
            if (IsCasting == true || IsReloading == true)
                return false;
            return _cooldown.ExpiredOrNotRunning(Runner);
        }
        public override bool CanReload(bool autoReload)
        {
            if (IsCasting == true || IsReloading == true)
                return false;
            if (_cooldown.ExpiredOrNotRunning(Runner) == false)
                return false;
            return autoReload;
        }
        public override bool CanAinm()
        {
            return IsReloading;
        }
        public override void Reload()
        {
            if (CanReload(false) == false)
                return;
            IsReloading = true;
            _cooldown = TickTimer.CreateFromSeconds(Runner, _reloadTime);

        }

        public override void Cast(Vector2 targetPosition, LayerMask hitMask)
        {
            if (CanCast(true) == false)
                return;
            IsCasting = true;

            for (int i = 0; i < _invokesPerCast; i++)
            {
                CastInvoke(targetPosition + Vector2.right + new Vector2(0f,-i) , 2, hitMask);
            }
            _cooldown = TickTimer.CreateFromSeconds(Runner, _castTicks);
        }
        public bool CastInvoke(Vector2 areaPosition, float areaEffect, LayerMask hitMask)
        {
            if (Object.IsProxy == true)
                return false;

            var area = Runner.Spawn(_invokeSpell, areaPosition , Quaternion.identity, Object.InputAuthority, BeforeAreaSpawned);
            if (area == null)
                return true;
            area.AutoCast(Owner, areaPosition, areaEffect, hitMask, HitType);
            float castTime = (Runner.Tick) * Runner.DeltaTime;
            area.SetDespawnCooldown(_areaDespawnTime);

            void BeforeAreaSpawned(NetworkRunner runner, NetworkObject spawnedObject)
            {
                if (HasStateAuthority == true)
                    return;

            }
            return true;
        }
    }
}
