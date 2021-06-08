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
        }
        #endregion
        
        public override string GetTestingInfo() {
            return $"{base.GetTestingInfo()}\nConnected Ocean {connectedOcean?.name}";
        }
        public override void OnUseStructureConnector(LocationGridTile p_usedConnector) {
            base.OnUseStructureConnector(p_usedConnector);
            Assert.IsTrue(p_usedConnector.structure is Ocean, $"{name} did not connect to a tile inside an Ocean!");
            connectedOcean = p_usedConnector.structure as Ocean;
            Assert.IsTrue(p_usedConnector.tileObjectComponent.objHere is FishingSpot, $"{name} did not connect to a tile with a Fishing Spot!");
            (p_usedConnector.tileObjectComponent.objHere as FishingSpot).SetConnectedFishingShack(this);
        }
        protected override void AfterStructureDestruction(Character p_responsibleCharacter = null) {
            base.AfterStructureDestruction(p_responsibleCharacter);
            connectedOcean = null;
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
                return fishingSpots[GameUtilities.RandomBetweenTwoNumbers(0, fishingSpots.Count - 1)];
            }
            return null;
        }
        protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob) {
            producedJob = null;
            ResourcePile pileToHaul = p_worker.homeSettlement.SettlementResources.GetRandomPileOfFishes();
            if (pileToHaul != null) {
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
                p_worker.jobComponent.TryCreateCombineStockpile(builtPilesInSideStructure[0] as ResourcePile, builtPilesInSideStructure[1] as ResourcePile, out producedJob);
                if (producedJob != null) {
                    RuinarchListPool<TileObject>.Release(builtPilesInSideStructure);
                    return;
                }
            }
            RuinarchListPool<TileObject>.Release(builtPilesInSideStructure);

            //Find Fish
            TileObject fishingSpot = GetRandomFishingSpot();
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
    
    public override void Save(LocationStructure locationStructure) {
        base.Save(locationStructure);
        Fishery fishingShack = locationStructure as Fishery;
        if (fishingShack.connectedOcean != null) {
            connectedFishingShackID = fishingShack.connectedOcean.persistentID;
        }
    }
}
#endregion