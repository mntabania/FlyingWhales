using System;
using System.Collections.Generic;
using System.Linq;
using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;

public class WorldGenOptionsUIView : MVCUIView {
    #region interface for listener
    public interface IListener {
        void OnChangeMapSize(MAP_SIZE p_value);
        void OnChangeMigrationSpeed(MIGRATION_SPEED p_value);
        void OnChangeVictoryCondition(VICTORY_CONDITION p_value);
        void OnChangeSkillCooldownSpeed(SKILL_COOLDOWN_SPEED p_value);
        void OnChangeSkillCostAmount(SKILL_COST_AMOUNT p_value);
        void OnChangeSkillChargeAmount(SKILL_CHARGE_AMOUNT p_value);
        void OnChangeThreatAmount(THREAT_AMOUNT p_value);
        void OnClickAddBiome();
        void OnClickAddFaction();
    }
    #endregion
    #region MVC Properties and functions to override
    /*
     * this will be the reference to the model 
     * */
    public WorldGenOptionsUIModel UIModel
    {
        get
        {
            return _baseAssetModel as WorldGenOptionsUIModel;
        }
    }

    /*
     * Call this Create method to Initialize and instantiate the UI.
     * There's a callback on the controller if you want custom initialization
     * */
    public static void Create(Canvas p_canvas, WorldGenOptionsUIModel p_assets, Action<WorldGenOptionsUIView> p_onCreate)
    {
        var go = new GameObject(typeof(WorldGenOptionsUIView).ToString());
        var gui = go.AddComponent<WorldGenOptionsUIView>();
        var assetsInstance = Instantiate(p_assets);
        gui.Init(p_canvas, assetsInstance);
        if (p_onCreate != null)
        {
            p_onCreate.Invoke(gui);
        }
    }
    #endregion
    
    #region Subscribe/Unsubscribe for IListener
    public void Subscribe(IListener p_listener) {
        UIModel.onChangeMapSize += p_listener.OnChangeMapSize;
        UIModel.onChangeMigrationSpeed += p_listener.OnChangeMigrationSpeed;
        UIModel.onChangeVictoryCondition += p_listener.OnChangeVictoryCondition;
        UIModel.onChangeSkillCooldownSpeed += p_listener.OnChangeSkillCooldownSpeed;
        UIModel.onChangeSkillCostAmount += p_listener.OnChangeSkillCostAmount;
        UIModel.onChangeSkillChargeAmount += p_listener.OnChangeSkillChargeAmount;
        UIModel.onChangeThreatAmount += p_listener.OnChangeThreatAmount;
        UIModel.onClickAddBiome += p_listener.OnClickAddBiome;
        UIModel.onClickAddFaction += p_listener.OnClickAddFaction;
    }
    public void Unsubscribe(IListener p_listener) {
        UIModel.onChangeMapSize -= p_listener.OnChangeMapSize;
        UIModel.onChangeMigrationSpeed -= p_listener.OnChangeMigrationSpeed;
        UIModel.onChangeVictoryCondition -= p_listener.OnChangeVictoryCondition;
        UIModel.onChangeSkillCooldownSpeed -= p_listener.OnChangeSkillCooldownSpeed;
        UIModel.onChangeSkillCostAmount -= p_listener.OnChangeSkillCostAmount;
        UIModel.onChangeSkillChargeAmount -= p_listener.OnChangeSkillChargeAmount;
        UIModel.onChangeThreatAmount -= p_listener.OnChangeThreatAmount;
        UIModel.onClickAddBiome -= p_listener.OnClickAddBiome;
        UIModel.onClickAddFaction -= p_listener.OnClickAddFaction;
    }
    #endregion

    #region User Defined Functions
    public void InitializeBiomeItems() {
        List<string> biomeChoices = UtilityScripts.Utilities.GetEnumChoices(GameUtilities.customWorldBiomeChoices);
        biomeChoices.Insert(0, "Random");
        for (int i = 0; i < UIModel.biomeDropdownUIItems.Length; i++) {
            BiomeDropdownUIItem item = UIModel.biomeDropdownUIItems[i];
            item.Initialize(biomeChoices);
            item.SetMinusBtnState(i != 0); //Disable Minus btn of first item
        }
    }
    public void InitializeFactionItems() {
        List<string> factionTypeChoices = UtilityScripts.Utilities.GetEnumChoices(GameUtilities.customWorldFactionTypeChoices);
        factionTypeChoices.Add("Random");
        for (int i = 0; i < UIModel.factionSettingUIItems.Length; i++) {
            FactionSettingUIItem item = UIModel.factionSettingUIItems[i];
            item.Initialize(factionTypeChoices);
            item.SetMinusBtnState(i != 0); //Disable Minus btn of first item
        }
    }
    public void InitializeMapSizeDropdown() {
        UIModel.dropDownMapSize.ClearOptions();
        UIModel.dropDownMapSize.AddOptions(UtilityScripts.Utilities.GetEnumChoices<MAP_SIZE>());
        UIModel.dropDownMapSize.value = UIModel.dropDownMapSize.GetDropdownOptionIndex("Small");
    }
    public void SetMapSizeDropdownValue(string p_value) {
        UIModel.dropDownMapSize.value = UIModel.dropDownMapSize.GetDropdownOptionIndex(p_value);
    }
    public void InitializeMigrationDropdown() {
        UIModel.dropDownMigration.ClearOptions();
        UIModel.dropDownMigration.AddOptions(UtilityScripts.Utilities.GetEnumChoices<MIGRATION_SPEED>());
        UIModel.dropDownMigration.value = UIModel.dropDownMigration.GetDropdownOptionIndex("Normal");
    }
    public void SetMigrationDropdownValue(string p_value) {
        UIModel.dropDownMigration.value = UIModel.dropDownMigration.GetDropdownOptionIndex(p_value);
    }
    public void InitializeVictoryConditionDropdown() {
        UIModel.dropDownVictory.ClearOptions();
        UIModel.dropDownVictory.AddOptions(UtilityScripts.Utilities.GetEnumChoices<VICTORY_CONDITION>());
        UIModel.dropDownVictory.value = UIModel.dropDownVictory.GetDropdownOptionIndex("Eliminate All");
    }
    public void SetVictoryDropdownValue(string p_value) {
        UIModel.dropDownVictory.value = UIModel.dropDownVictory.GetDropdownOptionIndex(p_value);
    }
    public void InitializeCooldownDropdown() {
        UIModel.dropDownCooldown.ClearOptions();
        UIModel.dropDownCooldown.AddOptions(UtilityScripts.Utilities.GetEnumChoices<SKILL_COOLDOWN_SPEED>());
        UIModel.dropDownCooldown.value = UIModel.dropDownCooldown.GetDropdownOptionIndex("Normal");
    }
    public void SetCooldownDropdownValue(string p_value) {
        UIModel.dropDownCooldown.value = UIModel.dropDownCooldown.GetDropdownOptionIndex(p_value);
    }
    public void InitializeCostsDropdown() {
        UIModel.dropDownCosts.ClearOptions();
        UIModel.dropDownCosts.AddOptions(UtilityScripts.Utilities.GetEnumChoices<SKILL_COST_AMOUNT>());
        UIModel.dropDownCosts.value = UIModel.dropDownCosts.GetDropdownOptionIndex("Normal");
    }
    public void SetCostsDropdownValue(string p_value) {
        UIModel.dropDownCosts.value = UIModel.dropDownCosts.GetDropdownOptionIndex(p_value);
    }
    public void InitializeChargesDropdown() {
        UIModel.dropDownCharges.ClearOptions();
        UIModel.dropDownCharges.AddOptions(UtilityScripts.Utilities.GetEnumChoices<SKILL_CHARGE_AMOUNT>());
        UIModel.dropDownCharges.value = UIModel.dropDownCharges.GetDropdownOptionIndex("Normal");
    }
    public void SetChargesDropdownValue(string p_value) {
        UIModel.dropDownCharges.value = UIModel.dropDownCharges.GetDropdownOptionIndex(p_value);
    }
    public void InitializeThreatDropdown() {
        UIModel.dropDownThreat.ClearOptions();
        UIModel.dropDownThreat.AddOptions(UtilityScripts.Utilities.GetEnumChoices<THREAT_AMOUNT>());
        UIModel.dropDownThreat.value = UIModel.dropDownThreat.GetDropdownOptionIndex("Normal");
    }
    public void SetThreatDropdownValue(string p_value) {
        UIModel.dropDownThreat.value = UIModel.dropDownThreat.GetDropdownOptionIndex(p_value);
    }
    public void HideBiomeItem(BiomeDropdownUIItem p_biomeItem) {
        p_biomeItem.gameObject.SetActive(false);
    }
    public void ShowBiomeItem(BiomeDropdownUIItem p_biomeItem) {
        p_biomeItem.gameObject.SetActive(true);
    }
    public void SetAddBiomeBtnState(bool p_state) {
        UIModel.btnAddBiome.gameObject.SetActive(p_state);
    }
    public BiomeDropdownUIItem GetInactiveBiomeDropdown() {
        for (int i = 0; i < UIModel.biomeDropdownUIItems.Length; i++) {
            BiomeDropdownUIItem item = UIModel.biomeDropdownUIItems[i];
            if (!item.gameObject.activeSelf) {
                return item;
            }
        }
        return null;
    }
    public void ResetBiomes() {
        for (int i = 0; i < UIModel.biomeDropdownUIItems.Length; i++) {
            BiomeDropdownUIItem item = UIModel.biomeDropdownUIItems[i];
            if (i != 0) {
                HideBiomeItem(item);
            } else {
                item.Reset();
            }
        }
    }
    public void HideFactionItem(FactionSettingUIItem p_item) {
        p_item.gameObject.SetActive(false);
    }
    public void ShowFactionItem(FactionSettingUIItem p_item) {
        p_item.gameObject.SetActive(true);
    }
    public void SetAddFactionBtnState(bool p_state) {
        UIModel.btnAddFaction.gameObject.SetActive(p_state);
    }
    public FactionSettingUIItem GetInactiveFactionSettingUIItem() {
        for (int i = 0; i < UIModel.factionSettingUIItems.Length; i++) {
            FactionSettingUIItem item = UIModel.factionSettingUIItems[i];
            if (!item.gameObject.activeSelf) {
                return item;
            }
        }
        return null;
    }
    public void ResetFactionItems() {
        for (int i = 0; i < UIModel.factionSettingUIItems.Length; i++) {
            FactionSettingUIItem item = UIModel.factionSettingUIItems[i];
            HideFactionItem(item);
            item.Reset();
        }
    }
    public void UpdateFactionItems(List<FactionSetting> p_factionSettings) {
        for (int i = 0; i < UIModel.factionSettingUIItems.Length; i++) {
            FactionSettingUIItem item = UIModel.factionSettingUIItems[i];
            FactionSetting factionSetting = p_factionSettings.ElementAtOrDefault(i);
            item.SetMinusBtnState(i != 0);
            if (factionSetting != null) {
                ShowFactionItem(item);
                item.SetItemDetails(factionSetting);
            } else {
                HideFactionItem(item);
            }
        }
    }
    public void UpdateVillageCount(int p_count, int p_max) {
        if (p_count > p_max) {
            UIModel.txtVillages.text = $"Villages: {UtilityScripts.Utilities.ColorizeInvalidText(p_count.ToString())}/{p_max.ToString()}";
        } else {
            UIModel.txtVillages.text = $"Villages: {p_count.ToString()}/{p_max.ToString()}";    
        }
        
    }
    #endregion

}
