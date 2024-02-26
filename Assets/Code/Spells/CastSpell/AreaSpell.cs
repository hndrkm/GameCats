using UnityEngine;

namespace CatGame
{
    public class AreaSpell : BaseSpell
    {
        public float Scope => _scope;
        [SerializeField]
        private ExplodeArea _effectArea;
        [SerializeField]
        private float _scope = 2;
        public float GetDamage() 
        {
            return _effectArea.GetDamage() + Owner.Powerups.CharacterStats.ExrtaDamage;
        }
        protected override bool CastInstaceSpell(Vector2 castPosition, Vector2 targetPosition, Vector2 direction, LayerMask hitMask, bool isFrist)
        {
            if (_effectArea!=null)
            {
                var area = Runner.Spawn(_effectArea, targetPosition, Quaternion.identity, Object.InputAuthority);
            }
            return true;            
        }
    }
}
