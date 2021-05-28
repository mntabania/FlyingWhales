using System.Collections.Generic;
namespace Inner_Maps.Location_Structures {
    public class HunterLodge : ManMadeStructure {
        public HunterLodge(Region location) : base(STRUCTURE_TYPE.HUNTER_LODGE, location) {
            SetMaxHPAndReset(8000);
        }
        public HunterLodge(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(8000);
        }

        List<ResourcePile> CheckForMultipleSameResourcePileInsideStructure() {
            return DoesMultipleResourcePileExist(GetAllClothLeatherResourcePileOnTiles());
        }

        List<ResourcePile> DoesMultipleResourcePileExist(List<ResourcePile> p_allPiles) {
            List<ResourcePile> minkClothPile = new List<ResourcePile>();
            List<ResourcePile> moonCrawlerClothPile = new List<ResourcePile>();
            List<ResourcePile> rabbitClothPile = new List<ResourcePile>();
            List<ResourcePile> bearHide = new List<ResourcePile>();
            List<ResourcePile> boarHide = new List<ResourcePile>();
            List<ResourcePile> dragonHide = new List<ResourcePile>();
            List<ResourcePile> scaleHide = new List<ResourcePile>();
            List<ResourcePile> wolfHide = new List<ResourcePile>();
            p_allPiles.ForEach((eachList) => {
                switch (eachList.tileObjectType) {
                    case TILE_OBJECT_TYPE.MINK_CLOTH:
                    minkClothPile.Add(eachList);
                    break;
                    case TILE_OBJECT_TYPE.MOONCRAWLER_CLOTH:
                    moonCrawlerClothPile.Add(eachList);
                    break;
                    case TILE_OBJECT_TYPE.RABBIT_CLOTH:
                    rabbitClothPile.Add(eachList);
                    break;
                    case TILE_OBJECT_TYPE.BOAR_HIDE:
                    boarHide.Add(eachList);
                    break;
                    case TILE_OBJECT_TYPE.BEAR_HIDE:
                    bearHide.Add(eachList);
                    break;
                    case TILE_OBJECT_TYPE.DRAGON_HIDE:
                    dragonHide.Add(eachList);
                    break;
                    case TILE_OBJECT_TYPE.SCALE_HIDE:
                    scaleHide.Add(eachList);
                    break;
                    case TILE_OBJECT_TYPE.WOLF_HIDE:
                    wolfHide.Add(eachList);
                    break;
                }
            });
            if (minkClothPile.Count > 1) {
                return minkClothPile;
            }
            if (moonCrawlerClothPile.Count > 1) {
                return minkClothPile;
            }
            if (rabbitClothPile.Count > 1) {
                return minkClothPile;
            }
            return null;
        }

        List<ResourcePile> GetAllClothLeatherResourcePileOnTiles() {
            List<ResourcePile> pilePool = new List<ResourcePile>();
            passableTiles.ForEach((eachTile) => {
                if (eachTile.tileObjectComponent.objHere != null && eachTile.tileObjectComponent.objHere is ResourcePile resourcePile) {
                    pilePool.Add(resourcePile);
                }
            });
            return pilePool;
        }

        Summon GetTargetAnimal() {
            Summon targetAnimal = null;
            return targetAnimal;
        }

        protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob) {
            producedJob = null;
            if (p_worker.currentSettlement.SettlementResources.GetRandomPileOfClothOrLeather() != null) {
                //do haul job
            } else if (CheckForMultipleSameResourcePileInsideStructure() != null) {
                //do combine resourcepiles job
            } else if (GetTargetAnimal() != null) {
                //do shear or skin
            }
        }
    }
}