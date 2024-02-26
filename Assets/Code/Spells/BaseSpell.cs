using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CatGame
{
    public class BaseSpell : Spell
    {
        public bool IsCasting { get { return _state.IsBitSet(0); } set { _state = _state.SetBitNoRef(0, value); } }
        public bool IsReloading { get { return _state.IsBitSet(1); } set { _state = _state.SetBitNoRef(1, value); } }

        [SerializeField]
        private float _reloadTime = 2;
        [SerializeField]
        private int _instanesPerCast = 1;
        [SerializeField]
        private bool _castOnInputDownOnly;
        [SerializeField]
        private int _dispersion = 0;

        [Networked]
        private byte _state { get; set; }
        [Networked]
        private TickTimer _cooldown { get; set; }
        [Networked]
        protected int _instancesCount { get; set; }

        public float GetReloadTime() 
        {
            return _reloadTime - ((int)Owner.Powerups.CharacterStats.ReloadReduction)/10;
        }
        public override void Spawned()
        {
            base.Spawned();
        }
        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            if (Object.IsProxy == true)
            {
                return;
            }
            int? cooldownTargetTick = _cooldown.TargetTick;
            if (cooldownTargetTick.HasValue == true && cooldownTargetTick.Value <= Runner.Simulation.Tick) 
            {
                IsCasting = false;
                IsReloading = false;
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
            if (_castOnInputDownOnly == true && inputDown == false)
                return false;
            return _cooldown.ExpiredOrNotRunning(Runner);
        }
       
        public override bool CanAinm()
        {
            return IsReloading == false;
        }
        public override void Cast(Vector2 castPosition,Vector2 targetPosition, LayerMask hitMask)
        {
            if (CanCast(true) == false)
                return;

            IsCasting = true;
            Vector2 direction = targetPosition - castPosition;
            float distanceToTarget = direction.magnitude;
            direction/=distanceToTarget;

            for (int i = 0; i < _instanesPerCast; i++)
            {
                var spellPosition = castPosition;
                if (_dispersion > 0f)
                {
                    spellPosition = GetCastPosition(_dispersion,i);
                    
                }
                if (CastInstaceSpell(spellPosition, targetPosition,direction,hitMask,i==0) == true)
                {
                    _instancesCount ++;
                }
                
            }
            _cooldown = TickTimer.CreateFromSeconds(Runner, GetReloadTime());
            IsReloading = true;
        }
        protected virtual Vector2 GetCastPosition(int dispersion, int i) 
        {
            return Vector2.one;
        }
        protected virtual bool CastInstaceSpell(Vector2 castPosition, Vector2 targetPosition, Vector2 direction, LayerMask hitMask, bool isFrist) 
        {
            return false;
        }

    }
}
