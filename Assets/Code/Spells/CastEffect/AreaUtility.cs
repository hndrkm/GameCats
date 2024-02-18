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
        public static bool AreaCast(NetworkRunner runner,PlayerRef owner, int ownerObjectInstanceID, Vector2 castPosition, float areaRadio, LayerMask hitMask, List<LagCompensatedHit> validHits) 
        {
            validHits.Clear();
            var hits = ListPool.Get<LagCompensatedHit>(16);
            int hitCount = runner.LagCompensation.OverlapSphere(castPosition, areaRadio, owner, hits, hitMask,HitOptions.SubtickAccuracy);
            if (hits.Count <= 0) 
            {
                ListPool.Return(hits);
                return false;
            }
            var hitRoots = ListPool.Get<int>(16);
            Sort(hits, hitCount);
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

        public static void Sort(List<LagCompensatedHit> hits, int maxHits) 
        {
            while (true)
            {
                bool swap = false;
                for (int i = 0; i < maxHits; i++)
                {
                    for (int j = i+1; j < maxHits; j++) 
                    {
                        if (hits[j].Distance < hits[i].Distance)
                        {
                            LagCompensatedHit hit = hits[i];
                            hits[i] = hits[j];
                            hits[j] = hit;
                            swap = true;
                        }
                    }
                }
                if (swap == false)
                {
                    return;
                }
            }
        }
    }
}
