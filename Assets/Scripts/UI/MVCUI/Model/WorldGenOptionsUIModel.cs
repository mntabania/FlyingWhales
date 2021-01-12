using System;
using Ruinarch.MVCFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldGenOptionsUIModel : MVCUIModel {

    public System.Action<MAP_SIZE> onChangeMapSize;
    public System.Action<MIGRATION_SPEED> onChangeMigrationSpeed;
    public System.Action<VICTORY_CONDITION> onChangeVictoryCondition;
    public System.Action<SKILL_COOLDOWN_SPEED> onChangeSkillCooldownSpeed;
    public System.Action<SKILL_COST_AMOUNT> onChangeSkillCostAmount;
    public System.Action<SKILL_CHARGE_AMOUNT> onChangeSkillChargeAmount;
    public System.Action<THREAT_AMOUNT> onChangeThreatAmount;
    public System.Action onClickAddBiome;
    public System.Action onClickAddFaction;
    
    public TMP_Dropdown dropDownMapSize;
    public TMP_Dropdown dropDownMigration;
    public TMP_Dropdown dropDownVictory;
    public TMP_Dropdown dropDownCooldown;
    public TMP_Dropdown dropDownCosts;
    public TMP_Dropdown dropDownCharges;
    public TMP_Dropdown dropDownThreat;

    [Header("Biomes")] 
    public BiomeDropdownUIItem[] biomeDropdownUIItems;
    public Button btnAddBiome;
    
    [Header("Factions")]
    public FactionSettingUIItem[] factionSettingUIItems;
    public Button btnAddFaction;
    public TextMeshProUGUI txtVillages;
    
    private void OnEnable() {
        dropDownMapSize.onValueChanged.AddListener(OnChangeMapSize);
        dropDownMigration.onValueChanged.AddListener(OnChangeMigrationSpeed);
        dropDownVictory.onValueChanged.AddListener(OnChangeVictoryCondition);
        dropDownCooldown.onValueChanged.AddListener(OnChangeCooldownSpeed);
        dropDownCosts.onValueChanged.AddListener(OnChangeSkillCost);
        dropDownCharges.onValueChanged.AddListener(OnChangeChargesAmount);
        dropDownThreat.onValueChanged.AddListener(OnChangeThreatAmount);
        btnAddBiome.onClick.AddListener(OnClickAddBiome);
        btnAddFaction.onClick.AddListener(OnClickAddFaction);
    }
    private void OnDisable() {
        dropDownMapSize.onValueChanged.RemoveListener(OnChangeMapSize);
        dropDownMigration.onValueChanged.RemoveListener(OnChangeMigrationSpeed);
        dropDownVictory.onValueChanged.RemoveListener(OnChangeVictoryCondition);
        dropDownCooldown.onValueChanged.RemoveListener(OnChangeCooldownSpeed);
        dropDownCosts.onValueChanged.RemoveListener(OnChangeSkillCost);
        dropDownCharges.onValueChanged.RemoveListener(OnChangeChargesAmount);
        dropDownThreat.onValueChanged.RemoveListener(OnChangeThreatAmount);
        btnAddBiome.onClick.RemoveListener(OnClickAddBiome);
        btnAddFaction.onClick.RemoveListener(OnClickAddFaction);
    }
    
    private void OnChangeMapSize(int p_index) {
        MAP_SIZE mapSize = dropDownMapSize.ConvertCurrentSelectedOption<MAP_SIZE>();
        onChangeMapSize?.Invoke(mapSize);
    }
    private void OnChangeMigrationSpeed(int p_index) {
        MIGRATION_SPEED migrationSpeed = dropDownMigration.ConvertCurrentSelectedOption<MIGRATION_SPEED>();
        onChangeMigrationSpeed?.Invoke(migrationSpeed);
    }
    private void OnChangeVictoryCondition(int p_index) {
        VICTORY_CONDITION victoryCondition = dropDownVictory.ConvertCurrentSelectedOption<VICTORY_CONDITION>();
        onChangeVictoryCondition?.Invoke(victoryCondition);
    }
    private void OnChangeCooldownSpeed(int p_index) {
        SKILL_COOLDOWN_SPEED cooldownSpeed = dropDownCooldown.ConvertCurrentSelectedOption<SKILL_COOLDOWN_SPEED>();
        onChangeSkillCooldownSpeed?.Invoke(cooldownSpeed);
    }
    private void OnChangeSkillCost(int p_index) {
        SKILL_COST_AMOUNT skillCostAmount = dropDownCosts.ConvertCurrentSelectedOption<SKILL_COST_AMOUNT>();
        onChangeSkillCostAmount?.Invoke(skillCostAmount);
    }
    private void OnChangeChargesAmount(int p_index) {
        SKILL_CHARGE_AMOUNT chargeAmount = dropDownCharges.ConvertCurrentSelectedOption<SKILL_CHARGE_AMOUNT>();
        onChangeSkillChargeAmount?.Invoke(chargeAmount);
    }
    private void OnChangeThreatAmount(int p_index) {
        THREAT_AMOUNT threatAmount = dropDownThreat.ConvertCurrentSelectedOption<THREAT_AMOUNT>();
        onChangeThreatAmount?.Invoke(threatAmount);
    }
    private void OnClickAddBiome() {
        onClickAddBiome?.Invoke();
    }
    private void OnClickAddFaction() {
        onClickAddFaction?.Invoke();
    }
}
