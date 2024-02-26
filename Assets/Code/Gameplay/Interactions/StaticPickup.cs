using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    public class StaticPickup : NetworkBehaviour, IPickup
    {
        public bool Consumed => _consumed;
        public bool IsDisabled => _isDisabled;
        public bool AutoDespawn => _despawnDelay > 0f;
        public Action<StaticPickup> PickupConsumed;
        
        [SerializeField]
        private GameObject _visuals;
        [SerializeField]
        private float _despawnDelay = 2f;
        [SerializeField]
        private string _interactionName;
        [SerializeField]
        private string _interactionDescription;
        [SerializeField]
        private EBehaviur _startBehaviur;


        [Networked]
        private EBehaviur _behaviur { get; set; }
        [Networked(OnChanged = nameof(OnConsumeChanged),OnChangedTargets = OnChangedTargets.All)]
        private NetworkBool _consumed { get; set; }
        [Networked]
        private NetworkBool _isDisabled { get; set; }
        private TickTimer _despawnCooldown;
        private Collider _collider;
        public string Name => InteractionName;

        public string Description => InteractionDescription;

        public bool IsActive => IsDisabled;

        protected virtual string InteractionName => _interactionName;
        protected virtual string InteractionDescription => _interactionDescription;

        public void Refresh()
        {
            if (Object == null || Object.HasStateAuthority == false)
                return;
            _consumed = false;
            SetIsDisable(false);
        }
        public void SetIsDisable(bool value) 
        {
            if (Object == null || Object.HasStateAuthority == false)
                return;
            _isDisabled = value;
        }
        public bool TryConsume(Agent agent,out string result) 
        {
            if (Object == null) 
            {
                result = "no esta en la red";
                return false;
            }
            if (_isDisabled == true || _consumed == true)
            {
                result = "item invalido";
                return false;
            }
            if (Consume(agent,out result) ==  false)
                return false;

            _consumed = true;
            
            if (_despawnDelay > 0)
                _despawnCooldown = TickTimer.CreateFromSeconds(Runner, _despawnDelay);

            PickupConsumed?.Invoke(this);
            return true;
        }
        public void SetBehaviour(EBehaviur behaviur, float despawnDelay)
        {
            if (Object == null || Object.HasStateAuthority == false)
                return;
            _behaviur= behaviur;
            _despawnDelay = despawnDelay;
        }

        protected virtual bool Consume(Agent agent, out string result) 
        { 
            result = string.Empty; 
            return false;
        }

        public virtual void OnConsumed() 
        {
            if (Runner.Stage == SimulationStages.Resimulate)
                return;
            _visuals.SetActive(false);

        }
        public override void Spawned()
        {
            if (Object.HasStateAuthority == true)
            {
                _behaviur = _startBehaviur;    
            }
            UpdateState();
        }
        public override void FixedUpdateNetwork()
        {
            if (Object.HasStateAuthority == false)
                return;
            if (_consumed == true && _despawnCooldown.Expired(Runner) == true)
                Runner.Despawn(Object);
        }
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            PickupConsumed = null;
            _despawnCooldown = default;
        }

        protected void Awake()
        {
            _collider = GetComponentInChildren<Collider>();
        }
        protected void OnTriggerEnter(Collider other)
        {
            if (Object == null || Object.HasStateAuthority == false)
                return;
            if (_consumed == true)
                return;

            var agent = other.GetComponentInParent<Agent>();
            if (agent == null)
                return;
            TryConsume(agent,out string result);
        }
        private void UpdateState() 
        {
            _collider.enabled = _consumed == false;
            _visuals.SetActive(_consumed == false);
            _collider.isTrigger = _behaviur == EBehaviur.Trigger;
            //_collider.gameObject.layer = _behaviur == EBehaviur.Trigger ? 1 : 0;
        }
        private static void OnConsumeChanged(Changed<StaticPickup> changed) 
        {
            if (changed.Behaviour._consumed ==  true)
            {
                changed.Behaviour.OnConsumed();
            }
            changed.Behaviour.UpdateState();
        }
        private static void OnBehaviourChanged(Changed<StaticPickup> changed) 
        {
            changed.Behaviour.UpdateState();
        }
        public enum EBehaviur 
        {
            None,
            Trigger,
            Interaction,
        }
    }
}
