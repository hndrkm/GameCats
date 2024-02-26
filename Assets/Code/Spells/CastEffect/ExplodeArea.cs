using Fusion;
using UnityEngine;

namespace CatGame
{
    public class ExplodeArea : ContextBehaviour
    {
        [SerializeField]
        private LayerMask _hitMask;
        [SerializeField]
        private EHitType _hitType;

        [SerializeField]
        private float _radius;

        [SerializeField]
        private float _hitValue;
        [SerializeField]
        private float _despawnDelay;

        [SerializeField]
        private Transform _effectRoot;

        private TickTimer _despawnTimer;
        public float GetDamage() 
        {
            if (Object == null)
            {
                return _hitValue;
            }
            return _hitValue + Context.NetworkGame.GetPlayer(Object.InputAuthority).ActiveAgent.Powerups.CharacterStats.ExrtaDamage;
        }
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

            int count = Runner.LagCompensation.OverlapSphere(position, _radius, Object.InputAuthority, hits, _hitMask);

            var player = Context.NetworkGame.GetPlayer(Object.InputAuthority);
            var owner = player != null ? player.ActiveAgent : null;
            for ( int i = 0; i < count; i++ ) 
            {
                var hit = hits[i];
                if (hit.Hitbox == null)
                    continue;
                var hitTarget = hit.Hitbox.Root.GetComponent<IHitTarget>();
                if (hitTarget == null)
                    continue;
                
                int hitRootID2 = hit.Hitbox.Root.gameObject.GetInstanceID();
                Debug.Log($"{Object.InputAuthority}  {hitRootID2}");
                if (hitRootID2 == owner.GetInstanceID())
                {
                    continue;
                }

                int hitRootID = hit.Hitbox.Root.GetInstanceID();
                if (hitRoots.Contains(hitRootID) == true)
                    continue;
                var direction = hit.GameObject.transform.position - position;
                float distance = direction.magnitude;
                direction /= distance;
                if (Runner.GetPhysicsScene2D().Raycast(position, direction, distance, layerMask: 0) == true)
                    continue;

                hitRoots.Add(hitRootID);

                float damage = GetDamage();
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
                _effectRoot.localScale = Vector2.one * _radius * 2;
            }
        }
    }
}
