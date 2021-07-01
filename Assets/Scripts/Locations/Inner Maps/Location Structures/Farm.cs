using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

namespace Inner_Maps.Location_Structures {
    public class Farm : ManMadeStructure {
        // public override Vector2 selectableSize { get; }
        // public override Vector3 worldPosition => structureObj.transform.position;
        public List<LocationGridTile> farmTiles { get; private set; }

        #region getters
        public override Type serializedData => typeof(SaveDataFarm);
        #endregion
        
        public Farm(Region location) : base(STRUCTURE_TYPE.FARM, location){
            // selectableSize = new Vector2(5f, 5f);
            wallsAreMadeOf = RESOURCE.WOOD;
            farmTiles = new List<LocationGridTile>();
        }
        public Farm(Region location, SaveDataManMadeStructure data) : base(location, data) {
            // selectableSize = new Vector2(5f, 5f);
            wallsAreMadeOf = RESOURCE.WOOD;
            farmTiles = new List<LocationGridTile>();
        }

        #region Loading
        public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure) {
            base.LoadReferences(saveDataLocationStructure);
            SaveDataFarm saveDataFarm = saveDataLocationStructure as SaveDataFarm;
            for (int i = 0; i < saveDataFarm.farmTiles.Length; i++) {
                TileLocationSave tileLocationSave = saveDataFarm.farmTiles[i];
                LocationGridTile tile = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(tileLocationSave);
                farmTiles.Add(tile);
            }
        }
        #endregion
        
        public override void Initialize() {
            base.Initialize();
            Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted);
        }
        protected override void AfterStructureDestruction(Character p_responsibleCharacter = null) {
            base.AfterStructureDestruction(p_responsibleCharacter);
            Messenger.RemoveListener(Signals.HOUR_STARTED, OnHourStarted);
        }

        private void OnHourStarted() {
            if(GameManager.Instance.currentTick == 120) { //6am
                List<TileObject> tileObjects = RuinarchListPool<TileObject>.Claim();
                PopulateCropsThatAreNotRipe(tileObjects);
                int numOfCropsToRipen = GameUtilities.RandomBetweenTwoNumbers(2, 3);
                for (int i = 0; i < numOfCropsToRipen; i++) {
                    if(tileObjects.Count > 0) {
                        int chosenIndex = GameUtilities.RandomBetweenTwoNumbers(0, tileObjects.Count - 1);
                        Crops chosenCrop = tileObjects[chosenIndex] as Crops;
                        chosenCrop.SetGrowthState(Crops.Growth_State.Ripe);
                        tileObjects.RemoveAt(chosenIndex);
                    } else {
                        break;
                    }
                }
                RuinarchListPool<TileObject>.Release(tileObjects);
            }
        }
        private void PopulateListOfFoodPilesOfSameType(List<TileObject> p_list) {
            PopulateListOfFoodPilesOfType(p_list, TILE_OBJECT_TYPE.CORN);
            if (p_list.Count <= 1) {
                p_list.Clear();
                PopulateListOfFoodPilesOfType(p_list, TILE_OBJECT_TYPE.PINEAPPLE);
                if (p_list.Count <= 1) {
                    p_list.Clear();
                    PopulateListOfFoodPilesOfType(p_list, TILE_OBJECT_TYPE.HYPNO_HERB);
                    if (p_list.Count <= 1) {
                        p_list.Clear();
                        PopulateListOfFoodPilesOfType(p_list, TILE_OBJECT_TYPE.ICEBERRY);
                        if (p_list.Count <= 1) {
                            p_list.Clear();
                            PopulateListOfFoodPilesOfType(p_list, TILE_OBJECT_TYPE.POTATO);
                        }
                    }
                }
            }
        }
        private void PopulateListOfFoodPilesOfType(List<TileObject> p_list, TILE_OBJECT_TYPE p_type) {
            List<TileObject> pilePool = GetTileObjectsOfType(p_type);
            if (pilePool != null && pilePool.Count > 1) {
                for (int i = 0; i < pilePool.Count; i++) {
                    TileObject t = pilePool[i];
                    if (t.mapObjectState == MAP_OBJECT_STATE.BUILT && !t.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
                        p_list.Add(t);
                    }
                }
            }
        }

        #region tilling section
        private GenericTileObject GetUntilledFarmTile() {
            for (int x = 0; x < farmTiles.Count; ++x) {
                LocationGridTile farmTile = farmTiles[x];
                if (!CheckIfTileIsTilled(farmTile) && !farmTile.tileObjectComponent.genericTileObject.HasJobTargetingThis(JOB_TYPE.TILL_TILE)) {
                    return farmTile.tileObjectComponent.genericTileObject;
                }
            }
            return null;
        }

        private bool CheckIfTileIsTilled(LocationGridTile p_targetTile) {
            if (p_targetTile.tileObjectComponent.objHere == null) {
                return false;
            }
            return (p_targetTile.tileObjectComponent.objHere.tileObjectType == TILE_OBJECT_TYPE.CORN_CROP ||
                p_targetTile.tileObjectComponent.objHere.tileObjectType == TILE_OBJECT_TYPE.HYPNO_HERB_CROP ||
                p_targetTile.tileObjectComponent.objHere.tileObjectType == TILE_OBJECT_TYPE.ICEBERRY_CROP ||
                p_targetTile.tileObjectComponent.objHere.tileObjectType == TILE_OBJECT_TYPE.PINEAPPLE_CROP ||
                p_targetTile.tileObjectComponent.objHere.tileObjectType == TILE_OBJECT_TYPE.POTATO_CROP);
        }
        #endregion tilling section

        #region harvesting section
        private TileObject GetHarvestableCrop() {
            List<TileObject> choices = RuinarchListPool<TileObject>.Claim();
            for (int x = 0; x < farmTiles.Count; ++x) {
                LocationGridTile farmTile = farmTiles[x];
                if (CheckIfTileHasHarvestableCrop(farmTile)) {
                    choices.Add(farmTile.tileObjectComponent.objHere);
                }
            }
            if (choices.Count > 0) {
                TileObject target = CollectionUtilities.GetRandomElement(choices);
                RuinarchListPool<TileObject>.Release(choices);
                return target;
            }
            RuinarchListPool<TileObject>.Release(choices);
            return null;
        }
        private bool CheckIfTileHasHarvestableCrop(LocationGridTile p_targetTile) {
            if (p_targetTile.tileObjectComponent.objHere == null) {
                return false;
            }

            Crops crops = p_targetTile.tileObjectComponent.objHere as Crops;
            if (crops == null) {
                return false;
            }

            return crops.currentGrowthState == Crops.Growth_State.Ripe;
            
            // return (p_targetTile.tileObjectComponent.objHere.tileObjectType == TILE_OBJECT_TYPE.CORN ||
            //     p_targetTile.tileObjectComponent.objHere.tileObjectType == TILE_OBJECT_TYPE.HYPNO_HERB ||
            //     p_targetTile.tileObjectComponent.objHere.tileObjectType == TILE_OBJECT_TYPE.ICEBERRY ||
            //     p_targetTile.tileObjectComponent.objHere.tileObjectType == TILE_OBJECT_TYPE.PINEAPPLE ||
            //     p_targetTile.tileObjectComponent.objHere.tileObjectType == TILE_OBJECT_TYPE.POTATO);
        }
		#endregion

        #region Farm Tiles
        public void AddFarmTile(LocationGridTile p_tile) {
            farmTiles.Add(p_tile);
        }
        #endregion

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
            ResourcePile pileToHaul = p_worker.homeSettlement.SettlementResources.GetRandomPileOfCropsForFarmHaul(p_worker.homeSettlement);
            if (pileToHaul != null && p_worker.structureComponent.workPlaceStructure.unoccupiedTiles.Count > 0) {
                //do haul job
                p_worker.jobComponent.TryCreateHaulJob(pileToHaul, out producedJob);
                if (producedJob != null) {
                    return;
                }
            }

            if (GameUtilities.RollChance(35)) {
                GenericTileObject untilledTileObject = GetUntilledFarmTile();
                if (untilledTileObject != null) {
                    //do till farm tile
                    p_worker.jobComponent.TriggerTillTile(untilledTileObject, out producedJob);
                    if (producedJob != null) {
                        return;
                    }
                }
            }
            
            //do combine resourcepiles job
            List<TileObject> builtPilesInSideStructure = RuinarchListPool<TileObject>.Claim();
            PopulateListOfFoodPilesOfSameType(builtPilesInSideStructure);
            if (builtPilesInSideStructure.Count > 1) {
                p_worker.jobComponent.TryCreateCombineStockpile(builtPilesInSideStructure[1] as ResourcePile, builtPilesInSideStructure[0] as ResourcePile, out producedJob);
                if (producedJob != null) {
                    RuinarchListPool<TileObject>.Release(builtPilesInSideStructure);
                    return;
                }
            }
            RuinarchListPool<TileObject>.Release(builtPilesInSideStructure);

            int chanceToHarvest = 100;
            int currentFoodInFarm = GetTotalResourceInStructure(RESOURCE.FOOD);
            if (currentFoodInFarm >= 180) {
                chanceToHarvest = 35;
            }
            if (GameUtilities.RollChance(chanceToHarvest)) {
                TileObject crop = GetHarvestableCrop();
                if (crop != null) {
                    //do harvest crops
                    p_worker.jobComponent.TriggerHarvestCrops(crop, out producedJob);
                    if (producedJob != null) {
                        return;
                    }
                }    
            }

            if(TryCreateCleanJob(p_worker, out producedJob)) { return; }
        }

        #region Damage
        public override void OnTileDamaged(LocationGridTile tile, int amount, bool isPlayerSource) {
            //farms can be damaged  by any tile
            AdjustHP(amount, isPlayerSource: isPlayerSource);
            OnStructureDamaged();
        }
        #endregion

        #region Testing
        public override string GetTestingInfo() {
            string info = base.GetTestingInfo();
            info = $"{info}\nFarm Tiles: {farmTiles.ComafyList()}";
            info = $"{info}\nTotal Food: {GetTotalResourceInStructure(RESOURCE.FOOD).ToString()}";
            return info;
        }
        #endregion
    }
}

#region Save Data
public class SaveDataFarm : SaveDataManMadeStructure {
    public TileLocationSave[] farmTiles;
    public override void Save(LocationStructure locationStructure) {
        base.Save(locationStructure);
        Farm farm = locationStructure as Farm;
        farmTiles = new TileLocationSave[farm.farmTiles.Count];
        for (int i = 0; i < farm.farmTiles.Count; i++) {
            LocationGridTile farmTile = farm.farmTiles[i];
            farmTiles[i] = new TileLocationSave(farmTile);
        }
    }
}
#endregion
