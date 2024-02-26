using Fusion;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace CatGame
{
    public class InvokeSpell : BaseSpell
    {
        [SerializeField]
        private InvokeArea _effectInvoke;
        [SerializeField]
        private float _speed;
        [SerializeField]
        private float _radius;
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

            var area = Runner.Spawn(_effectInvoke, castPosition , Quaternion.identity, Object.InputAuthority, BeforeAreaSpawned, predictionKey);

            if (area == null)
                return true;
            area.AutoCast(Owner, castPosition, _speed, hitMask, HitType);
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
        protected override Vector2 GetCastPosition(int dispersion, int i)
        {
            var posOwner2d = new Vector2(Owner.transform.position.x, Owner.transform.position.y);
            var X = Mathf.Cos(dispersion * i * 10) * _radius;
            var Y = Mathf.Sin(dispersion * i * 10) * _radius;
            Debug.Log($"x {Mathf.Cos(dispersion * i)} y {Mathf.Sin(dispersion * i)}");
            return posOwner2d + new Vector2(X, Y);
        }
    }
}
