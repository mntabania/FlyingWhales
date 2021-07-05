using Inner_Maps.Location_Structures;
using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
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
            Messenger.AddListener<Character, EquipmentItem>(CharacterSignals.CHARACTER_EQUIPPED_ITEM, OnCharacterEquippedItem);
        }
        protected override void UnsubscribeListeners() {
            base.UnsubscribeListeners();
            Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
            Messenger.RemoveListener<Character, EquipmentItem>(CharacterSignals.CHARACTER_EQUIPPED_ITEM, OnCharacterEquippedItem);
        }
        private void OnCharacterEquippedItem(Character p_character, EquipmentItem p_equipment) {
            if (IsCharacterAlreadyHasRequest(p_character)) {
                EQUIPMENT_TYPE equipmentType = EQUIPMENT_TYPE.WEAPON;
                if (p_equipment is WeaponItem) {
                    equipmentType = EQUIPMENT_TYPE.WEAPON;
                } else if (p_equipment is ArmorItem) {
                    equipmentType = EQUIPMENT_TYPE.ARMOR;
                } else if (p_equipment is AccessoryItem) {
                    equipmentType = EQUIPMENT_TYPE.ACCESSORY;
                }
                RemoveAllRequestFromCharacter(p_character, equipmentType);
            }
        }
        private void OnCharacterDied(Character p_Character) {
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
        private void RemoveAllRequestFromCharacter(Character p_character, EQUIPMENT_TYPE equipmentType) {
            List<WorkShopRequestForm> allCurrentRequests = RuinarchListPool<WorkShopRequestForm>.Claim();
            allCurrentRequests.AddRange(requests);
            
            for (int i = 0; i < allCurrentRequests.Count; i++) {
                WorkShopRequestForm requestForm = allCurrentRequests[i];
                if (requestForm.requestingCharacter == p_character && requestForm.equipmentType == equipmentType) {
                    requests.Remove(requestForm);
                }
            }
            RuinarchListPool<WorkShopRequestForm>.Release(allCurrentRequests);
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
            // return;
            GetReferenceForAllMetals();
            GetReferenceForStones();
            GetReferenceForWood();
            GetReferenceForCloths();
            GetReferenceForLeathers();

            //craft part
            //Debug.LogError("Try Craft: ");
            TILE_OBJECT_TYPE equipToMake = GetEquipmentToMakeFromRequestList();
            if (equipToMake != TILE_OBJECT_TYPE.NONE) {
                //do craft action/job
                p_worker.jobComponent.CreateEquipment(equipToMake, this, out producedJob);
                if (producedJob != null) {
                    //Debug.LogError("Try Craft ACCepted: " + equipToMake + " " + p_worker.name);
                    return;
                }
            }

            //Combine Part
            //try check if there are same metal resourcepiles of same type object(i.e. same copper, same iron etc.), then combine it
            //Debug.LogError("Try Search for metal resourcepile of same type: ");
            List<TileObject> pilesForCombining = RuinarchListPool<TileObject>.Claim();
            pilesForCombining = CreateListForHaulCombineCandidate(m_metals);
            if (pilesForCombining.Count > 1) {
                p_worker.jobComponent.TryCreateCombineStockpile(pilesForCombining[0] as ResourcePile, pilesForCombining[1] as ResourcePile, out producedJob);
                if (producedJob != null) {
                    // Debug.LogError("Try Search for metal resourcepile of same type Accepted: " + pilesForCombining[0].tileObjectType);
                    RuinarchListPool<TileObject>.Release(pilesForCombining);
                    return;
                }
            }

            //try check if there are same stone resourcepiles of same type object, then combine it
            //Debug.LogError("Try Search for m_stones resourcepile of same type: ");
            pilesForCombining = CreateListForHaulCombineCandidate(m_stones);
            if (pilesForCombining.Count > 1) {
                p_worker.jobComponent.TryCreateCombineStockpile(pilesForCombining[0] as ResourcePile, pilesForCombining[1] as ResourcePile, out producedJob);
                if (producedJob != null) {
                    //Debug.LogError("Try Search for m_stones resourcepile of same type Accepted: " + pilesForCombining[0].tileObjectType);
                    RuinarchListPool<TileObject>.Release(pilesForCombining);
                    return;
                }
            }

            //try check if there are same woods resourcepiles of same type object, then combine it
            //Debug.LogError("Try Search for m_woods resourcepile of same type: ");
            pilesForCombining = CreateListForHaulCombineCandidate(m_woods);
            if (pilesForCombining.Count > 1) {
                p_worker.jobComponent.TryCreateCombineStockpile(pilesForCombining[0] as ResourcePile, pilesForCombining[1] as ResourcePile, out producedJob);
                if (producedJob != null) {
                    //Debug.LogError("Try Search for m_woods resourcepile of same type Accepted: " + pilesForCombining[0].tileObjectType);
                    RuinarchListPool<TileObject>.Release(pilesForCombining);
                    return;
                }
            }

            //try check if there are same cloth resourcepiles of same type object, then combine it
            //Debug.LogError("Try Search for m_cloth resourcepile of same type: ");
            pilesForCombining = CreateListForHaulCombineCandidate(m_cloth);
            if (pilesForCombining.Count > 1) {
                p_worker.jobComponent.TryCreateCombineStockpile(pilesForCombining[0] as ResourcePile, pilesForCombining[1] as ResourcePile, out producedJob);
                if (producedJob != null) {
                    //Debug.LogError("Try Search for m_cloth resourcepile of same type Accepted: " + pilesForCombining[0].tileObjectType);
                    RuinarchListPool<TileObject>.Release(pilesForCombining);
                    return;
                }
            }

            //try check if there are same m_leather resourcepiles of same type object, then combine it
            //Debug.LogError("Try Search for m_cloth resourcepile of same type: "); 
            pilesForCombining = CreateListForHaulCombineCandidate(m_leather);
            if (pilesForCombining.Count > 1) {
                p_worker.jobComponent.TryCreateCombineStockpile(pilesForCombining[0] as ResourcePile, pilesForCombining[1] as ResourcePile, out producedJob);
                if (producedJob != null) {
                    //Debug.LogError("Try Search for m_leather resourcepile of same type Accepted: " + pilesForCombining[0].tileObjectType);
                    RuinarchListPool<TileObject>.Release(pilesForCombining);
                    return;
                }
            }
            RuinarchListPool<TileObject>.Release(pilesForCombining);

            //haul part
            bool ignoreHaul = ShouldIgnoreHaul(m_metals);

            //Debug.LogError("Try haul m_metals: " + m_metals.Count);
            if (!ignoreHaul) {
                ResourcePile metalPile = p_worker.homeSettlement.SettlementResources.GetRandomPileOfResourceTypeForWorkshopHaul(RESOURCE.METAL, STRUCTURE_TYPE.MINE, p_worker.homeSettlement);
                if (metalPile != null) {
                    p_worker.jobComponent.TryCreateHaulJobForCrafter(metalPile, out producedJob, 40);
                    if(producedJob != null) {
                        //Debug.LogError("Try haul metal Accepted: " + metalPile + " " + p_worker.name);
                        return;
					}
                }
            }

            //Debug.LogError("Try haul m_stones: " + m_stones.Count);
            ignoreHaul = ShouldIgnoreHaul(m_stones);
            if (!ignoreHaul) {
                ResourcePile stonePile = p_worker.homeSettlement.SettlementResources.GetRandomPileOfResourceTypeForWorkshopHaul(RESOURCE.STONE, STRUCTURE_TYPE.MINE, p_worker.homeSettlement);
                if (stonePile != null) {
                    p_worker.jobComponent.TryCreateHaulJobForCrafter(stonePile, out producedJob, 40);
                    if (producedJob != null) {
                        //Debug.LogError("Try haul m_stones Accepted: " + stonePile + " " + p_worker.name);
                        return;
                    }
                }
            }

            ignoreHaul = ShouldIgnoreHaul(m_cloth);
            //Debug.LogError("Try haul m_cloth: " + m_cloth.Count);
            if (!ignoreHaul) {
                ResourcePile clothPile = p_worker.homeSettlement.SettlementResources.GetRandomPileOfResourceTypeForWorkshopHaul(RESOURCE.CLOTH, STRUCTURE_TYPE.HUNTER_LODGE, p_worker.homeSettlement);
                if (clothPile != null) {
                    p_worker.jobComponent.TryCreateHaulJobForCrafter(clothPile, out producedJob, 40);
                    if (producedJob != null) {
                        //Debug.LogError("Try haul m_cloth Accepted: " + clothPile + " " + p_worker.name);
                        return;
                    }
                }
            }

            //Debug.LogError("Try haul m_leather: " + m_leather.Count);
            ignoreHaul = ShouldIgnoreHaul(m_leather);
            if (!ignoreHaul) {
                ResourcePile leatherPile = p_worker.homeSettlement.SettlementResources.GetRandomPileOfResourceTypeForWorkshopHaul(RESOURCE.LEATHER, STRUCTURE_TYPE.HUNTER_LODGE, p_worker.homeSettlement);
                if (leatherPile != null) {
                    p_worker.jobComponent.TryCreateHaulJobForCrafter(leatherPile, out producedJob, 40);
                    if (producedJob != null) {
                        //Debug.LogError("Try haul m_leather Accepted: " + leatherPile + " " + p_worker.name);
                        return;
                    }
                }
            }

            //Debug.LogError("Try haul m_woods: " + m_woods.Count);
            ignoreHaul = ShouldIgnoreHaul(m_woods);
            if (!ignoreHaul) {
                ResourcePile woodPile = p_worker.homeSettlement.SettlementResources.GetRandomPileOfResourceTypeForWorkshopHaul(RESOURCE.WOOD, STRUCTURE_TYPE.LUMBERYARD, p_worker.homeSettlement);
                if (woodPile != null) {
                    p_worker.jobComponent.TryCreateHaulJobForCrafter(woodPile, out producedJob, 40);
                    if (producedJob != null) {
                        //Debug.LogError("Try haul m_woods Accepted: " + woodPile + " " + p_worker.name);
                        return;
                    }
                }
            }
            
            if(TryCreateCleanJob(p_worker, out producedJob)) { return; }
        }

        public bool ShouldIgnoreHaul(List<TileObject> p_list) {
            for (int x = 0; x < p_list.Count; ++x) {
                if ((p_list[x] as ResourcePile).resourceInPile >= 40) {
                    return true;
                }
            }
            return false;
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

        List<TileObject> CreateListForHaulCombineCandidate(List<TileObject> p_list) {
            List<TileObject> candidates = new List<TileObject>();
            for (int x = 0; x < p_list.Count; ++x) { 
                for(int y = 0; y < p_list.Count; ++y) {
                    if (x == y) {
                        continue;
                    }
                    if (p_list[x].tileObjectType == p_list[y].tileObjectType) {
                        if (!candidates.Contains(p_list[x])) {
                            candidates.Add(p_list[x]);
                        }
                        if (!candidates.Contains(p_list[y])) {
                            candidates.Add(p_list[y]);
                        }
                        return candidates;
                    }
				}
            }
            return candidates;
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
        
        #region Worker
        public override bool CanHireAWorker() {
            return !HasAssignedWorker();
        }
        #endregion
        
        #region Purchasing
        public override bool CanPurchaseFromHere(Character p_buyer, out bool needsToPay, out int buyerOpinionOfWorker) {
            return DefaultCanPurchaseFromHereForSingleWorkerStructures(p_buyer, out needsToPay, out buyerOpinionOfWorker);
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