using UnityEngine;
using System.Collections.Generic;
namespace Inner_Maps.Location_Structures {
    public class Workshop : ManMadeStructure {

        public class WorkShopRequestForm {
            public Character requestingCharacter;
            public EQUIPMENT_TYPE equipmentType;
            public bool isSubjectForRemoval;
            public override string ToString() {
                return $"{requestingCharacter.name} - {equipmentType.ToString()}";
            }
        }

        public List<WorkShopRequestForm> requests = new List<WorkShopRequestForm>();

        private WorkShopRequestForm m_doneRequest;

        // public override Vector2 selectableSize { get; }
        public override Vector3 worldPosition {
            get {
                Vector3 pos = structureObj.transform.position;
                pos.x -= 0.5f;
                pos.y -= 0.5f;
                return pos;
            }
        }

        private List<TileObject> m_metals = new List<TileObject>();
        private List<TileObject> m_stones = new List<TileObject>();
        private List<TileObject> m_cloth = new List<TileObject>();
        private List<TileObject> m_leather = new List<TileObject>();
        private List<TileObject> m_woods = new List<TileObject>();
        public Workshop(Region location) : base(STRUCTURE_TYPE.WORKSHOP, location) {
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
            SetMaxHPAndReset(8000);
        }
        public Workshop(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(8000);
        }

        public void OnCharacterDied(Character p_Character) {
            RemoveAllRequestFromCharacter(p_Character);
        }

        public void PostRequest(WorkShopRequestForm p_requestForm) {
            requests.Add(p_requestForm);
        }

        public bool IsCharacterAlreadyHasRequest(Character p_requestor) {
            for(int x = 0; x < requests.Count; ++x) {
                if(requests[x].requestingCharacter == p_requestor) {
                    return true;
				}
			}
            return false;
		}

        public void RemoveAllRequestFromCharacter(Character p_character) {
            requests.RemoveAll(item => item.requestingCharacter == p_character);
        }

        void EvaluateRequests() {
            requests.RemoveAll(item => item.isSubjectForRemoval == true);
            if(m_doneRequest != null) {
                requests.Remove(m_doneRequest);
                m_doneRequest = null;
			}
        }

        TILE_OBJECT_TYPE GetEquipmentToMakeFromRequestList() {
            TILE_OBJECT_TYPE availEquipment = TILE_OBJECT_TYPE.NONE;
            for(int x = 0; x < requests.Count; ++x) {
                if (requests[x].requestingCharacter != null && !requests[x].requestingCharacter.isDead) {
                    CharacterClassData cData = CharacterManager.Instance.GetOrCreateCharacterClassData(requests[x].requestingCharacter.characterClass.className);
                    List<TILE_OBJECT_TYPE> list = new List<TILE_OBJECT_TYPE>();
                    switch (requests[x].equipmentType) {
                        case EQUIPMENT_TYPE.WEAPON:
                        list = cData.craftableWeapons;
                        break;
                        case EQUIPMENT_TYPE.ARMOR:
                        list = cData.craftableArmors;
                        break;
                        case EQUIPMENT_TYPE.ACCESSORY:
                        list = cData.craftableAccessories;
                        break;
                    }
                    for (int y = 0; y < list.Count; ++y) {
                        List<CONCRETE_RESOURCES> resourcesNeeded = EquipmentDataHandler.Instance.GetResourcesNeeded(list[y]);
                        if (CanBeCrafted(resourcesNeeded, EquipmentDataHandler.Instance.GetResourcesNeededAmount(list[y]))) {
                            m_doneRequest = requests[x];
                            availEquipment = list[y];
                            break;
                        } else {
                            requests[x].isSubjectForRemoval = true;
                        }
                    }
                }
			}
            EvaluateRequests();
            return availEquipment;
        }

        protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob) {
            producedJob = null;

            GetReferenceForAllMetals();
            GetReferenceForStones();
            GetReferenceForWood();
            GetReferenceForCloths();
            GetReferenceForLeathers();
            //craft part
            TILE_OBJECT_TYPE equipToMake = GetEquipmentToMakeFromRequestList();
            if(equipToMake != TILE_OBJECT_TYPE.NONE) {
                //do craft action/job
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
                List<TileObject> list = new List<TileObject>();
                switch (p_needs[x].GetResourceCategory()) {
                    case RESOURCE.METAL:
                    list = m_metals;
                    break;
                    case RESOURCE.STONE:
                    list = m_stones;
                    break;
                    case RESOURCE.WOOD:
                    list = m_woods;
                    break;
                    case RESOURCE.CLOTH:
                    list = m_cloth;
                    break;
                    case RESOURCE.LEATHER:
                    list = m_leather;
                    break;
                }
                for (int y = 0; y < list.Count; ++y) {
                    if (list[y].tileObjectType == p_needs[x].ConvertResourcesToTileObjectType()) {
                        if ((list[y] as ResourcePile).resourceInPile < p_count) {
                            return false;
                        }
                    }
                }
            }
            return true;
		}

        void GetReferenceForAllMetals() {
            m_metals.Clear();
            PopulateTileObjectsOfType<MetalPile>(m_metals);
            // m_metals = GetTileObjectsOfType(TILE_OBJECT_TYPE.COPPER);
            // m_metals.AddRange(GetTileObjectsOfType(TILE_OBJECT_TYPE.IRON));
            // m_metals.AddRange(GetTileObjectsOfType(TILE_OBJECT_TYPE.MITHRIL));
            // m_metals.AddRange(GetTileObjectsOfType(TILE_OBJECT_TYPE.ORICHALCUM));
        }


        void GetReferenceForStones() {
            m_stones.Clear();
            PopulateTileObjectsOfType(m_stones, TILE_OBJECT_TYPE.STONE_PILE);
            // m_stones = GetTileObjectsOfType(TILE_OBJECT_TYPE.STONE_PILE);
        }

        void GetReferenceForWood() {
            m_woods.Clear();
            PopulateTileObjectsOfType(m_woods, TILE_OBJECT_TYPE.WOOD_PILE);
            // m_woods = GetTileObjectsOfType(TILE_OBJECT_TYPE.WOOD_PILE);
        }

        void GetReferenceForCloths() {
            m_cloth.Clear();
            PopulateTileObjectsOfType<ClothPile>(m_cloth);
            // m_cloth = GetTileObjectsOfType(TILE_OBJECT_TYPE.MINK_CLOTH);
            // m_cloth.AddRange(GetTileObjectsOfType(TILE_OBJECT_TYPE.MOONCRAWLER_CLOTH));
            // m_cloth.AddRange(GetTileObjectsOfType(TILE_OBJECT_TYPE.RABBIT_CLOTH));
            // m_cloth.AddRange(GetTileObjectsOfType(TILE_OBJECT_TYPE.SPIDER_SILK));
            // m_cloth.AddRange(GetTileObjectsOfType(TILE_OBJECT_TYPE.WOOL));
        }

        void GetReferenceForLeathers() {
            m_leather.Clear();
            PopulateTileObjectsOfType<LeatherPile>(m_leather);
            // m_leather = GetTileObjectsOfType(TILE_OBJECT_TYPE.BOAR_HIDE);
            // m_leather.AddRange(GetTileObjectsOfType(TILE_OBJECT_TYPE.BEAR_HIDE));
            // m_leather.AddRange(GetTileObjectsOfType(TILE_OBJECT_TYPE.WOLF_HIDE));
            // m_leather.AddRange(GetTileObjectsOfType(TILE_OBJECT_TYPE.SCALE_ARMOR));
            // m_leather.AddRange(GetTileObjectsOfType(TILE_OBJECT_TYPE.SCALE_HIDE));
        }

        #region For Testing
        public override string GetTestingInfo() {
            string info = base.GetTestingInfo();
            info = $"{info}\nRequests: ";
            for (int i = 0; i < requests.Count; i++) {
                WorkShopRequestForm requestForm = requests[i];
                info = $"{info}\n{requestForm.ToString()}";
            }
            return info;
        }
        #endregion
    }
}