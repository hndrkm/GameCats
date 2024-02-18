using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Fusion.NetworkCharacterController;

namespace CatGame
{
    public class ExplodeArea : ContextBehaviour
    {
        [SerializeField]
        private LayerMask _hitMask;
        [SerializeField]
        private EHitType _hitType;

        [SerializeField]
        private float _innerRadius;
        [SerializeField]
        private float _outerRadius;

        [SerializeField]
        private float _innerHitValue;
        [SerializeField]
        private float _outerHitValue;
        [SerializeField]
        private float _despawnDelay;

        [SerializeField]
        private Transform _effectRoot;

        private TickTimer _despawnTimer;

        public override void Spawned()
        {
            base.Spawned();
            ShowEffect();
            Explode();
            
        }
        public override void FixedUpdateNetwork()
        {
            if (Object.HasStateAuthority == false)
                return;
            if (_despawnTimer.Expired(Runner) == false)
                return;
            Runner.Despawn(Object);
        }
        private void Explode() 
        {
            if (Object.HasStateAuthority == false)
                return;

            var hits = ListPool.Get<LagCompensatedHit>(16);
            var hitRoots = ListPool.Get<int>(16);

            var position = transform.position;

            int count = Runner.LagCompensation.OverlapSphere(position, _outerRadius, Object.InputAuthority, hits, _hitMask);

            var player = Context.NetworkGame.GetPlayer(Object.InputAuthority);
            var owner = player != null ? player.ActiveAgent : null;
            Debug.Log(count);
            for ( int i = 0; i < count; i++ ) 
            {
                var hit = hits[i];
                Debug.Log(hit.GameObject.name);
                if (hit.Hitbox == null)
                    continue;
                var hitTarget = hit.Hitbox.Root.GetComponent<IHitTarget>();
                if (hitTarget == null)
                    continue;
                int hitRootID = hit.Hitbox.Root.GetInstanceID();
                if (hitRoots.Contains(hitRootID) == true)
                    continue;

                var direction = hit.GameObject.transform.position - position;
                float distance = direction.magnitude;
                direction /= distance;
                if (Runner.GetPhysicsScene2D().Raycast(position, direction, distance, layerMask: 0) == true)
                    continue;

                hitRoots.Add(hitRootID);

                float damage = _innerHitValue;
                hit.Point = hit.GameObject.transform.position;
                hit.Normal = -direction;

                if (owner != null) 
                {
                    HitUtility.ProcessHit(owner, direction, hit, damage, _hitType, out HitData hitdata);
                }
                else
                {
                    HitUtility.ProcessHit(Object.InputAuthority, direction, hit, damage, _hitType, out HitData hitdata);
                }
            }

            ListPool.Return(hitRoots);
            ListPool.Return(hits);
            _despawnTimer = TickTimer.CreateFromSeconds(Runner, _despawnDelay);
        }
        private void ShowEffect() 
        {
            if (_effectRoot != null)
            { 
                _effectRoot.gameObject.SetActive(true);
                _effectRoot.localScale = Vector2.one * _outerRadius * 2;
            }
        }
    }
}
