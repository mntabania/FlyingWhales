using UnityEngine.Assertions;
namespace Goap.Job_Checkers {
    public class CombineStockpileApplicabilityChecker : JobApplicabilityChecker {
        public override string key => JobManager.Combine_Stockpile_Applicability;
        public override bool IsJobStillApplicable(JobQueueItem job) {
            GoapPlanJob goapPlanJob = job as GoapPlanJob;
            Assert.IsNotNull(goapPlanJob);
            
            ResourcePile pileToDeposit = goapPlanJob.targetPOI as ResourcePile;
            Assert.IsNotNull(pileToDeposit);
            
            TileObjectOtherData targetPileData = goapPlanJob.otherData[INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE][0] as TileObjectOtherData;
            ResourcePile targetPile = targetPileData.tileObject as ResourcePile;
            Assert.IsNotNull(targetPile);
            
            NPCSettlement npcSettlement = job.originalOwner as NPCSettlement;
            Assert.IsNotNull(npcSettlement);
            
            
            return targetPile.gridTileLocation != null
                   && targetPile.gridTileLocation.IsPartOfSettlement(npcSettlement)
                   && targetPile.structureLocation == npcSettlement.mainStorage
                   && pileToDeposit.gridTileLocation != null
                   && pileToDeposit.gridTileLocation.IsPartOfSettlement(npcSettlement)
                   && pileToDeposit.structureLocation == npcSettlement.mainStorage
                   && targetPile.resourceStorageComponent.HasEnoughSpaceFor(pileToDeposit.providedResource, pileToDeposit.resourceInPile);
        }
    }
}