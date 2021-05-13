using System.Collections.Generic;
using UnityEngine.Assertions;
using UtilityScripts;

namespace Goap.Job_Checkers {
    public class ProduceResourceApplicabilityChecker : JobApplicabilityChecker {
        public override string key => JobManager.Produce_Resource_Applicability;
        
        public const int MinimumFood = 100;
        public const int MinimumMetal = 100;
        public const int MinimumStone = 100;
        public const int MinimumWood = 100;
        
        public override bool IsJobStillApplicable(JobQueueItem job) {
            GoapPlanJob goapPlanJob = job as GoapPlanJob;
            Assert.IsNotNull(goapPlanJob);
            NPCSettlement settlement = job.originalOwner as NPCSettlement;
            Assert.IsNotNull(settlement);

            RESOURCE resource;
            if (job.jobType == JOB_TYPE.PRODUCE_FOOD) {
                resource = RESOURCE.FOOD;
            } else if (job.jobType == JOB_TYPE.PRODUCE_WOOD) {
                resource = RESOURCE.WOOD;
            } else if (job.jobType == JOB_TYPE.PRODUCE_STONE) {
                resource = RESOURCE.STONE;
            } else {
                resource = RESOURCE.METAL;
            }
            
            return GetTotalResource(resource, settlement) < GetMinimumResource(resource);
        }
        
        private int GetTotalResource(RESOURCE resourceType, NPCSettlement settlement) {
            int resource = 0;
            List<TileObject> piles = RuinarchListPool<TileObject>.Claim();
            settlement.mainStorage.PopulateTileObjectsOfType<ResourcePile>(piles);
            for (int i = 0; i < piles.Count; i++) {
                ResourcePile resourcePile = piles[i] as ResourcePile;
                if (resourcePile.providedResource == resourceType) {
                    resource += resourcePile.resourceInPile;	
                }
            }
            RuinarchListPool<TileObject>.Release(piles);
            return resource;
        }
        private int GetMinimumResource(RESOURCE resource) {
            switch (resource) {
                case RESOURCE.FOOD:
                    return MinimumFood;
                case RESOURCE.WOOD:
                    return MinimumWood;
                case RESOURCE.METAL:
                    return MinimumMetal;
                case RESOURCE.STONE:
                    return MinimumStone;
            }
            throw new System.Exception($"There is no minimum resource for {resource.ToString()}");
        }
    }
}