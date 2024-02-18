using System;
using Fusion;

namespace CatGame
{
    using UnityEngine;

    public struct BodyHitData : INetworkStruct
    {
        public EHitAction Action;
        public float Damage;
        [Networked,Accuracy(0.01f)]
        public Vector2 RelativePosition { get; set; }
        [Networked, Accuracy(0.01f)]
        public Vector2 Direction { get; set; }
        public PlayerRef Instigator;
    }
    public class Health : ContextBehaviour,IHitTarget,IHitInstigator
    {
        public bool IsAlive => CurrentHealth > 0f;
        public float MaxHealth => _maxHealth;

        [Networked, HideInInspector]
        public float CurrentHealth { get; private set; }


        public event Action<HitData> HitTaken;
        public event Action<HitData> HitPerformed;
        [SerializeField]
        private float _maxHealth;
        [SerializeField]
        private Transform _hitIndicatorPivot;

        [Header("Regeneration")]
        [SerializeField]
        private float _healthRegenPerSecond;
        [SerializeField]
        private float _maxHealthFromRegen;
        [SerializeField]
        private int _regenTickPerSecond;
        [SerializeField]
        private int _regenCombatDelay;

        [Networked]
        private int _hitCount { get; set; }
        [Networked, Capacity(8)]
        private NetworkArray<BodyHitData> _hitData { get; }

        private int _visibleHitCount;
        private Agent _agent;

        private TickTimer _regenTickTimer;
        private float _healthRegenPerTick;
        private float _regenTickTime;
        public void OnSpawned(Agent agent)
        {
            _visibleHitCount = _hitCount;
        }
        public void OnDespawned()
        {
            HitTaken = null;
            HitPerformed = null;
        }
        public void OnFixedUpdate()
        {
            if (Object.HasStateAuthority == false)
                return;

            if (IsAlive == true && _healthRegenPerSecond > 0f && _regenTickTimer.ExpiredOrNotRunning(Runner) == true)
            {
                _regenTickTimer = TickTimer.CreateFromSeconds(Runner, _regenTickTime);

                var healthDiff = _maxHealthFromRegen - CurrentHealth;
                if (healthDiff <= 0f)
                    return;

                AddHealth(Mathf.Min(healthDiff, _healthRegenPerTick));
            }
        }
        protected void Awake()
        {
            _agent = GetComponent<Agent>();
        }
        Transform IHitTarget.HitPivot => _hitIndicatorPivot != null ? _hitIndicatorPivot : transform;
        void IHitTarget.ProcessHit(ref HitData hitData)
        {
            if (IsAlive == false)
            {
                hitData.Amount = 0;
                return;
            }

            ApplyHit(ref hitData);

            if (IsAlive == false)
            {
                hitData.IsFatal = true;
                Context.GameplayMode.AgentDeath(_agent, hitData);
            }
        }
        void IHitInstigator.HitPerformed(HitData hitData)
        {
            if (hitData.Amount > 0 && hitData.Target != (IHitTarget)this && Runner.IsResimulation == false)
            {
                HitPerformed?.Invoke(hitData);
            }
        }
        private void ApplyHit(ref HitData hit)
        {
            if (IsAlive == false)
                return;

            if (hit.Action == EHitAction.Damage)
            {
                hit.Amount = ApplyDamage(hit.Amount);
            }
            else if (hit.Action == EHitAction.Heal)
            {
                hit.Amount = AddHealth(hit.Amount);
            }
           

            if (hit.Amount <= 0)
                return;
            if (hit.InstigatorRef == Context.LocalPlayerRef && Runner.IsForward == true)
            {
                HitTaken?.Invoke(hit);
            }

            if (Object.HasStateAuthority == false)
                return;

            _hitCount++;

            var bodyHitData = new BodyHitData
            {
                Action = hit.Action,
                Damage = hit.Amount,
                Direction = hit.Direction,
                RelativePosition = hit.Position != Vector2.zero ? hit.Position - new Vector2(transform.position.x, transform.position.y) : Vector2.zero,
                Instigator = hit.InstigatorRef,
            };

            int hitIndex = _hitCount % _hitData.Length;
            _hitData.Set(hitIndex, bodyHitData);
        }
        private float ApplyDamage(float damage)
        {
            if (damage <= 0f)
                return 0f;

            ResetRegenDelay();
            Debug.Log(damage);
            var healthChange = AddHealth(-(damage ));

            return -(healthChange);
        }
        public void ResetRegenDelay()
        {
            _regenTickTimer = TickTimer.CreateFromSeconds(Runner, _regenCombatDelay);
        }
        public override void CopyBackingFieldsToState(bool firstTime)
        {
            base.CopyBackingFieldsToState(firstTime);

            InvokeWeavedCode();

            CurrentHealth = _maxHealth;
        }


        private float AddHealth(float health)
        {
            float previousHealth = CurrentHealth;
            SetHealth(CurrentHealth + health);
            return CurrentHealth - previousHealth;
        }
        private void SetHealth(float health)
        {
            CurrentHealth = Mathf.Clamp(health, 0, _maxHealth);
        }
        [ContextMenu("sumar Health")]
        private void Debug_AddHealth()
        {
            CurrentHealth += 10;
        }

        [ContextMenu("restar Health")]
        private void Debug_RemoveHealth()
        {
            CurrentHealth -= 10;
        }
    }
}

