using System.Collections.Generic;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class HunterLodge : ManMadeStructure {
        public HunterLodge(Region location) : base(STRUCTURE_TYPE.HUNTER_LODGE, location) {
            nameWithoutID = "Skinner's Lodge";
            name = $"{nameWithoutID} {id.ToString()}";
            SetMaxHPAndReset(8000);
        }
        public HunterLodge(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(8000);
        }

        private void PopulateClothAndLeatherList(List<TileObject> p_list, TILE_OBJECT_TYPE p_type) {
            List<TileObject> unsortedList = GetTileObjectsOfType(p_type);
            if (unsortedList != null) {
                for (int i = 0; i < unsortedList.Count; i++) {
                    TileObject t = unsortedList[i];
                    if (t.mapObjectState == MAP_OBJECT_STATE.BUILT && !t.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
                        p_list.Add(t);
                    }
                }
            }
        }

        private void SetListToVariable(List<TileObject> builtPilesInSideStructure) {
            PopulateClothAndLeatherList(builtPilesInSideStructure, TILE_OBJECT_TYPE.MINK_CLOTH);
            if (builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure.Clear();
            PopulateClothAndLeatherList(builtPilesInSideStructure, TILE_OBJECT_TYPE.MOONCRAWLER_CLOTH);
            if (builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure.Clear();
            PopulateClothAndLeatherList(builtPilesInSideStructure, TILE_OBJECT_TYPE.RABBIT_CLOTH);
            if (builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure.Clear();
            PopulateClothAndLeatherList(builtPilesInSideStructure, TILE_OBJECT_TYPE.BEAR_HIDE);
            if (builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure.Clear();
            PopulateClothAndLeatherList(builtPilesInSideStructure, TILE_OBJECT_TYPE.BOAR_HIDE);
            if (builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure.Clear();
            PopulateClothAndLeatherList(builtPilesInSideStructure, TILE_OBJECT_TYPE.DRAGON_HIDE);
            if (builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure.Clear();
            PopulateClothAndLeatherList(builtPilesInSideStructure, TILE_OBJECT_TYPE.SCALE_HIDE);
            if (builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure.Clear();
            PopulateClothAndLeatherList(builtPilesInSideStructure, TILE_OBJECT_TYPE.WOLF_HIDE);
            if (builtPilesInSideStructure.Count > 1) {
                return;
            }
        }
        protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob) {
            producedJob = null;
            ResourcePile pile = p_worker.homeSettlement.SettlementResources.GetRandomPileOfClothOrLeather();
            if (pile != null) {
                p_worker.jobComponent.TryCreateHaulJob(pile, out producedJob);
                if (producedJob != null) {
                    return;
				}
            }
            //Combine resource piles
            List<TileObject> builtPilesInSideStructure = RuinarchListPool<TileObject>.Claim();
            SetListToVariable(builtPilesInSideStructure);
            if (builtPilesInSideStructure.Count > 1) {
                p_worker.jobComponent.TryCreateCombineStockpile(builtPilesInSideStructure[0] as ResourcePile, builtPilesInSideStructure[1] as ResourcePile, out producedJob);
                if (producedJob != null) {
                    RuinarchListPool<TileObject>.Release(builtPilesInSideStructure);
                    return;
                }
            }
            RuinarchListPool<TileObject>.Release(builtPilesInSideStructure);

            Character randomTarget;
            List<Character> targetAnimals = RuinarchListPool<Character>.Claim();
            if (!p_worker.faction.factionType.IsActionConsideredACrime(CRIME_TYPE.Animal_Killing)) {
                p_worker.homeSettlement.SettlementResources.PopulateAllAnimalsThatProducesMats(targetAnimals);

                randomTarget = CollectionUtilities.GetRandomElement(targetAnimals);
                RuinarchListPool<Character>.Release(targetAnimals);
                if (randomTarget != null) {
                    p_worker.jobComponent.TriggerSkinAnimal(randomTarget, out producedJob);

                    if (producedJob != null) {
                        return;
                    }
                }

            }
            
            RuinarchListPool<Character>.Release(targetAnimals); p_worker.homeSettlement.SettlementResources.PopulateAllAnimalsThatAreShearable(targetAnimals);
            randomTarget = CollectionUtilities.GetRandomElement(targetAnimals); 
            if (randomTarget != null) {
                if (randomTarget is Animal) {
                    p_worker.jobComponent.TriggerShearAnimal(randomTarget, out producedJob);
                }
                if (producedJob != null) {
                    return;
                }
            }
        }
    }
}