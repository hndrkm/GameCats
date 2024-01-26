using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    public class AreaSpell : Spell
    {
        public bool IsCasting { get { return _state.IsBitSet(0); } set { _state = _state.SetBitNoRef(0, value); } }
        public bool IsReloading { get { return _state.IsBitSet(1); } set { _state = _state.SetBitNoRef(1, value); } }
        public float ReloadTime => _reloadTime;
        public float Cooldown => _cooldown.ExpiredOrNotRunning(Runner) == false ? _cooldown.RemainingTime(Runner).Value : 0;

        [SerializeField]
        private GameObject _areaSpell;
        [SerializeField]
        private float _areaEffect = 2f;
        [SerializeField]
        private float _areaDespawnTime = 0.5f;

        [SerializeField]
        private int _reloadTime=2;
        [SerializeField]
        private bool _castOnKeyDownOnly;
        [SerializeField]
        private int _areasPerCast;
        [SerializeField]
        private int _initialEnergy;


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

        }

        public override bool IsBusy()
        {
            return IsCasting || IsReloading;
        }
        public override bool CanCast(bool inputDown)
        {
            if(IsCasting == true || IsReloading == true)
                return false;
            return _cooldown.ExpiredOrNotRunning(Runner);
        }
        public override bool CanReload(bool autoReload)
        {
            if (IsCasting == true || IsReloading == true)
                return false;
            if (_cooldown.ExpiredOrNotRunning(Runner)==false)
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
            _cooldown = TickTimer.CreateFromSeconds(Runner,_reloadTime);

        }
        public override void Cast(Vector2 targetPosition, LayerMask hitMask)
        {
            if(CanCast(true) == false)
                return;
            IsCasting = true;

            for (int i = 0; i < _areasPerCast; i++)
            {
                CastArea(targetPosition, 2, hitMask); 
            }
            _cooldown = TickTimer.CreateFromSeconds(Runner, _castTicks);
        }

        public bool CastArea(Vector2 areaPosition, float areaEffect, LayerMask hitMask) 
        {
            if(Object.IsProxy == true)
                return false;

            var area = Runner.Spawn(_areaSpell, areaPosition,Quaternion.identity,Object.InputAuthority,BeforeAreaSpawned);
            if (area == null)
                return true;

            void BeforeAreaSpawned(NetworkRunner runner,NetworkObject spawnedObject) 
            {
                if (HasStateAuthority == true)
                    return;

            }
            return true;
        }
    }
}
