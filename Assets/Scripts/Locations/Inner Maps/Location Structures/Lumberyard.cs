using UnityEngine;
using System.Collections.Generic;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class Lumberyard : ManMadeStructure {

        List<TileObject> builtPiles = RuinarchListPool<TileObject>.Claim();
        public override Vector3 worldPosition => structureObj.transform.position;
        public Lumberyard(Region location) : base(STRUCTURE_TYPE.LUMBERYARD, location){
            SetMaxHPAndReset(8000);
        }
        public Lumberyard(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(8000);
        }

        
        void CreateWoodPileList() {
            List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
            builtPiles.Clear();
            pilePool = GetTileObjectsOfType(TILE_OBJECT_TYPE.WOOD_PILE);
            pilePool.ForEach((eachList) => {
                if (eachList.mapObjectState == MAP_OBJECT_STATE.BUILT) {
                    builtPiles.Add(eachList);
                }
            });
            RuinarchListPool<TileObject>.Release(pilePool);
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
            CreateWoodPileList();
            Debug.LogError(builtPiles.Count);
            if (builtPiles != null && builtPiles.Count > 0) {
                Debug.LogError(p_worker.name + " HAUL Lumberyard to resource pile");
                p_worker.jobComponent.TryCreateHaulJob(builtPiles[0] as ResourcePile, out producedJob);
                if(producedJob != null) {
                    return;
				}
            } 
            if (builtPiles != null && builtPiles.Count > 1) {
                Debug.LogError(p_worker.name + " COMBINE WOOD PILE Lumberyard to resource pile");
                p_worker.jobComponent.TryCreateCombineStockpile(builtPiles[0] as ResourcePile, builtPiles[1] as ResourcePile, out producedJob);
                if (producedJob != null) {
                    return;
                }
            }
            TileObject tree = GetRandomTree();
            if (tree != null){
                Debug.LogError(p_worker.name + " CHOP Lumberyard to resource pile");
                TileObject targetTree = GetRandomTree();
                p_worker.jobComponent.TriggerChopWood(targetTree, out producedJob);
            }
        }
    }
}