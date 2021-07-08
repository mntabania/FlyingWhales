using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Components;
using Characters.Villager_Wants;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;

public class VillagerWantsComponent : CharacterComponent, CharacterEventDispatcher.IHomeStructureListener, 
    CharacterEventDispatcher.IEquipmentListener, CharacterEventDispatcher.IInventoryListener, 
    CharacterEventDispatcher.IFactionListener, CharacterEventDispatcher.ITraitListener {

    private readonly List<VillagerWant> _wantsToProcess;

    #region getters
    public List<VillagerWant> wantsToProcess => _wantsToProcess;
    #endregion
    
    public VillagerWantsComponent() {
        _wantsToProcess = new List<VillagerWant>();
    }
    public VillagerWantsComponent(SaveDataVillagerWantsComponent p_data) {
        _wantsToProcess = new List<VillagerWant>();
        for (int i = 0; i < p_data.wantsToProcess.Length; i++) {
            System.Type type = p_data.wantsToProcess[i];
            VillagerWant want = CharacterManager.Instance.GetVillagerWantInstance<VillagerWant>(type);
            _wantsToProcess.Add(want);
        }
    }

    #region Loading
    public void LoadReferences(SaveDataVillagerWantsComponent p_data, Character p_character) {
        SubscribeListeners(p_character);
    }
    #endregion

    #region Initialization
    public void Initialize(Character p_character) {
        SubscribeListeners(p_character);
        //toggle all other wants on, since we expect that the character will have none of the things that it wants at the start of the game
        EvaluateAllWants(p_character);
    }
    private void SubscribeListeners(Character p_character) {
        p_character.eventDispatcher.SubscribeToCharacterSetHomeStructure(this);
        p_character.eventDispatcher.SubscribeToObjectPlacedInCharactersDwelling(this);
        p_character.eventDispatcher.SubscribeToObjectRemovedFromCharactersDwelling(this);
        p_character.eventDispatcher.SubscribeToWeaponEvents(this);
        p_character.eventDispatcher.SubscribeToInventoryEvents(this);
        p_character.eventDispatcher.SubscribeToFactionEvents(this);
        p_character.eventDispatcher.SubscribeToCharacterGainedTrait(this);
        p_character.eventDispatcher.SubscribeToCharacterLostTrait(this);
    }
    #endregion
    
    #region Wants Management
    private void ToggleWantOn(VillagerWant p_want) {
        if (!wantsToProcess.Contains(p_want)) {
            //order new want based on priority.
            bool hasBeenInserted = false;
            if(wantsToProcess.Count > 0) {
                for (int i = 0; i < wantsToProcess.Count; i++) {
                    VillagerWant villagerWant = wantsToProcess[i];
                    if (p_want.priority > villagerWant.priority) {
                        wantsToProcess.Insert(i, p_want);
                        hasBeenInserted = true;
                        break;
                    }
                }
            }
            if (!hasBeenInserted) {
                wantsToProcess.Add(p_want);
            }
            p_want.OnWantToggledOn(owner);
#if DEBUG_LOG
            Debug.Log($"{GameManager.Instance.TodayLogString()}{owner.name} Added want: {p_want.GetType()}");
#endif
        }
    }
    private bool ToggleWantOff(VillagerWant p_want) {
        if (wantsToProcess.Remove(p_want)) {
            p_want.OnWantToggledOff(owner);
#if DEBUG_LOG
            Debug.Log($"{GameManager.Instance.TodayLogString()}{owner.name} Removed want: {p_want.GetType()}");
#endif
            return true;
        }
        return false;
    }
    private void EvaluateAllWants(Character p_character) {
        List<VillagerWant> allWants = CharacterManager.Instance.allWants.Values.ToList();
        for (int i = 0; i < allWants.Count; i++) {
            VillagerWant want = allWants[i];
            if (want.IsWantValid(p_character)) {
                ToggleWantOn(want);
            } else {
                ToggleWantOff(want);
            }
        }
    }
    #endregion

    #region Inquiry
    public VillagerWant GetTopPriorityWant(Character p_character, out LocationStructure p_chosenStructure) {
        for (int i = 0; i < wantsToProcess.Count; i++) {
            VillagerWant want = wantsToProcess[i];
            if (want.CanVillagerObtainWant(p_character, out p_chosenStructure)) {
                return want;
            }
        }
        p_chosenStructure = null;
        return null;
    }
    public bool IsWantToggledOn<T>() where T : VillagerWant {
        for (int i = 0; i < _wantsToProcess.Count; i++) {
            VillagerWant want = _wantsToProcess[i];
            if (want is T) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Dwelling
    public void OnCharacterSetHomeStructure(Character p_character, LocationStructure p_homeStructure) {
        Assert.IsTrue(p_character == owner);
        if (p_homeStructure is Dwelling || p_homeStructure is VampireCastle) {
            DwellingWant dwellingWant = CharacterManager.Instance.GetVillagerWantInstance<DwellingWant>();
            ToggleWantOff(dwellingWant);
            //check if other wants are still valid given new home.
            EvaluateAllWants(p_character);
        } else if (p_homeStructure is null) {
            DwellingWant dwellingWant = CharacterManager.Instance.GetVillagerWantInstance<DwellingWant>();
            ToggleWantOn(dwellingWant);
            
            //check if other wants are still valid given no home.
            EvaluateAllWants(p_character);
        }
    }
    public void OnObjectPlacedInHomeDwelling(Character p_character, LocationStructure p_homeStructure, TileObject p_placedObject) {
        Assert.IsTrue(p_character == owner);
        if (p_placedObject is FoodPile) {
            //toggle food want 1 off if a food pile is placed in the character's home.
            FoodWant_1 foodWant1 = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_1>();
            ToggleWantOff(foodWant1);
            Dwelling dwelling = p_homeStructure as Dwelling;
            if (dwelling != null) {
                if (dwelling.differentFoodPileKindsInDwelling >= 2) {
                    FoodWant_2 foodWant2 = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_2>();
                    ToggleWantOff(foodWant2);    
                }
                if (dwelling.differentFoodPileKindsInDwelling >= 3) {
                    FoodWant_3 foodWant3 = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_3>();
                    ToggleWantOff(foodWant3);    
                }    
            }
        } else if (p_placedObject is Table) {
            //toggle table want off if a table is placed or built in the character's home.
            TableWant tableWant = CharacterManager.Instance.GetVillagerWantInstance<TableWant>();
            ToggleWantOff(tableWant);
        } else if (p_placedObject is Bed) {
            //toggle bed want off if a table is placed or built in the character's home.
            BedWant bedWant = CharacterManager.Instance.GetVillagerWantInstance<BedWant>();
            ToggleWantOff(bedWant);
        } else if (p_placedObject is Torch) {
            //toggle home torch want off if a torch is placed or built in the character's home.
            HomeTorchWant bedWant = CharacterManager.Instance.GetVillagerWantInstance<HomeTorchWant>();
            ToggleWantOff(bedWant);
        } else if (p_placedObject is Guitar) {
            //toggle guitar want off if a guitar is placed or built in the character's home.
            GuitarWant guitarWant = CharacterManager.Instance.GetVillagerWantInstance<GuitarWant>();
            ToggleWantOff(guitarWant);
        }
    }
    public void OnObjectRemovedFromHomeDwelling(Character p_character, LocationStructure p_homeStructure, TileObject p_removedObject) {
        Assert.IsTrue(p_character == owner);
        
        if (p_removedObject is FoodPile) {
            if (!p_homeStructure.HasTileObjectThatIsBuiltFoodPile()) {
                //Food Pile is taken from home structure floor and there are no other Food Piles inside
                FoodWant_1 foodWant1 = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_1>();
                ToggleWantOn(foodWant1);    
            }
            Dwelling dwelling = p_homeStructure as Dwelling;
            if (dwelling != null) {
                if (dwelling.differentFoodPileKindsInDwelling < 2) {
                    FoodWant_2 foodWant2 = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_2>();
                    ToggleWantOn(foodWant2);
                }
                if (dwelling.differentFoodPileKindsInDwelling < 3) {
                    FoodWant_3 foodWant3 = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_3>();
                    ToggleWantOn(foodWant3);
                }    
            }
        } else if (p_removedObject is Table) {
            if (!p_homeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.TABLE)) {
                //Table is destroyed or taken from home structure floor and there are no other Tables inside
                TableWant tableWant = CharacterManager.Instance.GetVillagerWantInstance<TableWant>();
                ToggleWantOn(tableWant);    
            }
        } else if (p_removedObject is Bed) {
            if (!p_homeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.BED)) {
                //Bed is destroyed or taken from home structure floor and there are no other Beds inside
                BedWant bedWant = CharacterManager.Instance.GetVillagerWantInstance<BedWant>();
                ToggleWantOn(bedWant);    
            }
        } else if (p_removedObject is Torch) {
            if (!p_homeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.TORCH)) {
                //Torch is destroyed or taken from home structure floor and there are no other torches inside
                HomeTorchWant homeTorchWant = CharacterManager.Instance.GetVillagerWantInstance<HomeTorchWant>();
                ToggleWantOn(homeTorchWant);    
            }
        } else if (p_removedObject is Guitar) {
            if (!p_homeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.GUITAR)) {
                //Guitar is destroyed or taken from home structure floor and there are no other guitars inside
                GuitarWant guitarWant = CharacterManager.Instance.GetVillagerWantInstance<GuitarWant>();
                if (guitarWant.IsWantValid(p_character)) {
                    ToggleWantOn(guitarWant);    
                }
            }
        }
    }
    #endregion

    #region Equipment
    public void OnWeaponEquipped(Character p_character, EquipmentItem p_weapon) {
        Assert.IsTrue(p_character == owner);
        WeaponWant weaponWant = CharacterManager.Instance.GetVillagerWantInstance<WeaponWant>();
        ToggleWantOff(weaponWant);    
    }
    public void OnWeaponUnequipped(Character p_character, EquipmentItem p_weapon) {
        Assert.IsTrue(p_character == owner);
        WeaponWant weaponWant = CharacterManager.Instance.GetVillagerWantInstance<WeaponWant>();
        ToggleWantOn(weaponWant);
    }
    public void OnArmorEquipped(Character p_character, EquipmentItem p_weapon) {
        Assert.IsTrue(p_character == owner);
        ArmorWant armorWant = CharacterManager.Instance.GetVillagerWantInstance<ArmorWant>();
        ToggleWantOff(armorWant);
    }
    public void OnArmorUnequipped(Character p_character, EquipmentItem p_weapon) {
        Assert.IsTrue(p_character == owner);
        ArmorWant armorWant = CharacterManager.Instance.GetVillagerWantInstance<ArmorWant>();
        ToggleWantOn(armorWant);
    }
    public void OnAccessoryEquipped(Character p_character, EquipmentItem p_weapon) {
        Assert.IsTrue(p_character == owner);
        AccessoryWant armorWant = CharacterManager.Instance.GetVillagerWantInstance<AccessoryWant>();
        ToggleWantOff(armorWant);
    }
    public void OnAccessoryUnequipped(Character p_character, EquipmentItem p_weapon) {
        Assert.IsTrue(p_character == owner);
        AccessoryWant armorWant = CharacterManager.Instance.GetVillagerWantInstance<AccessoryWant>();
        ToggleWantOn(armorWant);
    }
    #endregion

    #region Inventory
    public void OnItemObtained(Character p_character, TileObject p_obtainedItem) {
        Assert.IsTrue(p_character == owner);
        if (p_obtainedItem is HealingPotion) {
            HealingPotionWant healingPotionWant = CharacterManager.Instance.GetVillagerWantInstance<HealingPotionWant>();
            ToggleWantOff(healingPotionWant);
        }
    }
    public void OnItemLost(Character p_character, TileObject p_lostItem) {
        Assert.IsTrue(p_character == owner);
        if (p_lostItem is HealingPotion) {
            if (!p_character.HasItem(TILE_OBJECT_TYPE.HEALING_POTION)) {
                HealingPotionWant healingPotionWant = CharacterManager.Instance.GetVillagerWantInstance<HealingPotionWant>();
                ToggleWantOn(healingPotionWant);    
            }
        }
    }
    #endregion

    #region Faction
    public void OnJoinFaction(Character p_character, Faction p_newFaction) {
        Assert.IsTrue(p_character == owner);
        EvaluateAllWants(p_character);
    }
    #endregion

    #region Traits
    public void OnCharacterGainedTrait(Character p_character, Trait p_gainedTrait) {
        Assert.IsTrue(p_character == owner);
        if (p_gainedTrait is MusicHater) {
            GuitarWant guitarWant = CharacterManager.Instance.GetVillagerWantInstance<GuitarWant>();
            ToggleWantOff(guitarWant);
        } else if (p_gainedTrait is Vampire) {
            FoodWant_1 foodWant1 = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_1>();
            ToggleWantOff(foodWant1);
            FoodWant_2 foodWant2 = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_2>();
            ToggleWantOff(foodWant2);
            FoodWant_3 foodWant3 = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_3>();
            ToggleWantOff(foodWant3);
        }
    }

    public void OnCharacterLostTrait(Character p_character, Trait p_lostTrait, Character p_removedBy) {
        Assert.IsTrue(p_character == owner);
        if (p_lostTrait is MusicHater) {
            GuitarWant guitarWant = CharacterManager.Instance.GetVillagerWantInstance<GuitarWant>();
            if (guitarWant.IsWantValid(p_character)) {
                ToggleWantOn(guitarWant);    
            }
        } else if (p_lostTrait is Vampire) {
            FoodWant_1 foodWant1 = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_1>();
            if (foodWant1.IsWantValid(p_character)) {
                ToggleWantOn(foodWant1);    
            }
            FoodWant_2 foodWant2 = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_2>();
            if (foodWant2.IsWantValid(p_character)) {
                ToggleWantOn(foodWant2);    
            }
            FoodWant_3 foodWant3 = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_3>();
            if (foodWant3.IsWantValid(p_character)) {
                ToggleWantOn(foodWant3);    
            }
        }
    }
    #endregion
}


#region Save Data
public class SaveDataVillagerWantsComponent : SaveData<VillagerWantsComponent> {
    public System.Type[] wantsToProcess;
    public override void Save(VillagerWantsComponent data) {
        base.Save(data);
        wantsToProcess = new Type[data.wantsToProcess.Count];
        for (int i = 0; i < data.wantsToProcess.Count; i++) {
            wantsToProcess[i] = data.wantsToProcess[i].GetType();
        }
    }
    public override VillagerWantsComponent Load() {
        return new VillagerWantsComponent(this);
    }
}
#endregion