using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

namespace Inner_Maps.Location_Structures {
    public class Farm : ManMadeStructure {
        // public override Vector2 selectableSize { get; }
        // public override Vector3 worldPosition => structureObj.transform.position;
        public Farm(Region location) : base(STRUCTURE_TYPE.FARM, location){
            // selectableSize = new Vector2(5f, 5f);
            wallsAreMadeOf = RESOURCE.WOOD;
        }
        public Farm(Region location, SaveDataManMadeStructure data) : base(location, data) {
            // selectableSize = new Vector2(5f, 5f);
            wallsAreMadeOf = RESOURCE.WOOD;
        }

        public List<LocationGridTile> farmTile => (structureObj as FarmStructureObject).farmTiles;

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

        List<ResourcePile> CheckForMultipleSameResourcePileInsideStructure() {
            return DoesMultipleResourcePileExist(GetAllCropsResourcePileOnTiles());
        }

        List<ResourcePile> DoesMultipleResourcePileExist(List<ResourcePile> p_allPiles) {
            List<ResourcePile> cornPile = new List<ResourcePile>();
            List<ResourcePile> pineApplePile = new List<ResourcePile>();
            List<ResourcePile> hypnoPile = new List<ResourcePile>();
            List<ResourcePile> iceBerryPile = new List<ResourcePile>();
            List<ResourcePile> potatoPile = new List<ResourcePile>();
            p_allPiles.ForEach((eachList) => {
                switch (eachList.tileObjectType) {
                    case TILE_OBJECT_TYPE.CORN:
                    cornPile.Add(eachList);
                    break;
                    case TILE_OBJECT_TYPE.PINEAPPLE:
                    pineApplePile.Add(eachList);
                    break;
                    case TILE_OBJECT_TYPE.HYPNO_HERB:
                    hypnoPile.Add(eachList);
                    break;
                    case TILE_OBJECT_TYPE.ICEBERRY:
                    iceBerryPile.Add(eachList);
                    break;
                    case TILE_OBJECT_TYPE.POTATO:
                    potatoPile.Add(eachList);
                    break;
                }
            });
            if(cornPile.Count > 1) {
                return cornPile;
			}
            if (pineApplePile.Count > 1) {
                return cornPile;
            }
            if (hypnoPile.Count > 1) {
                return cornPile;
            }
            if (iceBerryPile.Count > 1) {
                return cornPile;
            }
            if (potatoPile.Count > 1) {
                return cornPile;
            }
            return null;
        }

        List<ResourcePile> GetAllCropsResourcePileOnTiles() {
            List<ResourcePile> pilePool = new List<ResourcePile>();
            passableTiles.ForEach((eachTile) => {
                if (eachTile.tileObjectComponent.objHere != null && eachTile.tileObjectComponent.objHere is ResourcePile resourcePile) {
                    pilePool.Add(resourcePile);
                }
            });
            return pilePool;
        }

        #region tilling section
        private TileObject GetUntilledFarmTile() {
            for (int x = 0; x < farmTile.Count; ++x) {
                if (!CheckIfTileIsTilled(farmTile[x].tileObjectComponent.genericTileObject)) {
                    return farmTile[x].tileObjectComponent.genericTileObject;
                }
            }
            return null;
        }

        private bool CheckIfTileIsTilled(TileObject p_targetTile) {
            if (p_targetTile.gridTileLocation.tileObjectComponent.objHere == null) {
                return false;
            }
            return (p_targetTile.gridTileLocation.tileObjectComponent.objHere.tileObjectType == TILE_OBJECT_TYPE.CORN_CROP ||
                p_targetTile.gridTileLocation.tileObjectComponent.objHere.tileObjectType == TILE_OBJECT_TYPE.HYPNO_HERB_CROP ||
                p_targetTile.gridTileLocation.tileObjectComponent.objHere.tileObjectType == TILE_OBJECT_TYPE.ICEBERRY_CROP ||
                p_targetTile.gridTileLocation.tileObjectComponent.objHere.tileObjectType == TILE_OBJECT_TYPE.PINEAPPLE_CROP ||
                p_targetTile.gridTileLocation.tileObjectComponent.objHere.tileObjectType == TILE_OBJECT_TYPE.POTATO_CROP);
        }
        #endregion tilling section

        #region harvesting section
        private TileObject GetHarvestableCrop() {
            for (int x = 0; x < farmTile.Count; ++x) {
                if (CheckIfTileHasHarvestableCrop(farmTile[x].tileObjectComponent.genericTileObject)) {
                    return farmTile[x].tileObjectComponent.objHere;
                }
            }
            return null;
        }
        private bool CheckIfTileHasHarvestableCrop(TileObject p_targetTile) {
            if (p_targetTile.gridTileLocation.tileObjectComponent.objHere == null) {
                return false;
            }
            return (p_targetTile.gridTileLocation.tileObjectComponent.objHere.tileObjectType == TILE_OBJECT_TYPE.CORN ||
                p_targetTile.gridTileLocation.tileObjectComponent.objHere.tileObjectType == TILE_OBJECT_TYPE.HYPNO_HERB ||
                p_targetTile.gridTileLocation.tileObjectComponent.objHere.tileObjectType == TILE_OBJECT_TYPE.ICEBERRY ||
                p_targetTile.gridTileLocation.tileObjectComponent.objHere.tileObjectType == TILE_OBJECT_TYPE.PINEAPPLE ||
                p_targetTile.gridTileLocation.tileObjectComponent.objHere.tileObjectType == TILE_OBJECT_TYPE.POTATO);
        }
		#endregion

		protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob) {
            producedJob = null;
            if (p_worker.currentSettlement.SettlementResources.GetRandomPileOfCrops() != null) {
                //do haul job
            } else if (GetUntilledFarmTile() != null) {
                //do till farm tile
            } else if (CheckForMultipleSameResourcePileInsideStructure() != null) {
                //do combine resourcepiles job
            } else if (GetHarvestableCrop() != null) {
                //do harvest crops
            }
        }
    }
}
