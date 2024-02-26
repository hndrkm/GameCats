using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    public abstract class Spell : ContextBehaviour
    {
        public string SepllID => _spellID;
        public int SpellSlot => _spellSlot;
        public EHitType HitType => _hitType;
        public string Displayname => _displayName;

        [Networked]
        public bool IsEquip { get; set;}
        [Networked,HideInInspector]
        public Agent Owner { get; set; }



        [SerializeField]
        private string _spellID;
        [SerializeField]
        private int _spellSlot;
        [SerializeField]
        private EHitType _hitType;
        [SerializeField]
        private string _displayName;

        public void EquipSpell() 
        {
            if (IsEquip ==true) 
            {
                return;
            }
            IsEquip = true;
        }
        public void UnequipSpell() 
        {
            if (IsEquip == false)
            {
                return;
            }
            IsEquip = false;
        }
        
        public virtual bool IsBusy(){ return false; }
        public abstract bool CanCast(bool inputDown);
        public abstract void Cast(Vector2 castPosition, Vector2 targetPosition,LayerMask hitMask);
        public virtual bool CanAinm() { return false; }
        public void SetParent(Transform parentTrasform) 
        {
            transform.SetParent(parentTrasform,false);
            transform.localPosition = Vector2.zero;
            transform.localRotation = Quaternion.identity;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            
        }


    }
}
