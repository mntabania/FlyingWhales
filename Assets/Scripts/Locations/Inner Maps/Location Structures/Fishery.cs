using System;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using UtilityScripts;

namespace Inner_Maps.Location_Structures {
    public class Fishery : ManMadeStructure {
        // public override Vector3 worldPosition => structureObj.transform.position;
        public override Type serializedData => typeof(SaveDataFishery);
        public Ocean connectedOcean { get; private set; }
        public FishingSpot connectedFishingSpot { get; private set; }
        
        public Fishery(Region location) : base(STRUCTURE_TYPE.FISHERY, location) {
            SetMaxHPAndReset(4000);
        }
        public Fishery(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(4000);
        }
        
        #region Loading
        public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure) {
            base.LoadReferences(saveDataLocationStructure);
            SaveDataFishery saveDataFishingShack = saveDataLocationStructure as SaveDataFishery;
            if (!string.IsNullOrEmpty(saveDataFishingShack.connectedFishingShackID)) {
                connectedOcean = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataFishingShack.connectedFishingShackID) as Ocean;
            }
            if (!string.IsNullOrEmpty(saveDataFishingShack.connectedFishingSpotID)) {
                connectedFishingSpot = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(saveDataFishingShack.connectedFishingSpotID) as FishingSpot;
            }
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
        
        public override string GetTestingInfo() {
            return $"{base.GetTestingInfo()}\nConnected Ocean {connectedOcean?.name}";
        }
        public override void OnUseStructureConnector(LocationGridTile p_usedConnector) {
            base.OnUseStructureConnector(p_usedConnector);
            Assert.IsTrue(p_usedConnector.structure is Ocean, $"{name} did not connect to a tile inside an Ocean!");
            connectedOcean = p_usedConnector.structure as Ocean;
            var fishingSpot = p_usedConnector.tileObjectComponent.objHere as FishingSpot;
            Assert.IsNotNull(fishingSpot, $"{name} did not connect to a tile with a Fishing Spot!");
            connectedFishingSpot = fishingSpot;
            fishingSpot.SetConnectedFishingShack(this);
        }
        protected override void AfterStructureDestruction(Character p_responsibleCharacter = null) {
            base.AfterStructureDestruction(p_responsibleCharacter);
            connectedOcean = null;
            connectedFishingSpot = null;
        }
        private void PopulateFishPileListInsideStructure(List<TileObject> builtPilesInSideStructure) {
            List<TileObject> pilePool = GetTileObjectsOfType(TILE_OBJECT_TYPE.FISH_PILE);
            if (pilePool != null) {
                for (int i = 0; i < pilePool.Count; i++) {
                    TileObject t = pilePool[i];
                    if (t.mapObjectState == MAP_OBJECT_STATE.BUILT && !t.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
                        builtPilesInSideStructure.Add(t);
                    }
                }
            }
        }
        private TileObject GetRandomFishingSpot() {
            List<TileObject> fishingSpots = connectedOcean.GetTileObjectsOfType(TILE_OBJECT_TYPE.FISHING_SPOT);
            if (fishingSpots != null && fishingSpots.Count > 0) {
                List<TileObject> fishingSpotChoices = RuinarchListPool<TileObject>.Claim();
                // return fishingSpots[GameUtilities.RandomBetweenTwoNumbers(0, fishingSpots.Count - 1)];
                for (int i = 0; i < fishingSpots.Count; i++) {
                    TileObject fishingSpot = fishingSpots[i];
                    if (fishingSpot.gridTileLocation != null && fishingSpot.gridTileLocation.area.settlementOnArea == settlementLocation) {
                        //only pick fishing spots that are part of the settlement
                        fishingSpotChoices.Add(fishingSpot);
                    }
                }
                if (fishingSpotChoices.Count > 0) {
                    TileObject randomFishingSpot = CollectionUtilities.GetRandomElement(fishingSpotChoices);
                    RuinarchListPool<TileObject>.Release(fishingSpotChoices);
                    return randomFishingSpot;
                }
                RuinarchListPool<TileObject>.Release(fishingSpotChoices);
            }
            return null;
        }
        protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob) {
            producedJob = null;
            ResourcePile pileToHaul = p_worker.homeSettlement.SettlementResources.GetRandomPileOfFishesForFisheryHaul(p_worker.homeSettlement);
            if (pileToHaul != null && p_worker.structureComponent.workPlaceStructure.unoccupiedTiles.Count > 0) {
                //do haul job
                p_worker.jobComponent.TryCreateHaulJob(pileToHaul, out producedJob);
                if (producedJob != null) {
                    return;
                }
            }

            //do combine resourcepiles job
            List<TileObject> builtPilesInSideStructure = RuinarchListPool<TileObject>.Claim();
            PopulateFishPileListInsideStructure(builtPilesInSideStructure);
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

            if (TryCreateCleanJob(p_worker, out producedJob)) { return; }
            
            //Find Fish
            TileObject fishingSpot = GetRandomFishingSpot();
            if (fishingSpot == null) { fishingSpot = connectedFishingSpot; }
            if (fishingSpot != null) {
                //do harvest crops
                p_worker.jobComponent.TriggerFindFish(fishingSpot as FishingSpot, out producedJob);
            }
        }
        
        #region Damage
        public override void OnTileDamaged(LocationGridTile tile, int amount, bool isPlayerSource) {
            //fisheries can be damaged  by any tile
            AdjustHP(amount, isPlayerSource: isPlayerSource);
            OnStructureDamaged();
        }
        #endregion
    }
}

#region Save Data
public class SaveDataFishery : SaveDataManMadeStructure {

    public string connectedFishingShackID;
    public string connectedFishingSpotID;
    
    public override void Save(LocationStructure locationStructure) {
        base.Save(locationStructure);
        Fishery fishingShack = locationStructure as Fishery;
        if (fishingShack.connectedOcean != null) {
            connectedFishingShackID = fishingShack.connectedOcean.persistentID;
        }
        if (fishingShack.connectedFishingSpot != null) {
            connectedFishingSpotID = fishingShack.connectedFishingSpot.persistentID;
        }
    }
}
#endregion