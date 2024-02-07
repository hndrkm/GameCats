using Fusion;
using UnityEditor;
using UnityEngine;

namespace CatGame
{
    public enum EHitAction 
    {
        None,
        Damage,
        Heal,
        Energy,
    }
    public struct HitData 
    {
        public EHitAction Action;
        public float Amount;
        public bool IsFatal;
        public Vector2 Position;
        public Vector2 Normal;
        public Vector2 Direction;
        public PlayerRef InstigatorRef;
        public IHitInstigator Instigator;
        public IHitTarget Target;
        public EHitType HitType;
    }
    public enum EHitType 
    {
        None, 
        Fire,
        Ice,
        Buff,
        Heal, 
        Energy,
    }
    public interface IHitTarget
    {
        Transform HitPivot { get; }
        void ProcessHit(ref HitData hit);
    }
    public interface IHitInstigator
    {
        void HitPerformed(HitData hit);
    }
    public static class HitUtility
    {
        public static bool ProcessHit(PlayerRef instigatorRef, Vector2 direction, LagCompensatedHit hit, float baseDamage, EHitType hitType, out HitData processedHit)
        {
            processedHit = default;

            IHitTarget target = hit.Hitbox != null ? hit.Hitbox.Root.GetComponent<IHitTarget>() : null;
            if (target == null)
                return false;

            processedHit.Action = EHitAction.Damage;
            processedHit.Amount = baseDamage;
            processedHit.Position = hit.Point;
            processedHit.Normal = hit.Normal;
            processedHit.Direction = direction;
            processedHit.Target = target;
            processedHit.InstigatorRef = instigatorRef;
            processedHit.HitType = hitType;
            return ProcessHit(ref processedHit);
        }
        public static bool ProcessHit(NetworkBehaviour instigator, Vector2 direction, LagCompensatedHit hit, float baseDamage, EHitType hitType, out HitData processedHit)
        {
            processedHit = default;

            IHitTarget target = hit.Hitbox != null ? hit.Hitbox.Root.GetComponent<IHitTarget>() : null;
            if (target == null)
                return false;

            if (hit.Hitbox.Root.gameObject == instigator)
                return false;

            processedHit.Action = EHitAction.Damage;
            processedHit.Amount = baseDamage;
            processedHit.Position = hit.Point;
            processedHit.Normal = hit.Normal;
            processedHit.Direction = direction;
            processedHit.Target = target;
            processedHit.InstigatorRef = instigator != null ? instigator.Object.InputAuthority : default;
            processedHit.Instigator = instigator != null ? instigator.GetComponent<IHitInstigator>() : null;
            processedHit.HitType = hitType;
            return ProcessHit(ref processedHit);
        }

        public static bool ProcessHit(NetworkBehaviour instigator, Collider collider, float damage, EHitType hitType, out HitData processedHit)
        {
            processedHit = new HitData();

            var target = collider.GetComponentInParent<IHitTarget>();
            if (target == null)
                return false;

            processedHit.Action = EHitAction.Damage;
            processedHit.Amount = damage;
            processedHit.InstigatorRef = instigator.Object.InputAuthority;
            processedHit.Instigator = instigator.GetComponent<IHitInstigator>();
            processedHit.Position = collider.transform.position;
            processedHit.Normal = (instigator.transform.position - collider.transform.position).normalized;
            processedHit.Direction = -processedHit.Normal;
            processedHit.Target = target;
            processedHit.HitType = hitType;

            return ProcessHit(ref processedHit);
        }


        public static bool ProcessHit(ref HitData hitData) 
        {
            hitData.Target.ProcessHit(ref hitData);
            if (hitData.Instigator != null && hitData.Target is Health)
            {
                hitData.Instigator.HitPerformed(hitData);
            }
            return true;
        }
    }
}
