using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    public class AreaSpell : CastSpell
    {
        [SerializeField]
        private ExplodeArea _effectArea;

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
