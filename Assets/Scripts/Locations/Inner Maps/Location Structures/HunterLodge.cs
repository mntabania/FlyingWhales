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
            createdList = GetTileObjectsOfType(p_type);
            if (createdList != null) {
                createdList.ForEach((eachList) => {
                    if (eachList.mapObjectState == MAP_OBJECT_STATE.BUILT && !((eachList as TileObject).HasJobTargetingThis(JOB_TYPE.HAUL))) {
                        builtPilesInSideStructure.Add(eachList);
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

        Summon GetTargetAnimal() {
            Summon targetAnimal = null;
            return targetAnimal;
        }

        protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob) {
            producedJob = null;
            ResourcePile pile = p_worker.currentSettlement.SettlementResources.GetRandomPileOfClothOrLeather();
            if (pile != null) {
                p_worker.jobComponent.TryCreateHaulJob(pile, out producedJob);
                UnityEngine.Debug.LogError(p_worker.name + "HAUL IN HUNTER LODGE: ");
                if (producedJob != null) {
                    return;
				}
            }
            SetListToVariable();
            if (builtPilesInSideStructure != null && builtPilesInSideStructure.Count > 1) {
                UnityEngine.Debug.LogError(p_worker.name + "COMBINE IN HUNTER LODGE: ");
                p_worker.jobComponent.TryCreateCombineStockpile(builtPilesInSideStructure[0] as ResourcePile, builtPilesInSideStructure[1] as ResourcePile, out producedJob);
                if (producedJob != null) {
                    return;
                }
            }
            List<Summon> targetAnimals = RuinarchListPool<Summon>.Claim();
            targetAnimals = p_worker.currentSettlement.SettlementResources.GetAllAnimalsThatAreShearable();
            if (targetAnimals != null && targetAnimals.Count > 0) {
                UnityEngine.Debug.LogError(p_worker.name + "SHEARING: " + targetAnimals[0].name);
                p_worker.jobComponent.TriggerShearAnimal(targetAnimals[0], out producedJob);
                if (producedJob != null) {
                    return;
                }
            }
        }
    }
}