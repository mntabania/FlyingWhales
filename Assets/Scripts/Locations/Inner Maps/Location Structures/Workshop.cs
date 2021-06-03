using UnityEngine;
using System.Collections.Generic;
namespace Inner_Maps.Location_Structures {
    public class Workshop : ManMadeStructure {

        public List<TILE_OBJECT_TYPE> listOfEquipmentsToProduce = new List<TILE_OBJECT_TYPE>();
        public override Vector2 selectableSize { get; }
        public override Vector3 worldPosition => structureObj.transform.position;

        private List<TileObject> m_metals = new List<TileObject>();
        private List<TileObject> m_stones = new List<TileObject>();
        private List<TileObject> m_cloth = new List<TileObject>();
        private List<TileObject> m_leather = new List<TileObject>();
        private List<TileObject> m_woods = new List<TileObject>();
        public Workshop(Region location) : base(STRUCTURE_TYPE.WORKSHOP, location) {
            SetMaxHPAndReset(8000);
        }
        public Workshop(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(8000);
        }

        protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob) {
            producedJob = null;

            GetReferenceForAllMetals();
            GetReferenceForStones();
            GetReferenceForWood();
            GetReferenceForCloths();
            GetReferenceForLeathers();
            //craft part
            for (int x = 0; x < listOfEquipmentsToProduce.Count; ++x) {
                List<CONCRETE_RESOURCES> resourcesNeeded = EquipmentDataHandler.Instance.GetResourcesNeeded(listOfEquipmentsToProduce[x]);
                if (CanBeCrafted(resourcesNeeded, 40)) { 
                    //craft weapon
                }
            }

            //haul part
            if (m_metals.Count < 40) {
                ResourcePile metalPile = p_worker.homeSettlement.SettlementResources.GetRandomPileOfMetalForCraftsman(p_worker);
                if (metalPile != null) {
                    p_worker.jobComponent.TryCreateHaulJob(metalPile, out producedJob);
                    if(producedJob != null) {
                        return;
					}
                }
            }

            if (m_stones.Count < 40) {
                ResourcePile stonePile = p_worker.homeSettlement.SettlementResources.GetRandomPileOfStoneForCraftsman(p_worker);
                if (stonePile != null) {
                    p_worker.jobComponent.TryCreateHaulJob(stonePile, out producedJob);
                    if (producedJob != null) {
                        return;
                    }
                }
            }

            if (m_cloth.Count < 40) {
                ResourcePile clothPile = p_worker.homeSettlement.SettlementResources.GetRandomPileOfClothForCraftsman(p_worker);
                if (clothPile != null) {
                    p_worker.jobComponent.TryCreateHaulJob(clothPile, out producedJob);
                    if (producedJob != null) {
                        return;
                    }
                }
            }

            if (m_leather.Count < 40) {
                ResourcePile leatherPile = p_worker.homeSettlement.SettlementResources.GetRandomPileOfLeatherForCraftsman(p_worker);
                if (leatherPile != null) {
                    p_worker.jobComponent.TryCreateHaulJob(leatherPile, out producedJob);
                    if (producedJob != null) {
                        return;
                    }
                }
            }

            if (m_woods.Count < 40) {
                ResourcePile woodPile = p_worker.homeSettlement.SettlementResources.GetRandomPileOfWoodForCraftsman(p_worker);
                if (woodPile != null) {
                    p_worker.jobComponent.TryCreateHaulJob(woodPile, out producedJob);
                    if (producedJob != null) {
                        return;
                    }
                }
            }
        }

        bool CanBeCrafted(List<CONCRETE_RESOURCES> p_needs, int p_count) {
            for(int x = 0; x < p_needs.Count; ++x) {
                switch (p_needs[x].GetResourceCategory()) {
                    case RESOURCE.METAL:
                    for (int y = 0; y < m_metals.Count; ++y) {
                        if (m_metals[y].tileObjectType == p_needs[x].ConvertResourcesToTileObjectType()) { 
                            if((m_metals[y] as ResourcePile).resourceInPile < p_count) {
                                return false;
                            }
                        }
                    }
                    break;
                    case RESOURCE.STONE:
                    for (int y = 0; y < m_stones.Count; ++y) {
                        if (m_stones[y].tileObjectType == p_needs[x].ConvertResourcesToTileObjectType()) {
                            if ((m_stones[y] as ResourcePile).resourceInPile < p_count) {
                                return false;
                            }
                        }
                    }
                    break;
                    case RESOURCE.WOOD:
                    for (int y = 0; y < m_woods.Count; ++y) {
                        if (m_woods[y].tileObjectType == p_needs[x].ConvertResourcesToTileObjectType()) {
                            if ((m_woods[y] as ResourcePile).resourceInPile < p_count) {
                                return false;
                            }
                        }
                    }
                    break;
                    case RESOURCE.CLOTH:
                    for (int y = 0; y < m_cloth.Count; ++y) {
                        if (m_cloth[y].tileObjectType == p_needs[x].ConvertResourcesToTileObjectType()) {
                            if ((m_cloth[y] as ResourcePile).resourceInPile < p_count) {
                                return false;
                            }
                        }
                    }
                    break;
                    case RESOURCE.LEATHER:
                    for (int y = 0; y < m_leather.Count; ++y) {
                        if (m_leather[y].tileObjectType == p_needs[x].ConvertResourcesToTileObjectType()) {
                            if ((m_leather[y] as ResourcePile).resourceInPile < p_count) {
                                return false;
                            }
                        }
                    }
                    break;
                }
			}
            return true;
		}

        void GetReferenceForAllMetals() {
            m_metals.Clear();
            m_metals = GetTileObjectsOfType(TILE_OBJECT_TYPE.COPPER);
            m_metals.AddRange(GetTileObjectsOfType(TILE_OBJECT_TYPE.IRON));
            m_metals.AddRange(GetTileObjectsOfType(TILE_OBJECT_TYPE.MITHRIL));
            m_metals.AddRange(GetTileObjectsOfType(TILE_OBJECT_TYPE.ORICHALCUM));
        }


        void GetReferenceForStones() {
            m_stones.Clear();
            m_stones = GetTileObjectsOfType(TILE_OBJECT_TYPE.STONE_PILE);
        }

        void GetReferenceForWood() {
            m_woods.Clear();
            m_woods = GetTileObjectsOfType(TILE_OBJECT_TYPE.WOOD_PILE);
        }

        void GetReferenceForCloths() {
            m_cloth.Clear();
            m_cloth = GetTileObjectsOfType(TILE_OBJECT_TYPE.MINK_CLOTH);
            m_cloth.AddRange(GetTileObjectsOfType(TILE_OBJECT_TYPE.MOONCRAWLER_CLOTH));
            m_cloth.AddRange(GetTileObjectsOfType(TILE_OBJECT_TYPE.RABBIT_CLOTH));
            m_cloth.AddRange(GetTileObjectsOfType(TILE_OBJECT_TYPE.SPIDER_SILK));
            m_cloth.AddRange(GetTileObjectsOfType(TILE_OBJECT_TYPE.WOOL));
        }

        void GetReferenceForLeathers() {
            m_leather.Clear();
            m_leather = GetTileObjectsOfType(TILE_OBJECT_TYPE.BOAR_HIDE);
            m_leather.AddRange(GetTileObjectsOfType(TILE_OBJECT_TYPE.BEAR_HIDE));
            m_leather.AddRange(GetTileObjectsOfType(TILE_OBJECT_TYPE.WOLF_HIDE));
            m_leather.AddRange(GetTileObjectsOfType(TILE_OBJECT_TYPE.SCALE_ARMOR));
            m_leather.AddRange(GetTileObjectsOfType(TILE_OBJECT_TYPE.SCALE_HIDE));
        }
    }
}