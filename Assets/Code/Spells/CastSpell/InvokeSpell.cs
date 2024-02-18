using Fusion;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace CatGame
{
    public class InvokeSpell : CastSpell
    {
        [SerializeField]
        private EffectInvoke _effectInvoke;
        [SerializeField]
        private float _areaRadio;
        [SerializeField]
        private float _minDespawnTime = 1;

        protected override bool CastInstaceSpell(Vector2 castPosition, Vector2 targetPosition, Vector2 direction, LayerMask hitMask, bool isFrist)
        {
            if (Object.IsProxy)
                return false;

            var predictionKey = new NetworkObjectPredictionKey
            {
                Byte0 = (byte)Runner.Simulation.Tick,
                Byte1 = (byte)Object.InputAuthority.PlayerId,
                Byte2 = (byte)Object.Id.Raw,
            };

            var area = Runner.Spawn(_effectInvoke, targetPosition, Quaternion.identity, Object.InputAuthority, BeforeAreaSpawned, predictionKey);

            if (area == null)
                return true;
            area.AutoCast(Owner, targetPosition, _areaRadio, hitMask, HitType);
            area.SetDespawnCooldown(Mathf.Min(_minDespawnTime, area.CastDespawnTime));

            void BeforeAreaSpawned(NetworkRunner runner, NetworkObject spawnedObject)
            {
                if (HasStateAuthority == true)
                    return;
                var area = spawnedObject.GetComponent<Area>();
                area.PredictedInputAuthority = Object.InputAuthority;
            }
            return true;
        }
    }
}
