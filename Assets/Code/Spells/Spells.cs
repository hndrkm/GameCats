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
    public class Spells : NetworkBehaviour,IBeforeTick
    {
        [SerializeField]
        private Spell[] _initialSpells;
        [SerializeField]
        private SpellSlot[] _slots;
        [SerializeField]
        private LayerMask _hitMask;
        [Networked]
        private NetworkArray<Spell> _spells { get; }

        private bool _spellsRefresh;
        private Agent _agent;
        public void OnSpawned() 
        {
            if (Object.HasInputAuthority == false)
            { return; }
            int bestSpellSlot = 0;
            for (int i = 0; i < _initialSpells.Length; i++)
            {
                var spellPrefab = _initialSpells[i];
                if (spellPrefab == null)
                    continue;
                var spell = Runner.Spawn(spellPrefab,inputAuthority : Object.InputAuthority);
                AddSpell(spell);
                if (bestSpellSlot == 0)
                    bestSpellSlot = spell.SpellSlot;
                _spellsRefresh = true;
            }
        }
        public bool CanCastSpell(bool keyDown, int slot) 
        {
            if (_spells.Length > slot)
                return false;
            if (_spells[slot] == null)
                return false;
            return _spells[slot].CanCast(keyDown);
        }
        public bool CanAim(int slot) 
        {
            if (_spells.Length > slot)
                return false;
            if (_spells[slot] == null)
                return false;
            return _spells[slot].CanAinm();
        }
        public Spell GetSpell(int slot) 
        {
            return _spells[slot];
        }
        public bool Cast(int spellSlot) 
        {
            var spell = _spells[spellSlot];
            if (spell == null)
                return false;

            spell.Cast(GetTargetPoint(), _hitMask);
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
        }
        private void PickupSpell(Spell spell) 
        {
            if(spell == null)
                return;
            AddSpell(spell);
        }
        private Vector2 GetTargetPoint() 
        {
            var target = _agent.AgentInput.CachedInput.AimLocation;
            target += new Vector2(transform.position.x,transform.position.y);
            Debug.Log(target);
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
