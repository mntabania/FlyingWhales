using UnityEngine;
using System.Collections.Generic;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class Lumberyard : ManMadeStructure {

        List<TileObject> builtPilesInSideStructure = RuinarchListPool<TileObject>.Claim();
        
        public override Vector3 worldPosition => structureObj.transform.position;

        public Lumberyard(Region location) : base(STRUCTURE_TYPE.LUMBERYARD, location){
            SetMaxHPAndReset(8000);
        }
        public Lumberyard(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(8000);
        }
                
        void CreateWoodPileListInsideStructure() {
            List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
            builtPilesInSideStructure.Clear();
            pilePool = GetTileObjectsOfType(TILE_OBJECT_TYPE.WOOD_PILE);
            if(pilePool != null) {
                pilePool.ForEach((eachList) => {
                    if (eachList.mapObjectState == MAP_OBJECT_STATE.BUILT && !((eachList as TileObject).HasJobTargetingThis(JOB_TYPE.HAUL))) {
                        builtPilesInSideStructure.Add(eachList);
                    }
                });
                RuinarchListPool<TileObject>.Release(pilePool);
            }
        }

        TileObject GetRandomTree() {
            for(int x = 0; x < occupiedArea.tileObjectComponent.itemsInArea.Count; ++x) {
                if (occupiedArea.tileObjectComponent.itemsInArea[x] is TreeObject) {
                    return occupiedArea.tileObjectComponent.itemsInArea[x];
                }
			}
            return null;
        }

        protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob) {
            producedJob = null;
            //check if there are woodpiles that can be hauled inside settlement
            ResourcePile woodPile = p_worker.homeSettlement.SettlementResources.GetRandomPileOfWoods();
            if (woodPile != null) {
                p_worker.jobComponent.TryCreateHaulJob(woodPile, out producedJob);
                if(producedJob != null) {
                    return;
				}
            }
            //check if there are multiple woodpiles inside this structure
            CreateWoodPileListInsideStructure();
            if (builtPilesInSideStructure != null && builtPilesInSideStructure.Count > 1) {
                p_worker.jobComponent.TryCreateCombineStockpile(builtPilesInSideStructure[0] as ResourcePile, builtPilesInSideStructure[1] as ResourcePile, out producedJob);
                if (producedJob != null) {
                    return;
                }
            }
            //check if there are available tree that can be chopped
            TileObject tree = GetRandomTree();
            if (tree != null){
                TileObject targetTree = GetRandomTree();
                p_worker.jobComponent.TriggerChopWood(targetTree, out producedJob);
            }
        }
    }
}