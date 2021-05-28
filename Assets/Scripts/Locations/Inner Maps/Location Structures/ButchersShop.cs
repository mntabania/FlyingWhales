using UnityEngine;
using System.Collections.Generic;
namespace Inner_Maps.Location_Structures {
    public class ButchersShop : ManMadeStructure {
        public override Vector3 worldPosition => structureObj.transform.position;

        public ButchersShop(Region location) : base(STRUCTURE_TYPE.BUTCHERS_SHOP, location) {
            SetMaxHPAndReset(4000);
        }
        public ButchersShop(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(4000);
        }

        List<ResourcePile> CheckForMultipleSameResourcePileInsideStructure() {
            return DoesMultipleResourcePileExist(GetAllMeatResourcePileOnTiles());
        }

        List<ResourcePile> DoesMultipleResourcePileExist(List<ResourcePile> p_allPiles) {
            List<ResourcePile> animalMeat = new List<ResourcePile>();
            List<ResourcePile> humanMeat = new List<ResourcePile>();
            List<ResourcePile> elfMeat = new List<ResourcePile>();
            p_allPiles.ForEach((eachList) => {
                switch (eachList.tileObjectType) {
                    case TILE_OBJECT_TYPE.ANIMAL_MEAT:
                    animalMeat.Add(eachList);
                    break;
                    case TILE_OBJECT_TYPE.HUMAN_MEAT:
                    humanMeat.Add(eachList);
                    break;
                    case TILE_OBJECT_TYPE.ELF_MEAT:
                    elfMeat.Add(eachList);
                    break;
                }
            });
            if (animalMeat.Count > 1) {
                return animalMeat;
            }
            if (humanMeat.Count > 1) {
                return animalMeat;
            }
            if (elfMeat.Count > 1) {
                return animalMeat;
            }
            return null;
        }

        List<ResourcePile> GetAllMeatResourcePileOnTiles() {
            List<ResourcePile> pilePool = new List<ResourcePile>();
            passableTiles.ForEach((eachTile) => {
                if (eachTile.tileObjectComponent.objHere != null && eachTile.tileObjectComponent.objHere is ResourcePile resourcePile) {
                    pilePool.Add(resourcePile);
                }
            });
            return pilePool;
        }

        Character GetTargetAnimal() {
            Character targetAnimalToButcher = null;
            return targetAnimalToButcher;
        }

        protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob) {
            producedJob = null;
            if (p_worker.currentSettlement.SettlementResources.GetRandomPileOfMeats() != null) {
                //do haul job
            } else if (CheckForMultipleSameResourcePileInsideStructure() != null) {
                //do combine resourcepiles job
            } else if(GetTargetAnimal() != null){
                //do butcher
            }
        }
    }
}