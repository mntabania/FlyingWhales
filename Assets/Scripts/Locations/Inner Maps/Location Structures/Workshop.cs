using System;
using UnityEngine;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
namespace Inner_Maps.Location_Structures {
    public class Workshop : ManMadeStructure {

        public List<WorkShopRequestForm> requests = new List<WorkShopRequestForm>();
        
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

        #region getters
        public override Type serializedData => typeof(SaveDataWorkshop);
        #endregion
        
        public Workshop(Region location) : base(STRUCTURE_TYPE.WORKSHOP, location) {
            
            SetMaxHPAndReset(8000);
        }
        public Workshop(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(8000);
        }

        #region Loading
        public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure) {
            base.LoadReferences(saveDataLocationStructure);
            SaveDataWorkshop saveDataWorkshop = saveDataLocationStructure as SaveDataWorkshop;
            if (saveDataWorkshop.requestForms != null) {
                for (int i = 0; i < saveDataWorkshop.requestForms.Length; i++) {
                    SaveDataWorkShopRequestForm saveDataWorkShopRequestForm = saveDataWorkshop.requestForms[i];
                    WorkShopRequestForm loaded = saveDataWorkShopRequestForm.Load();
                    Character requestingCharacter = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveDataWorkShopRequestForm.requestingCharacterID);
                    if (requestingCharacter != null) {
                        loaded.requestingCharacter = requestingCharacter;
                    }
                    requests.Add(loaded);
                }    
            }
            
        }
        #endregion

        protected override void SubscribeListeners() {
            base.SubscribeListeners();
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        }
        protected override void UnsubscribeListeners() {
            base.UnsubscribeListeners();
            Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
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
            
        }
        protected override void AfterStructureDestruction(Character p_responsibleCharacter = null) {
            base.AfterStructureDestruction(p_responsibleCharacter);
            requests.Clear();
        }
        public void RemoveFirstRequestThatIsFulfilledBy(TileObject p_object) {
            for (int i = 0; i < requests.Count; i++) {
                WorkShopRequestForm requestForm = requests[i];
                if (p_object is WeaponItem && requestForm.equipmentType == EQUIPMENT_TYPE.WEAPON) {
                    CharacterClassData characterClassData = CharacterManager.Instance.GetOrCreateCharacterClassData(requestForm.requestingCharacter.characterClass.className);
                    if (characterClassData.craftableWeapons.Contains(p_object.tileObjectType)) {
#if DEBUG_LOG
                        Debug.Log($"Removed request {requestForm} because of {p_object.nameWithID}");
#endif
                        requests.RemoveAt(i);
                        break;
                    }
                } else if (p_object is ArmorItem && requestForm.equipmentType == EQUIPMENT_TYPE.ARMOR) {
                    CharacterClassData characterClassData = CharacterManager.Instance.GetOrCreateCharacterClassData(requestForm.requestingCharacter.characterClass.className);
                    if (characterClassData.craftableArmors.Contains(p_object.tileObjectType)) {
#if DEBUG_LOG
                        Debug.Log($"Removed request {requestForm} because of {p_object.nameWithID}");
#endif
                        requests.RemoveAt(i);
                        break;
                    }
                } else if (p_object is AccessoryItem && requestForm.equipmentType == EQUIPMENT_TYPE.ACCESSORY) {
                    CharacterClassData characterClassData = CharacterManager.Instance.GetOrCreateCharacterClassData(requestForm.requestingCharacter.characterClass.className);
                    if (characterClassData.craftableAccessories.Contains(p_object.tileObjectType)) {
#if DEBUG_LOG
                        Debug.Log($"Removed request {requestForm} because of {p_object.nameWithID}");
#endif
                        requests.RemoveAt(i);
                        break;
                    }
                }
            }
        }
        TILE_OBJECT_TYPE GetEquipmentToMakeFromRequestList() {
            TILE_OBJECT_TYPE availEquipment = TILE_OBJECT_TYPE.NONE;
            for(int x = 0; x < requests.Count; ++x) {
                WorkShopRequestForm request = requests[x];
                if (request.requestingCharacter != null && !request.requestingCharacter.isDead) {
                    CharacterClassData cData = CharacterManager.Instance.GetOrCreateCharacterClassData(request.requestingCharacter.characterClass.className);
                    List<TILE_OBJECT_TYPE> list = null;
                    //NOTE: Craftable lists are ordered from worst to best, so it is safe to assume that the
                    //last equipment in the list is the best equipment of that type that this workshop can create
                    switch (request.equipmentType) {
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
                    if (list != null) {
                        //reverse traverse the list, since we want to evaluate the best equipment first.
                        for (int y = list.Count - 1; y >= 0; y--) {
                            TILE_OBJECT_TYPE equipmentType = list[y];
                            List<CONCRETE_RESOURCES> concreteResourcesNeeded = EquipmentDataHandler.Instance.GetResourcesNeeded(equipmentType);
                            RESOURCE generalResource = EquipmentDataHandler.Instance.GetGeneralResourcesNeeded(equipmentType);
                            int resourcesNeededAmount = EquipmentDataHandler.Instance.GetResourcesNeededAmount(equipmentType);
                            if (concreteResourcesNeeded != null && concreteResourcesNeeded.Count > 0) {
                                if (CanBeCrafted(concreteResourcesNeeded, resourcesNeededAmount, out _)) {
                                    availEquipment = equipmentType;
                                    break;
                                } else {
                                    request.isSubjectForRemoval = true;
                                }
                            } else if(generalResource != RESOURCE.NONE) {
                                if (CanBeCrafted(generalResource, resourcesNeededAmount)) {
                                    availEquipment = equipmentType;
                                    break;
                                } else {
                                    request.isSubjectForRemoval = true;
                                }
                            }
                        }
                    }
                }
                if (availEquipment != TILE_OBJECT_TYPE.NONE) {
                    //found an available equipment to craft
                    break;
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
                p_worker.jobComponent.CreateEquipment(equipToMake, this, out producedJob);
                if (producedJob != null) {
                    return;
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

        //this one is used for specific resource
        public bool CanBeCrafted(List<CONCRETE_RESOURCES> p_needs, int p_count, out ResourcePile foundResourcePile) {
            for(int x = 0; x < p_needs.Count; ++x) {
                List<TileObject> list = null;
                CONCRETE_RESOURCES neededResource = p_needs[x];
                switch (neededResource.GetResourceCategory()) {
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
                if (list != null) {
                    for (int y = 0; y < list.Count; ++y) {
                        TileObject tileObject = list[y];
                        if (tileObject is ResourcePile resourcePile && tileObject.tileObjectType == neededResource.ConvertResourcesToTileObjectType()) {
                            if (resourcePile.resourceInPile >= p_count) {
                                foundResourcePile = resourcePile;
                                return true;
                            }
                        }
                    }
                }
            }
            foundResourcePile = null;
            return false;
        }

        //this one is used for any type i.e any metals, any cloths, any leathers
        public bool CanBeCrafted(RESOURCE p_generalResource, int p_count) {
            List<TileObject> list = null;
            switch (p_generalResource) {
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
            if (list != null) {
                for (int y = 0; y < list.Count; ++y) {
                    TileObject tileObject = list[y];
                    if (tileObject is ResourcePile resourcePile && resourcePile.resourceInPile >= p_count) {
                        return true;
                    }
                }    
            }
            return false;
        }

        void GetReferenceForAllMetals() {
            m_metals.Clear();
            PopulateTileObjectsOfType<MetalPile>(m_metals);
        }


        void GetReferenceForStones() {
            m_stones.Clear();
            PopulateTileObjectsOfType(m_stones, TILE_OBJECT_TYPE.STONE_PILE);
        }

        void GetReferenceForWood() {
            m_woods.Clear();
            PopulateTileObjectsOfType(m_woods, TILE_OBJECT_TYPE.WOOD_PILE);
        }

        void GetReferenceForCloths() {
            m_cloth.Clear();
            PopulateTileObjectsOfType<ClothPile>(m_cloth);
        }

        void GetReferenceForLeathers() {
            m_leather.Clear();
            PopulateTileObjectsOfType<LeatherPile>(m_leather);
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

public class WorkShopRequestForm {
    public Character requestingCharacter;
    public EQUIPMENT_TYPE equipmentType;
    public bool isSubjectForRemoval;

    public WorkShopRequestForm(){}
    public WorkShopRequestForm(SaveDataWorkShopRequestForm p_data) {
        equipmentType = p_data.equipmentType;
        isSubjectForRemoval = p_data.isSubjectForRemoval;
    }
    
    public override string ToString() {
        return $"{requestingCharacter.name} - {equipmentType.ToString()}";
    }
}

#region Save Data
public class SaveDataWorkShopRequestForm : SaveData<WorkShopRequestForm> {
    public string requestingCharacterID;
    public EQUIPMENT_TYPE equipmentType;
    public bool isSubjectForRemoval;
    public override void Save(WorkShopRequestForm data) {
        base.Save(data);
        requestingCharacterID = data.requestingCharacter.persistentID;
        equipmentType = data.equipmentType;
        isSubjectForRemoval = data.isSubjectForRemoval;
    }
    public override WorkShopRequestForm Load() {
        return new WorkShopRequestForm(this);
    }
}

public class SaveDataWorkshop : SaveDataManMadeStructure {
    public SaveDataWorkShopRequestForm[] requestForms;
    public override void Save(LocationStructure locationStructure) {
        base.Save(locationStructure);
        Workshop workshop = locationStructure as Workshop;
        if (workshop.requests.Count > 0) {
            requestForms = new SaveDataWorkShopRequestForm[workshop.requests.Count];
            for (int i = 0; i < workshop.requests.Count; i++) {
                WorkShopRequestForm workShopRequestForm = workshop.requests[i];
                SaveDataWorkShopRequestForm saveData = new SaveDataWorkShopRequestForm();
                saveData.Save(workShopRequestForm);
                requestForms[i] = saveData;
            }    
        }
    }
}
#endregion