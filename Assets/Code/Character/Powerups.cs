using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace CatGame
{
    [Serializable]
    public struct CharacterStats : INetworkStruct
    {
        //Health
        public short HealthRegen;
        public short ExtraHealth;
        //Spells
        public short ExrtaDamage;
        public short ExtraScope;
        public short ReloadReduction;
        //Controller
        public short ExtraSpeed;
    }
    public enum EBuffType
    {
        None,
        MaxHealth,
        RegerenHeal,
        ExtraDamage,
        ExtraScope,
        ReloadReduction,
        ExtraSpeed,
    }
    public class Powerups : ContextBehaviour
    {
        public bool IsActive => _state;
        public float RemainingTime => _cooldown.IsRunning == true ? _cooldown.RemainingTime(Runner).Value : 0f;
        public float Energy => _energy;

        [Networked]
        public CharacterStats CharacterStats { get; private set; }
        [SerializeField]
        public CharacterStats ConfigStats;
        private CharacterStats _baseStats;
        [SerializeField]
        private float _reloadTime = 10;
        [SerializeField]
        private float _initialEnergy = 100f;
        [SerializeField]
        private float _maxEnergy = 100f;
        [SerializeField]
        private float _energyPerHit = 10f;
        [SerializeField]
        private float _consume = 1f;
        
        [SerializeField]
        private Transform _visualsRoot;

        [Networked(OnChanged = nameof(OnStateChanged),OnChangedTargets = OnChangedTargets.All)]
        private NetworkBool _state { get; set; }
        [Networked]
        private float _energy { get; set; }
        [Networked]
        private TickTimer _cooldown { get; set; }

        private Agent _agent;

        public EBuffType GetRandomBuff() 
        {
            var selectRandom = UnityEngine.Random.Range(1, Enum.GetNames(typeof(EBuffType)).Length);
            return (EBuffType)selectRandom;
        }
        public void UpdateRandomStat()
        {
            var stats = CharacterStats;
            var selectBuff = GetRandomBuff();
            switch (selectBuff)
            {
                case EBuffType.None:
                    break;
                case EBuffType.MaxHealth:
                    stats.ExtraHealth = ConfigStats.ExtraHealth;
                    break;
                case EBuffType.RegerenHeal:
                    stats.HealthRegen = ConfigStats.HealthRegen;
                    break;
                case EBuffType.ExtraDamage:
                    stats.ExrtaDamage = ConfigStats.ExrtaDamage;
                    break;
                case EBuffType.ExtraScope:
                    stats.ExtraScope = ConfigStats.ExtraScope;
                    break;
                case EBuffType.ReloadReduction:
                    stats.ReloadReduction = ConfigStats.ReloadReduction;
                    break;
                case EBuffType.ExtraSpeed:
                    stats.ExtraSpeed = ConfigStats.ExtraSpeed;
                    break;
                default:
                    break;
            }

            CharacterStats = stats;
        }
        public void RestoreStats() 
        {
            CharacterStats = _baseStats; 
        }
        public void OnSpawned(Agent agent) 
        {
            _agent = agent;
            _baseStats = CharacterStats;
            AddEnergy(_initialEnergy);
            _visualsRoot.gameObject.SetActive(false);
        }
        public void OnFixdedUpdate() 
        {
            if (_agent == null) return;
            if (_agent.IsProAgent && FullEnergy())
                RandomActivate();
            if (IsActive == false) return;
            if (Object.IsProxy == true) return;
            if (_agent.Health.IsAlive == false) 
            {
                Desactivate();
                return;
            }
            float consume = _consume * Runner.DeltaTime;
            _energy -= consume;
            if (_energy <=0f)
            {
                _energy = 0f;
            }
            int? cooldownTargetTick = _cooldown.TargetTick;
            if (cooldownTargetTick.HasValue == true && cooldownTargetTick.Value <= Runner.Simulation.Tick)
            {
                Desactivate();
            }

        }
        public void OnDespawned() 
        {
            Desactivate();
        }
        public bool FullEnergy() 
        {
            var isFullEnergy = _energy >= _maxEnergy;
            return isFullEnergy;
        }
        public bool AddEnergy()
        {
            if (_energy >= _maxEnergy)
                return false;

            _energy = Mathf.Min(_energy + _energyPerHit, _maxEnergy);
            return true;
        }
        public bool AddEnergy(float energy) 
        {
            if (_energy >= _maxEnergy)
                return false;
            
            _energy = Mathf.Min(_energy+energy,_maxEnergy);
            return true;
        }
        public bool Activate() 
        {
            if (IsActive == true)
                return false;
            
            _state = true;
            _cooldown = TickTimer.CreateFromSeconds(Runner, _reloadTime);
            return true;
        }
        public bool RandomActivate() 
        {
            if (IsActive == true)
                return false;
            UpdateRandomStat();
            _state = true;
            _cooldown = TickTimer.CreateFromSeconds(Runner, _reloadTime);
            return true;
        }
        public void Desactivate() 
        {
            if (IsActive == false)
                return;
            RestoreStats();
            _state = false;
        }

        private void Awake()
        {
            
        }
        private void Activate_Internal() 
        {
            _visualsRoot.gameObject.SetActive(true);
        }
        private void Desactivate_Internal()
        {
            _visualsRoot.gameObject.SetActive(false);
        }
        public static void OnStateChanged(Changed<Powerups> changed) 
        {
            bool isActive = changed.Behaviour.IsActive;

            changed.LoadOld();
            bool wasActive = changed.Behaviour.IsActive;
            if (isActive == true && wasActive == false)
            {
                changed.LoadNew();
                changed.Behaviour.Activate_Internal();
            }
            else if (isActive == false && wasActive == true) 
            {
                changed.LoadNew();
                changed.Behaviour.Desactivate_Internal();
            }
        }
    }
}
