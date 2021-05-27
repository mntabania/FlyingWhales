using UnityEngine;
using System.Collections.Generic;
namespace Inner_Maps.Location_Structures {
    public class Lumberyard : ManMadeStructure {
        public override Vector3 worldPosition => structureObj.transform.position;
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

        TileObject GetrandomTree() {
            TileObject tree = null;
            return tree;
        }

        protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob) {
            producedJob = null;
            if (p_worker.currentSettlement.SettlementResources.GetRandomPileOfWoods() != null) {
                //do haul job
            } else if (CheckForMultipleSameResourcePileInsideStructure() != null) {
                //do combine resourcepiles job
            } else if(GetrandomTree() != null){
                //do chop wood job
            }
        }
    }
}