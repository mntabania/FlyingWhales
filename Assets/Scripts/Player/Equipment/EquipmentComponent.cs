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
            EquipmentBonusProcessor.RemoveEquipBonusToTarget(currentWeapon.equipmentData, p_targetCharacter);
        }
        currentWeapon = p_newWeapon;
        //apply new weapon stats again
        EquipmentBonusProcessor.ApplyEquipBonusToTarget(currentWeapon.equipmentData, p_targetCharacter);
    }

    private void SetArmor(EquipmentItem p_newArmor, Character p_targetCharacter) {
        //remove old Armor stats first
        if(currentArmor != null) {
            EquipmentBonusProcessor.RemoveEquipBonusToTarget(currentArmor.equipmentData, p_targetCharacter);
        }
        currentArmor = p_newArmor;
        //apply new Armor stats again
        EquipmentBonusProcessor.ApplyEquipBonusToTarget(currentArmor.equipmentData, p_targetCharacter);
    }

    private void SetAccessory(EquipmentItem p_newAaccessory, Character p_targetCharacter) {
        //remove old Accessory stats first
        if(currentAccessory != null) {
            EquipmentBonusProcessor.RemoveEquipBonusToTarget(currentAccessory.equipmentData, p_targetCharacter);
        }
        currentAccessory = p_newAaccessory;
        //apply new Accessory stats again
        EquipmentBonusProcessor.ApplyEquipBonusToTarget(currentAccessory.equipmentData, p_targetCharacter);
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
