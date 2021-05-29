using System.Collections.Generic;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class HunterLodge : ManMadeStructure {

        List<TileObject> builtPilesInSideStructure = RuinarchListPool<TileObject>.Claim();
        public HunterLodge(Region location) : base(STRUCTURE_TYPE.HUNTER_LODGE, location) {
            SetMaxHPAndReset(8000);
        }
        public HunterLodge(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(8000);
        }

        List<TileObject> CreateClothAndLeatherList(TILE_OBJECT_TYPE p_type) {
            List<TileObject> createdList = RuinarchListPool<TileObject>.Claim();
            List<TileObject> unsortedList = GetTileObjectsOfType(p_type);
            if (unsortedList != null) {
                unsortedList.ForEach((eachList) => {
                    if (eachList.mapObjectState == MAP_OBJECT_STATE.BUILT && !((eachList as TileObject).HasJobTargetingThis(JOB_TYPE.HAUL))) {
                        createdList.Add(eachList);
                    }
                });
            }
            return createdList;
        }

        void SetListToVariable() {
            builtPilesInSideStructure = CreateClothAndLeatherList(TILE_OBJECT_TYPE.MINK_CLOTH);
            if (builtPilesInSideStructure != null && builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure = CreateClothAndLeatherList(TILE_OBJECT_TYPE.MOONCRAWLER_CLOTH);
            if (builtPilesInSideStructure != null && builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure = CreateClothAndLeatherList(TILE_OBJECT_TYPE.RABBIT_CLOTH);
            if (builtPilesInSideStructure != null && builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure = CreateClothAndLeatherList(TILE_OBJECT_TYPE.BEAR_HIDE);
            if (builtPilesInSideStructure != null && builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure = CreateClothAndLeatherList(TILE_OBJECT_TYPE.BOAR_HIDE);
            if (builtPilesInSideStructure != null && builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure = CreateClothAndLeatherList(TILE_OBJECT_TYPE.DRAGON_HIDE);
            if (builtPilesInSideStructure != null && builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure = CreateClothAndLeatherList(TILE_OBJECT_TYPE.SCALE_HIDE);
            if (builtPilesInSideStructure != null && builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure = CreateClothAndLeatherList(TILE_OBJECT_TYPE.WOLF_HIDE);
            if (builtPilesInSideStructure != null && builtPilesInSideStructure.Count > 1) {
                return;
            }
        }
        protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob) {
            producedJob = null;
            ResourcePile pile = p_worker.currentSettlement.SettlementResources.GetRandomPileOfClothOrLeather();
            if (pile != null) {
                p_worker.jobComponent.TryCreateHaulJob(pile, out producedJob);
                if (producedJob != null) {
                    return;
				}
            }
            SetListToVariable();
            if (builtPilesInSideStructure != null && builtPilesInSideStructure.Count > 1) {
                p_worker.jobComponent.TryCreateCombineStockpile(builtPilesInSideStructure[0] as ResourcePile, builtPilesInSideStructure[1] as ResourcePile, out producedJob);
                if (producedJob != null) {
                    return;
                }
            }
            List<Summon> targetAnimals = RuinarchListPool<Summon>.Claim();
            if (!p_worker.faction.factionType.IsActionConsideredACrime(CRIME_TYPE.Animal_Killing)) {
                targetAnimals = p_worker.currentSettlement.SettlementResources.GetAllAnimalsThatProducesMats();
            } else {
                targetAnimals = p_worker.currentSettlement.SettlementResources.GetAllAnimalsThatAreShearable();
            }
            Summon randomTarget = targetAnimals[GameUtilities.RandomBetweenTwoNumbers(0, targetAnimals.Count - 1)];
            if (randomTarget != null) {
                if (randomTarget is Animal) {
                    p_worker.jobComponent.TriggerShearAnimal(randomTarget, out producedJob);
                } else {
                    p_worker.jobComponent.TriggerSkinAnimal(randomTarget, out producedJob);
                }
                
                if (producedJob != null) {
                    return;
                }
            }
            RuinarchListPool<Summon>.Release(targetAnimals);
        }
    }
}