using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UtilityScripts;

public class EquipmentComponent {

    public EquipmentItem currentWeapon { private set; get; }
    public EquipmentItem currentArmor { private set; get; }
    public EquipmentItem currentAccessory { private set; get; }

    public List<EquipmentItem> allEquipments = new List<EquipmentItem>();

    public EquipmentComponent() {
        Reset();
    }

    public EquipmentComponent(SaveDataEquipmentComponent p_copy) {
        currentWeapon = p_copy.currentWeapon;
        currentArmor = p_copy.currentArmor;
        currentAccessory = p_copy.currentAccessory;
    }

    void Reset() {
        currentWeapon = null;
        currentArmor = null;
        currentAccessory = null;
    }

    private void SetWeapon(EquipmentItem p_newWeapon, Character p_targetCharacter, bool p_initializedStackCountOnly = false) {
        //remove old weapon stats first
        if(currentWeapon != null) {
            allEquipments.Remove(currentWeapon);
            p_targetCharacter.UnobtainItemForEquipmentReplacement(currentWeapon);
        }
        currentWeapon = p_newWeapon;
        allEquipments.Add(p_newWeapon);
        //apply new weapon stats again
        EquipmentBonusProcessor.ApplyEquipBonusToTarget(currentWeapon, p_targetCharacter, p_initializedStackCountOnly);
        p_targetCharacter.eventDispatcher.ExecuteWeaponEquipped(p_targetCharacter, p_newWeapon);
        Messenger.Broadcast(CharacterSignals.CHARACTER_EQUIPPED_ITEM, p_targetCharacter, p_newWeapon);
    }

    private void SetArmor(EquipmentItem p_newArmor, Character p_targetCharacter, bool p_initializedStackCountOnly = false) {
        //remove old Armor stats first
        if(currentArmor != null) {
            allEquipments.Remove(currentArmor);
            p_targetCharacter.UnobtainItemForEquipmentReplacement(currentArmor);
        }
        currentArmor = p_newArmor;
        allEquipments.Add(p_newArmor);
        //apply new Armor stats again
        EquipmentBonusProcessor.ApplyEquipBonusToTarget(currentArmor, p_targetCharacter, p_initializedStackCountOnly);
        p_targetCharacter.eventDispatcher.ExecuteArmorEquipped(p_targetCharacter, p_newArmor);
        Messenger.Broadcast(CharacterSignals.CHARACTER_EQUIPPED_ITEM, p_targetCharacter, p_newArmor);
    }

    private void SetAccessory(EquipmentItem p_newAccessory, Character p_targetCharacter, bool p_initializedStackCountOnly = false) {
        //remove old Accessory stats first
        if(currentAccessory != null) {
            allEquipments.Remove(currentAccessory);
            p_targetCharacter.UnobtainItemForEquipmentReplacement(currentAccessory);
        }
        currentAccessory = p_newAccessory;
        allEquipments.Add(p_newAccessory);
        //apply new Accessory stats again
        EquipmentBonusProcessor.ApplyEquipBonusToTarget(currentAccessory, p_targetCharacter, p_initializedStackCountOnly);
        p_targetCharacter.eventDispatcher.ExecuteAccessoryEquipped(p_targetCharacter, p_newAccessory);
        Messenger.Broadcast(CharacterSignals.CHARACTER_EQUIPPED_ITEM, p_targetCharacter, p_newAccessory);
    }

    public void SetEquipment(EquipmentItem p_newItem, Character p_targetCharacter, bool p_initializedStackCountOnly = false) {
        if(p_newItem is WeaponItem) {
            SetWeapon(p_newItem, p_targetCharacter, p_initializedStackCountOnly);
		}
        if (p_newItem is ArmorItem) {
            SetArmor(p_newItem, p_targetCharacter, p_initializedStackCountOnly);
        }
        if (p_newItem is AccessoryItem) {
            SetAccessory(p_newItem, p_targetCharacter, p_initializedStackCountOnly);
        }

        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Equip Item", "equipped_item", null, LOG_TAG.Major);
        log.AddToFillers(p_targetCharacter, p_targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(p_newItem, p_newItem.name, LOG_IDENTIFIER.ITEM_1);
        log.AddLogToDatabase();
    }

    public void RemoveEquipment(EquipmentItem p_removedItem, Character p_targetCharacter) {
        if (p_removedItem is WeaponItem) {
            currentWeapon = null;
        }
        if (p_removedItem is ArmorItem) {
            currentArmor = null;
        }
        if (p_removedItem is AccessoryItem) {
            currentAccessory = null;
        }
        EquipmentBonusProcessor.RemoveEquipBonusToTarget(p_removedItem, p_targetCharacter);
        allEquipments.Remove(p_removedItem);
        if (p_removedItem is WeaponItem) {
            Messenger.Broadcast(CharacterSignals.WEAPON_UNEQUIPPED, p_targetCharacter, p_removedItem);
        }
        if (p_removedItem is ArmorItem) {
            Messenger.Broadcast(CharacterSignals.ARMOR_UNEQUIPPED, p_targetCharacter, p_removedItem);
        }
        if (p_removedItem is AccessoryItem) {
            Messenger.Broadcast(CharacterSignals.ACCESSORY_UNEQUIPPED, p_targetCharacter, p_removedItem);
        }
    }

    public EquipmentItem GetRandomRemainingEquipment(EquipmentItem p_equipToBeremoved) {
        List<EquipmentItem> randomList = new List<EquipmentItem>();
        if(currentWeapon != null) {
            if (!(p_equipToBeremoved is WeaponItem)) {
                randomList.Add(currentWeapon);
            }
        }
        if (currentArmor != null) {
            if (!(p_equipToBeremoved is ArmorItem)) {
                randomList.Add(currentArmor);
            }
        }
        if (currentAccessory != null) {
            if (!(p_equipToBeremoved is AccessoryItem)) {
                randomList.Add(currentAccessory);
            }
        }
        if (randomList.Count <= 0) {
            return null;
        }
        int index = UnityEngine.Random.Range(0, randomList.Count);
        return randomList[index];
    }

    public bool HasEquips() {
        return (currentWeapon != null || currentArmor != null || currentAccessory != null);
    }

    public bool EvaluateNewEquipment(EquipmentItem p_newEquipment, Character p_character) {
        CharacterClassData characterClassData = CharacterManager.Instance.GetOrCreateCharacterClassData(p_character.characterClass.className);
        if (!characterClassData.craftableAccessories.Contains(p_newEquipment.tileObjectType) &&
            !characterClassData.craftableArmors.Contains(p_newEquipment.tileObjectType) &&
            !characterClassData.craftableWeapons.Contains(p_newEquipment.tileObjectType)) {
            return false;
        } 
        if (p_newEquipment is WeaponItem) {
            if (currentWeapon != null) {
                if (currentWeapon.equipmentData.tier < p_newEquipment.equipmentData.tier) {
                    return true;
                } else {
                    return false;
                }
            } else {
                return true;
            }  
        }
        if (p_newEquipment is ArmorItem) {
            if (currentArmor != null) {
                if (currentArmor.equipmentData.tier < p_newEquipment.equipmentData.tier) {
                    return true;
                } else {
                    return false;
                }
            } else {
                return true;
            }
        }
        if (p_newEquipment is AccessoryItem) {
            if (currentAccessory != null) {
                if (currentAccessory.equipmentData.tier < p_newEquipment.equipmentData.tier) {
                    return true;
                } else {
                    return false;
                }
            } else {
                return true;
            }
        }
        return false;
    }
}

[System.Serializable]
public class SaveDataEquipmentComponent : SaveData<EquipmentComponent> {
    public EquipmentItem currentWeapon;
    public EquipmentItem currentArmor;
    public EquipmentItem currentAccessory;

    #region Overrides
    public override void Save(EquipmentComponent data) {
        currentWeapon = data.currentWeapon;
        currentArmor = data.currentArmor;
        currentAccessory = data.currentAccessory;
    }

    public override EquipmentComponent Load() {
        EquipmentComponent component = new EquipmentComponent(this);
        return component;
    }
    #endregion
}