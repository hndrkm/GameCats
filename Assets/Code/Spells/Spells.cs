using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    [Serializable]
    public class SpellSlot 
    {
        public Transform Active;
        public Transform Inactive;
    }
    public class Spells : NetworkBehaviour, IBeforeTick
    {
        [SerializeField]
        private Transform _aimVisual;
        [SerializeField]
        private Spell[] _initialSpells;
        [SerializeField]
        private SpellSlot[] _slots;
        [SerializeField]
        private LayerMask _hitMask;
        [Networked,Capacity(8)]
        private NetworkArray<Spell> _spells { get; }

        private bool _hasPower;
        private bool _spellsRefresh;
        private Agent _agent;

        private bool refreshTarget;
        private Vector2 target = Vector2.zero;
        public void OnSpawned() 
        {
            target = transform.position;
            _spellsRefresh = true;
            if (Object.HasStateAuthority == false)
                return;

            for (int i = 0; i < _initialSpells.Length; i++)
            {
                var spellPrefab = _initialSpells[i];
                if (spellPrefab == null)
                    continue;
                var spell = Runner.Spawn(spellPrefab,inputAuthority : Object.InputAuthority);
                AddSpell(spell);
            }
        }
        public void OnDespawned() 
        {

            for (int i = 0; i < _spells.Length; i++)
            {
                if (_spells[i] != null)
                {
                    Runner.Despawn(_spells[i].Object);
                    _spells.Set(i, null);
                }
            }
        }
        public void OnFixedUpdate() 
        {
            if(Object.HasStateAuthority == false)
                return;
            if (_agent.Health.IsAlive == false)
                return;

            if (_agent.Powerups.FullEnergy() == true)
            {
                _hasPower = true;
            }
            else
            {
                _hasPower = false;
            }

        }
        public void OnLateFixedUpdate() 
        {
            
        }
        public void OnRender() 
        {
        
        }
        public bool CanCastSpell(bool keyDown, int slot) 
        {
            
            if (slot >= _spells.Length)
                return false;
            if (_spells[slot] == null)
                return false;
            if (_hasPower == false && slot == 1)
            {
                return false;
            }
            return _spells[slot].CanCast(keyDown);
        }
        
        public bool CanAim(int slot) 
        {
            if (slot >= _spells.Length)
                return false;
            if (_spells[slot] == null)
                return false;
            return _spells[slot].CanAinm();
        }
        public void TryInteract() { }
        public bool HasSpell(int slot)
        {
            if (slot < 0 || slot >= _spells.Length)
                return false;
            var spell = _spells[slot];
            
            return spell!=null;
        }
        public Spell GetSpell(int slot) 
        {
            return _spells[slot];
        }

        public void Aim() 
        {
            _aimVisual.transform.position = GetTargetPoint();
        }
        public bool Cast(int spellSlot) 
        {
            var spell = _spells[spellSlot];
            if (spell == null)
                return false;
            var castPosition = _spells[spellSlot].gameObject.transform.position;
            var targetPosition = GetTargetPoint();
            target = transform.position;
            refreshTarget = true;
            spell.Cast(castPosition,targetPosition, _hitMask);
            return true;
        }

        
        void IBeforeTick.BeforeTick() 
        {
            RefreshSpells();
        }
        protected void Awake()
        {
            _agent = GetComponent<Agent>();
        }

        private void RefreshSpells() 
        {
            if (_spellsRefresh == false)
            {
                return;
            }
            for (int i = 0; i < _spells.Length; i++)
            {
                if (_spells[i] == null)
                {
                    continue;
                }
                _spells[i].SetParent(_slots[i].Active);
            }
            _spellsRefresh = false;
        }
        private void PickupSpell(Spell spell) 
        {
            if(spell == null)
                return;
            AddSpell(spell);
        }
        private Vector2 GetTargetPoint() 
        {
            if (refreshTarget == true)
            {
                target = transform.position;
            }
            refreshTarget = false;
            var posXY = new Vector2(transform.position.x, transform.position.y);
            var dir = _agent.AgentInput.FixedInput.AimLocation - posXY;
            target += dir.normalized * 5 * Runner.DeltaTime; 
            return target;
        }
        private void AddSpell(Spell spell) 
        {
            if (spell == null)
                return;
            RemoveSpell(spell.SpellSlot);
            spell.Owner = _agent;
            spell.Object.AssignInputAuthority(Object.InputAuthority);
            _spells.Set(spell.SpellSlot,spell);
        }
        private void RemoveSpell(int slot) 
        {
            var spell = _spells[slot];
            if (spell == null)
                return;
            if (spell.Owner != null)
                spell.Owner = null;
            
            spell.Object.RemoveInputAuthority();
            _spells.Set(slot, null);
        }
    }
}
