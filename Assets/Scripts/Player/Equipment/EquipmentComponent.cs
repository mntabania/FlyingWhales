using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EquipmentComponent {

    private Action<Equipment, Equipment> m_onWeaponChanged;
    private Action<Equipment, Equipment> m_onArmorChanged;
    private Action<Equipment, Equipment> m_onAccessoryChanged;

    public interface EquipmentEvensListener {
        void OnWeaponChanged(Equipment p_currentWeapon, Equipment p_newWeapon);
        void OnArmorChanged(Equipment p_currentArmor, Equipment p_newArmor);
        void OnAccessoryChanged(Equipment p_currentAccessory, Equipment p_newAccessory);
    }

    public Equipment currentWeapon { private set; get; }
    public Equipment currentArmor { private set; get; }
    public Equipment currentAccessory { private set; get; }

    public EquipmentComponent() {
        Reset();
    }

    void Reset() {
        currentWeapon = null;
        currentArmor = null;
        currentAccessory = null;
    }

    public void SetWeapon(Equipment p_newWeapon) {
        m_onWeaponChanged?.Invoke(currentWeapon, p_newWeapon);
        currentWeapon = p_newWeapon;
    }

    public void SetArmor(Equipment p_newArmor) {
        m_onArmorChanged?.Invoke(currentArmor, p_newArmor);
        currentArmor = p_newArmor;
    }

    public void SetAccessory(Equipment p_newAaccessory) {
        m_onAccessoryChanged?.Invoke(currentAccessory, p_newAaccessory);
        currentAccessory = p_newAaccessory;
    }

    #region Subs/Unsubs
    public void Subscribe(EquipmentEvensListener p_iListener) {
        m_onWeaponChanged += p_iListener.OnWeaponChanged;
        m_onArmorChanged += p_iListener.OnArmorChanged;
        m_onAccessoryChanged += p_iListener.OnAccessoryChanged;
    }

    public void Unsubscribe(EquipmentEvensListener p_iListener) {
        m_onWeaponChanged -= p_iListener.OnWeaponChanged;
        m_onArmorChanged -= p_iListener.OnArmorChanged;
        m_onAccessoryChanged -= p_iListener.OnAccessoryChanged;
    }
    #endregion
}
