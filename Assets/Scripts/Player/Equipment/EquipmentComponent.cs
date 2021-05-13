using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EquipmentComponent {

    public EquipmentItem currentWeapon { private set; get; }
    public EquipmentItem currentArmor { private set; get; }
    public EquipmentItem currentAccessory { private set; get; }

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

    private void SetWeapon(EquipmentItem p_newWeapon, Character p_targetCharacter) {
        //remove old weapon stats first
        if(currentWeapon != null) {
            p_targetCharacter.UnobtainItem(currentWeapon);
        }
        currentWeapon = p_newWeapon;
        //apply new weapon stats again
        EquipmentBonusProcessor.ApplyEquipBonusToTarget(currentWeapon, p_targetCharacter);
    }

    private void SetArmor(EquipmentItem p_newArmor, Character p_targetCharacter) {
        //remove old Armor stats first
        if(currentArmor != null) {
            p_targetCharacter.UnobtainItem(currentArmor);
        }
        currentArmor = p_newArmor;
        //apply new Armor stats again
        EquipmentBonusProcessor.ApplyEquipBonusToTarget(currentArmor, p_targetCharacter);
    }

    private void SetAccessory(EquipmentItem p_newAaccessory, Character p_targetCharacter) {
        //remove old Accessory stats first
        if(currentAccessory != null) {
            p_targetCharacter.UnobtainItem(currentAccessory);
        }
        currentAccessory = p_newAaccessory;
        //apply new Accessory stats again
        EquipmentBonusProcessor.ApplyEquipBonusToTarget(currentAccessory, p_targetCharacter);
    }

    public void SetEquipment(EquipmentItem p_newItem, Character p_targetCharacter) {
        if(p_newItem is WeaponItem) {
            SetWeapon(p_newItem, p_targetCharacter);
		}
        if (p_newItem is ArmorItem) {
            SetArmor(p_newItem, p_targetCharacter);
        }
        if (p_newItem is AccessoryItem) {
            SetAccessory(p_newItem, p_targetCharacter);
        }
    }

    public void RemoveEquipment(EquipmentItem p_newItem, Character p_targetCharacter) {
        if (p_newItem is WeaponItem) {
            currentWeapon = null;
        }
        if (p_newItem is ArmorItem) {
            currentArmor = null;
        }
        if (p_newItem is AccessoryItem) {
            currentAccessory = null;
        }
        EquipmentBonusProcessor.RemoveEquipBonusToTarget(p_newItem, p_targetCharacter);
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
                randomList.Add(currentArmor);
            }
        }
        if (randomList.Count <= 0) {
            return null;
        }
        return randomList[UnityEngine.Random.Range(0, randomList.Count)];
    }

    public bool HasEquips() {
        return (currentWeapon != null || currentArmor != null || currentAccessory != null);
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
