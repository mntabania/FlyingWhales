using UnityEngine;
using System.Collections.Generic;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class Lumberyard : ManMadeStructure {

        
        public override Vector3 worldPosition => structureObj.transform.position;

        public Lumberyard(Region location) : base(STRUCTURE_TYPE.LUMBERYARD, location){
            SetMaxHPAndReset(8000);
        }
        public Lumberyard(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(8000);
        }
                
        private void PopulateWoodPileListInsideStructure(List<TileObject> builtPilesInSideStructure) {
            List<TileObject> pilePool = GetTileObjectsOfType(TILE_OBJECT_TYPE.WOOD_PILE);
            if(pilePool != null) {
                for (int i = 0; i < pilePool.Count; i++) {
                    TileObject t = pilePool[i];
                    if (t.mapObjectState == MAP_OBJECT_STATE.BUILT && !t.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
                        builtPilesInSideStructure.Add(t);
                    }
                }
            }
        }

        private TileObject GetFirstTree() {
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
            ResourcePile woodPile = p_worker.homeSettlement.SettlementResources.GetRandomPileOfWoodsForHaul();
            if (woodPile != null && p_worker.structureComponent.workPlaceStructure.unoccupiedTiles.Count > 0) {
                p_worker.jobComponent.TryCreateHaulJob(woodPile, out producedJob);
                if(producedJob != null) {
                    return;
				}
            }
            //check if there are multiple woodpiles inside this structure
            List<TileObject> builtPilesInSideStructure = RuinarchListPool<TileObject>.Claim();
            PopulateWoodPileListInsideStructure(builtPilesInSideStructure);
            if (builtPilesInSideStructure.Count > 1) {
                p_worker.jobComponent.TryCreateCombineStockpile(builtPilesInSideStructure[0] as ResourcePile, builtPilesInSideStructure[1] as ResourcePile, out producedJob);
                if (producedJob != null) {
                    RuinarchListPool<TileObject>.Release(builtPilesInSideStructure);
                    return;
                }
            }
            RuinarchListPool<TileObject>.Release(builtPilesInSideStructure);
            //check if there are available tree that can be chopped
            TileObject tree = GetFirstTree();
            if (tree != null){
                p_worker.jobComponent.TriggerChopWood(tree, out producedJob);
                if (producedJob != null) {
                    return;
                }
            }
            
            if(TryCreateCleanJob(p_worker, out producedJob)) { return; }
        }
        
        #region Damage
        public override void OnTileDamaged(LocationGridTile tile, int amount, bool isPlayerSource) {
            //lumberyards can be damaged  by any tile
            AdjustHP(amount, isPlayerSource: isPlayerSource);
            OnStructureDamaged();
        }
        #endregion
    }
}