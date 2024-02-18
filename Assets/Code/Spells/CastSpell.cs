using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    public class CastSpell : Spell
    {
        public bool IsCasting { get { return _state.IsBitSet(0); } set { _state = _state.SetBitNoRef(0, value); } }
        public bool IsReloading { get { return _state.IsBitSet(1); } set { _state = _state.SetBitNoRef(1, value); } }

        [SerializeField]
        private float _reloadTime = 2;
        [SerializeField]
        private int _instanesPerCast = 1;
        [SerializeField]
        private bool _castOnInputDownOnly;

        [Header("Energy")]
        [SerializeField]
        private int _initialEnergy;
        [SerializeField]
        private int _maxEnergy;
        [SerializeField]
        private bool _hasUnlimitedEnergy;

        [Header("Cast")]
        [SerializeField]
        private Transform _castTrasnform;

        [Networked]
        private byte _state { get; set; }

        [Networked]
        private TickTimer _cooldown { get; set; }
        [Networked]
        protected int _instancesCount { get; set; }
        [Networked]
        private int _energy { get; set; }

        private List<LagCompensatedHit> _validHits = new List<LagCompensatedHit>();

        public override void Spawned()
        {
            base.Spawned();
            _energy = Mathf.Clamp(_initialEnergy,0,_maxEnergy);

            
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

            if (_energy <= 0)
                return false;
            if (_castOnInputDownOnly == true && inputDown == false)
                return false;
            return _cooldown.ExpiredOrNotRunning(Runner);
        }
        public override bool CanReload(bool autoReload)
        {
            if (IsCasting == true || IsReloading == true)
                return false;
            if (_energy >= _maxEnergy)
                return false;
            if (_cooldown.ExpiredOrNotRunning(Runner) == false)
                return false;

            return autoReload == false || _energy <= 0;
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
            if (_hasUnlimitedEnergy == false)
            {
                _energy -= _instanesPerCast;
            }
            Vector2 direction = targetPosition - castPosition;
            float distanceToTarget = direction.magnitude;
            direction/=distanceToTarget;

            for (int i = 0; i < _instanesPerCast; i++)
            {
                if (CastInstaceSpell(castPosition,targetPosition,direction,hitMask,i==0) == true)
                {
                    _instancesCount ++;
                }
                
            }
            _cooldown = TickTimer.CreateFromSeconds(Runner, _reloadTime);
        }
        public override void Reload()
        {
            if (CanReload(false) == false)
                return;
            IsReloading = true;
            _cooldown = TickTimer.CreateFromSeconds(Runner, _reloadTime);
        }
        public override bool HasEnergy()
        {
            return _energy > 0;
        }
        public override bool AddEnergy(int energy)
        {
            if(_energy >= _maxEnergy)
                return false;
            _energy = Mathf.Clamp(_energy + energy,0,_maxEnergy);
            return true;
        }
        protected virtual bool CastInstaceSpell(Vector2 castPosition, Vector2 targetPosition, Vector2 direction, LayerMask hitMask, bool isFrist) 
        {
            return false;
        }

    }
}
