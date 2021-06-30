using UnityEngine;
using System.Collections.Generic;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class ButchersShop : ManMadeStructure {

        //List<TileObject> builtPilesInSideStructure = RuinarchListPool<TileObject>.Claim();
        public override Vector3 worldPosition => structureObj.transform.position;

        public ButchersShop(Region location) : base(STRUCTURE_TYPE.BUTCHERS_SHOP, location) {
            SetMaxHPAndReset(4000);
        }
        public ButchersShop(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(4000);
        }

        //List<ResourcePile> CheckForMultipleSameResourcePileInsideStructure() {
        //    return DoesMultipleResourcePileExist(GetAllMeatResourcePileOnTiles());
        //}

        //List<ResourcePile> DoesMultipleResourcePileExist(List<ResourcePile> p_allPiles) {
        //    List<ResourcePile> animalMeat = new List<ResourcePile>();
        //    List<ResourcePile> humanMeat = new List<ResourcePile>();
        //    List<ResourcePile> elfMeat = new List<ResourcePile>();
        //    p_allPiles.ForEach((eachList) => {
        //        switch (eachList.tileObjectType) {
        //            case TILE_OBJECT_TYPE.ANIMAL_MEAT:
        //            animalMeat.Add(eachList);
        //            break;
        //            case TILE_OBJECT_TYPE.HUMAN_MEAT:
        //            humanMeat.Add(eachList);
        //            break;
        //            case TILE_OBJECT_TYPE.ELF_MEAT:
        //            elfMeat.Add(eachList);
        //            break;
        //        }
        //    });
        //    if (animalMeat.Count > 1) {
        //        return animalMeat;
        //    }
        //    if (humanMeat.Count > 1) {
        //        return animalMeat;
        //    }
        //    if (elfMeat.Count > 1) {
        //        return animalMeat;
        //    }
        //    return null;
        //}

        //List<ResourcePile> GetAllMeatResourcePileOnTiles() {
        //    List<ResourcePile> pilePool = new List<ResourcePile>();
        //    passableTiles.ForEach((eachTile) => {
        //        if (eachTile.tileObjectComponent.objHere != null && eachTile.tileObjectComponent.objHere is FoodPile resourcePile) {
        //            pilePool.Add(resourcePile);
        //        }
        //    });
        //    return pilePool;
        //}
        private void SetListToVariable(List<TileObject> builtPilesInSideStructure) {
            PopulateMeatList(builtPilesInSideStructure, TILE_OBJECT_TYPE.ANIMAL_MEAT);
            if (builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure.Clear();
            PopulateMeatList(builtPilesInSideStructure, TILE_OBJECT_TYPE.HUMAN_MEAT);
            if (builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure.Clear();
            PopulateMeatList(builtPilesInSideStructure, TILE_OBJECT_TYPE.ELF_MEAT);
            if (builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure.Clear();
            PopulateMeatList(builtPilesInSideStructure, TILE_OBJECT_TYPE.RAT_MEAT);
            if (builtPilesInSideStructure.Count > 1) {
                return;
            }
        }
        private void PopulateMeatList(List<TileObject> p_list, TILE_OBJECT_TYPE p_type) {
            List<TileObject> unsortedList = GetTileObjectsOfType(p_type);
            if (unsortedList != null) {
                for (int i = 0; i < unsortedList.Count; i++) {
                    TileObject t = unsortedList[i];
                    if (t.mapObjectState == MAP_OBJECT_STATE.BUILT && !t.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
                        p_list.Add(t);
                    }
                }
            }
        }

        #region Worker
        public override bool CanHireAWorker() {
            return true;
        }
        #endregion
        
        #region Purchasing
        public override bool CanPurchaseFromHere(Character p_buyer, out bool needsToPay, out int buyerOpinionOfWorker) {
            needsToPay = true;
            buyerOpinionOfWorker = 0;
            return true; //anyone can buy from food producing structures, but everyone also needs to pay. NOTE: It is intended that villagers can buy from unassigned structures
        }
        #endregion

        protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob) {
            producedJob = null;

            ResourcePile foodPile = p_worker.homeSettlement.SettlementResources.GetRandomPileOfMeatsForButchersShopHaul(p_worker.homeSettlement, p_worker);
            if (foodPile != null && p_worker.structureComponent.workPlaceStructure.unoccupiedTiles.Count > 0) {
                p_worker.jobComponent.TryCreateHaulJob(foodPile, out producedJob);
                if (producedJob != null) {
                    return;
                }
            }

            List<TileObject> builtPilesInSideStructure = RuinarchListPool<TileObject>.Claim();
            SetListToVariable(builtPilesInSideStructure);
            //List<ResourcePile> multiplePiles = CheckForMultipleSameResourcePileInsideStructure();
            if (builtPilesInSideStructure.Count > 1) {
                //always ensure that the first pile is the pile that all other piles will be dropped to, this is to prevent complications
                //when multiple workers are combining piles, causing targets of other jobs to mess up since their target pile was carried.
                p_worker.jobComponent.TryCreateCombineStockpile(builtPilesInSideStructure[1] as ResourcePile, builtPilesInSideStructure[0] as ResourcePile, out producedJob);
                if (producedJob != null) {
                    RuinarchListPool<TileObject>.Release(builtPilesInSideStructure);
                    return;
				}
            }
            RuinarchListPool<TileObject>.Release(builtPilesInSideStructure);

            Summon targetForButchering = p_worker.homeSettlement.SettlementResources.GetFirstButcherableAnimal(p_worker.homeSettlement);
            if (targetForButchering != null){
                p_worker.jobComponent.CreateButcherJob(targetForButchering, JOB_TYPE.MONSTER_BUTCHER, out producedJob);
                if (producedJob != null) {
                    return;
                }
            }
            
            if(TryCreateCleanJob(p_worker, out producedJob)) { return; }
        }
    }
}