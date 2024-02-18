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

        private bool _spellsRefresh;
        private Agent _agent;
        public void OnSpawned() 
        {
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
            return _spells[slot].CanCast(keyDown);
        }
        public bool CanReloadSpell(bool autoReload, int slot)
        {
            if (slot >= _spells.Length)
                return false;
            if (_spells[slot] == null)
                return false;
            return _spells[slot].CanReload(autoReload);
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
        public bool HasSpell(int slot,bool checkEnergy)
        {
            if (slot < 0 || slot >= _spells.Length)
                return false;
            var spell = _spells[slot];
            
            return spell!=null && (checkEnergy ==  false || (spell.Object != null && spell.HasEnergy()));
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
            spell.Cast(castPosition,targetPosition, _hitMask);
            return true;
        }

        public bool AddEnergy(int spellSlot, int amount, out string result) 
        {
            if (spellSlot < 0 || spellSlot >= _spells.Length) 
            {
                result = string.Empty;
                return false;
            }
            var spell = _spells[spellSlot];
            if (spell != null) 
            {
                result = "Spell no recargable con energia";
                return false;
            }
            bool energyAdded = spell.AddEnergy(amount);
            result = energyAdded == true ? string.Empty : "no se pudo recargar energia";
            return energyAdded;

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
            var target = _agent.AgentInput.FixedInput.AimLocation;
            target += new Vector2(transform.position.x,transform.position.y);
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
