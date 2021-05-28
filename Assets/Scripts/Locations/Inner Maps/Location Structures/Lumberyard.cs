using UnityEngine;
using System.Collections.Generic;
namespace Inner_Maps.Location_Structures {
    public class Lumberyard : ManMadeStructure {
        // public override Vector3 worldPosition => structureObj.transform.position;
        public Lumberyard(Region location) : base(STRUCTURE_TYPE.LUMBERYARD, location){
            SetMaxHPAndReset(8000);
        }
        public Lumberyard(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(8000);
        }

        List<ResourcePile> CheckForMultipleSameResourcePileInsideStructure() {
            return DoesMultipleResourcePileExist(GetAllWoodResourcePileOnTiles());
        }

        List<ResourcePile> GetAllWoodResourcePileOnTiles() {
            List<ResourcePile> pilePool = new List<ResourcePile>();
            passableTiles.ForEach((eachTile) => {
                if (eachTile.tileObjectComponent.objHere != null && eachTile.tileObjectComponent.objHere is ResourcePile resourcePile) {
                    pilePool.Add(resourcePile);
                }
            });
            return pilePool;
        }

        List<ResourcePile> DoesMultipleResourcePileExist(List<ResourcePile> p_allPiles) {
            List<ResourcePile> woodPile = new List<ResourcePile>();
            p_allPiles.ForEach((eachList) => {
                if (eachList is WoodPile) {
                    woodPile.Add(eachList);
                }
            });
            if (woodPile.Count > 1) {
                return woodPile;
            }
            return null;
        }

        TileObject GetrandomTree(Character p_worker) {
            TileObject tree = p_worker.currentSettlement.SettlementResources.GetAvailableTree();
            return tree;
        }

        protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob) {
            producedJob = null;
            if (p_worker.currentSettlement.SettlementResources.GetRandomPileOfWoods() != null) {
                Debug.LogError(p_worker.name + " HAUL Lumberyard to resource pile");
                p_worker.homeSettlement.settlementJobTriggerComponent.TryCreateHaulJob(p_worker.currentSettlement.SettlementResources.GetRandomPileOfWoods());
                //do haul job
            } else if (CheckForMultipleSameResourcePileInsideStructure() != null) {
                //p_worker.homeSettlement.settlementJobTriggerComponent.combine(p_worker.currentSettlement.SettlementResources.GetRandomPileOfWoods());
                //do combine resourcepiles job
            } else if(GetrandomTree(p_worker) != null){
                Debug.LogError(p_worker.name + " CHOP Lumberyard to resource pile");
                TileObject targetTree = GetrandomTree(p_worker);
                p_worker.jobComponent.TriggerChopWood(targetTree);
                producedJob = p_worker.jobQueue.GetJob(JOB_TYPE.CHOP_WOOD);
                //do chop wood job
            }
        }
    }
}