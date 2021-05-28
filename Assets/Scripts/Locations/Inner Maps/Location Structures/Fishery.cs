﻿using System;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

namespace Inner_Maps.Location_Structures {
    public class Fishery : ManMadeStructure {
        public override Vector3 worldPosition => structureObj.transform.position;
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

        List<ResourcePile> CheckForMultipleSameResourcePileInsideStructure() {
            return DoesMultipleResourcePileExist(GetAllFishResourcePileOnTiles());
        }

        List<ResourcePile> DoesMultipleResourcePileExist(List<ResourcePile> p_allPiles) {
            List<ResourcePile> fishPile = new List<ResourcePile>();
            p_allPiles.ForEach((eachList) => {
                if (eachList is FishPile) {
                    fishPile.Add(eachList);
                }
            });
            if (fishPile.Count > 1) {
                return fishPile;
            }
            return null;
        }

        List<ResourcePile> GetAllFishResourcePileOnTiles() {
            List<ResourcePile> pilePool = new List<ResourcePile>();
            passableTiles.ForEach((eachTile) => {
                if (eachTile.tileObjectComponent.objHere != null && eachTile.tileObjectComponent.objHere is ResourcePile resourcePile) {
                    pilePool.Add(resourcePile);
                }
            });
            return pilePool;
        }

        protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob) {
            producedJob = null;
            if (p_worker.currentSettlement.SettlementResources.GetRandomPileOfFishes() != null) {
                //do haul job
            } else if (CheckForMultipleSameResourcePileInsideStructure() != null) {
                //do combine resourcepiles job
            } else { 
                //fishing
            }
        }
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