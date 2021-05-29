using UnityEngine;
using System.Collections.Generic;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class ButchersShop : ManMadeStructure {

        List<TileObject> builtPilesInSideStructure = RuinarchListPool<TileObject>.Claim();
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
                if (eachTile.tileObjectComponent.objHere != null && eachTile.tileObjectComponent.objHere is FoodPile resourcePile) {
                    pilePool.Add(resourcePile);
                }
            });
            return pilePool;
        }

        protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob) {
            producedJob = null;
            ResourcePile foodPile = p_worker.currentSettlement.SettlementResources.GetRandomPileOfMeats();
            if (foodPile != null) {
                p_worker.jobComponent.TryCreateHaulJob(foodPile, out producedJob);
                if (producedJob != null) {
                    return;
                }
            }
            List<ResourcePile> multiplePiles = CheckForMultipleSameResourcePileInsideStructure();
            if (multiplePiles != null && multiplePiles.Count > 1) {
                p_worker.jobComponent.TryCreateCombineStockpile(multiplePiles[0], multiplePiles[1], out producedJob);
                if(producedJob != null) {
                    return;
				}
            }
            Summon targetForButchering = p_worker.currentSettlement.SettlementResources.GetRandomButcherableAnimal();
            if (targetForButchering != null){
                p_worker.jobComponent.CreateButcherJob(targetForButchering, JOB_TYPE.MONSTER_BUTCHER, out producedJob);
            }
        }
    }
}