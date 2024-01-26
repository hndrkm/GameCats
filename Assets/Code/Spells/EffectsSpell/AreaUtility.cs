using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    public static class AreaUtility 
    {
        public static bool AreaCast() 
        {
            return AreaCast();
        }
        public static bool AreaCast(NetworkRunner runner,PlayerRef owner, int ownerObjectInstanceID, Vector2 castPosition, float areaEffect, LayerMask hitMask, List<LagCompensatedHit> validHits) 
        {
            validHits.Clear();
            var hits = ListPool.Get<LagCompensatedHit>(16);
            runner.LagCompensation.OverlapSphere(castPosition, areaEffect, owner, hits, hitMask,HitOptions.SubtickAccuracy);
            if (hits.Count <= 0) 
            {
                ListPool.Return(hits);
                return false;
            }
            var hitRoots = ListPool.Get<int>(16);
            
            for (int i = 0;i<hits.Count;i++) 
            {
                var hit = hits[i];
                int hitRootID = hit.Hitbox != null ? hit.Hitbox.Root.gameObject.GetInstanceID() : 0;
                if (hitRootID != 0)
                {
                    if (hitRootID == ownerObjectInstanceID)
                        continue;
                    if (hitRoots.Contains(hitRootID) == true)
                        continue;
                    hitRoots.Add(hitRootID);
                }
                validHits.Add(hits[i]);
            }
            ListPool.Return(hitRoots);
            ListPool.Return(hits);
            return validHits.Count > 0;
        }


    }
}
