using System;
using System.Collections.Generic;
using Ruinarch.Custom_UI;
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
    public System.Action<RETALIATION> onChangeThreatAmount;
    public System.Action<OMNIPOTENT_MODE> onChangeOmnipotent;
    public System.Action onClickAddBiome;
    public System.Action onClickAddFaction;

    [Header("Map Size")]
    public TMP_Dropdown dropDownMapSize;
    public HoverHandler hoverHandlerMapSize;
    public System.Action<UIHoverPosition> onHoverOverMapSizeDropdown;
    public System.Action onHoverOutMapSizeDropdown;
    
    [Header("Migration")]
    public TMP_Dropdown dropDownMigration;
    public HoverHandler hoverHandlerMigration;
    public System.Action<UIHoverPosition> onHoverOverMigrationDropdown;
    public System.Action onHoverOutMigrationDropdown;
    
    [Header("Victory Condition")]
    public TMP_Dropdown dropDownVictory;
    public HoverHandler hoverHandlerVictory;
    public System.Action<UIHoverPosition> onHoverOverVictoryDropdown;
    public System.Action onHoverOutVictoryDropdown;
    
    [Header("Cooldown")]
    public TMP_Dropdown dropDownCooldown;
    public HoverHandler hoverHandlerCooldown;
    public System.Action<UIHoverPosition> onHoverOverCooldownDropdown;
    public System.Action onHoverOutCooldownDropdown;
    
    [Header("Costs")]
    public TMP_Dropdown dropDownCosts;
    public HoverHandler hoverHandlerCosts;
    public System.Action<UIHoverPosition> onHoverOverCostsDropdown;
    public System.Action onHoverOutCostsDropdown;
    
    [Header("Charges")]
    public TMP_Dropdown dropDownCharges;
    public HoverHandler hoverHandlerCharges;
    public System.Action<UIHoverPosition> onHoverOverChargesDropdown;
    public System.Action onHoverOutChargesDropdown;
    
    [Header("Threat")]
    public TMP_Dropdown dropDownThreat;
    public HoverHandler hoverHandlerThreat;
    public System.Action<UIHoverPosition> onHoverOverThreatDropdown;
    public System.Action onHoverOutThreatDropdown;

    [Header("Biomes")] 
    public BiomeDropdownUIItem[] biomeDropdownUIItems;
    public Button btnAddBiome;
    
    [Header("Factions")]
    public FactionSettingUIItem[] factionSettingUIItems;
    public Button btnAddFaction;
    public TextMeshProUGUI txtVillages;
    
    [Header("Omnipotent")]
    public TMP_Dropdown dropDownOmnipotent;
    public HoverHandler hoverHandlerOmnipotent;
    public System.Action<UIHoverPosition> onHoverOverOmnipotentDropdown;
    public System.Action onHoverOutOmnipotentDropdown;

    public UIHoverPosition tooltipPosition;

    private void OnEnable() {
        dropDownMapSize.onValueChanged.AddListener(OnChangeMapSize);
        dropDownMigration.onValueChanged.AddListener(OnChangeMigrationSpeed);
        dropDownVictory.onValueChanged.AddListener(OnChangeVictoryCondition);
        dropDownCooldown.onValueChanged.AddListener(OnChangeCooldownSpeed);
        dropDownCosts.onValueChanged.AddListener(OnChangeSkillCost);
        dropDownCharges.onValueChanged.AddListener(OnChangeChargesAmount);
        dropDownThreat.onValueChanged.AddListener(OnChangeThreatAmount);
        dropDownOmnipotent.onValueChanged.AddListener(OnChangeOmnipotent);
        
        btnAddBiome.onClick.AddListener(OnClickAddBiome);
        btnAddFaction.onClick.AddListener(OnClickAddFaction);
        
        hoverHandlerMapSize.AddOnHoverOverAction(OnHoverOverMapSize);
        hoverHandlerMapSize.AddOnHoverOutAction(OnHoverOutMapSize);
        
        hoverHandlerMigration.AddOnHoverOverAction(OnHoverOverMigration);
        hoverHandlerMigration.AddOnHoverOutAction(OnHoverOutMigration);
        
        hoverHandlerVictory.AddOnHoverOverAction(OnHoverOverVictory);
        hoverHandlerVictory.AddOnHoverOutAction(OnHoverOutVictory);
        
        hoverHandlerCooldown.AddOnHoverOverAction(OnHoverOverCooldown);
        hoverHandlerCooldown.AddOnHoverOutAction(OnHoverOutCooldown);
        
        hoverHandlerCosts.AddOnHoverOverAction(OnHoverOverCosts);
        hoverHandlerCosts.AddOnHoverOutAction(OnHoverOutCosts);
        
        hoverHandlerCharges.AddOnHoverOverAction(OnHoverOverCharges);
        hoverHandlerCharges.AddOnHoverOutAction(OnHoverOutCharges);
        
        hoverHandlerThreat.AddOnHoverOverAction(OnHoverOverThreat);
        hoverHandlerThreat.AddOnHoverOutAction(OnHoverOutThreat);
        
        hoverHandlerOmnipotent.AddOnHoverOverAction(OnHoverOverOmnipotent);
        hoverHandlerOmnipotent.AddOnHoverOutAction(OnHoverOutOmnipotent);
    }
    private void OnDisable() {
        dropDownMapSize.onValueChanged.RemoveListener(OnChangeMapSize);
        dropDownMigration.onValueChanged.RemoveListener(OnChangeMigrationSpeed);
        dropDownVictory.onValueChanged.RemoveListener(OnChangeVictoryCondition);
        dropDownCooldown.onValueChanged.RemoveListener(OnChangeCooldownSpeed);
        dropDownCosts.onValueChanged.RemoveListener(OnChangeSkillCost);
        dropDownCharges.onValueChanged.RemoveListener(OnChangeChargesAmount);
        dropDownThreat.onValueChanged.RemoveListener(OnChangeThreatAmount);
        dropDownOmnipotent.onValueChanged.RemoveListener(OnChangeOmnipotent);
        
        btnAddBiome.onClick.RemoveListener(OnClickAddBiome);
        btnAddFaction.onClick.RemoveListener(OnClickAddFaction);
        
        hoverHandlerMapSize.RemoveOnHoverOverAction(OnHoverOverMapSize);
        hoverHandlerMapSize.RemoveOnHoverOutAction(OnHoverOutMapSize);

        hoverHandlerMigration.RemoveOnHoverOverAction(OnHoverOverMigration);
        hoverHandlerMigration.RemoveOnHoverOutAction(OnHoverOutMigration);
        
        hoverHandlerVictory.RemoveOnHoverOverAction(OnHoverOverVictory);
        hoverHandlerVictory.RemoveOnHoverOutAction(OnHoverOutVictory);
        
        hoverHandlerCooldown.RemoveOnHoverOverAction(OnHoverOverCooldown);
        hoverHandlerCooldown.RemoveOnHoverOutAction(OnHoverOutCooldown);
        
        hoverHandlerCosts.RemoveOnHoverOverAction(OnHoverOverCosts);
        hoverHandlerCosts.RemoveOnHoverOutAction(OnHoverOutCosts);
        
        hoverHandlerCharges.RemoveOnHoverOverAction(OnHoverOverCharges);
        hoverHandlerCharges.RemoveOnHoverOutAction(OnHoverOutCharges);
        
        hoverHandlerThreat.RemoveOnHoverOverAction(OnHoverOverThreat);
        hoverHandlerThreat.RemoveOnHoverOutAction(OnHoverOutThreat);
        
        hoverHandlerOmnipotent.RemoveOnHoverOverAction(OnHoverOverOmnipotent);
        hoverHandlerOmnipotent.RemoveOnHoverOutAction(OnHoverOutOmnipotent);
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
        RETALIATION retaliation = dropDownThreat.ConvertCurrentSelectedOption<RETALIATION>();
        onChangeThreatAmount?.Invoke(retaliation);
    }
    private void OnChangeOmnipotent(int p_index) {
        OMNIPOTENT_MODE omnipotentMode = dropDownOmnipotent.ConvertCurrentSelectedOption<OMNIPOTENT_MODE>();
        onChangeOmnipotent?.Invoke(omnipotentMode);
    }
    private void OnClickAddBiome() {
        onClickAddBiome?.Invoke();
    }
    private void OnClickAddFaction() {
        onClickAddFaction?.Invoke();
    }

    #region Map Size
    private void OnHoverOverMapSize() {
        onHoverOverMapSizeDropdown?.Invoke(tooltipPosition);
    }
    private void OnHoverOutMapSize() {
        onHoverOutMapSizeDropdown?.Invoke();
    }
    #endregion
    
    #region Migration
    private void OnHoverOverMigration() {
        onHoverOverMigrationDropdown?.Invoke(tooltipPosition);
    }
    private void OnHoverOutMigration() {
        onHoverOutMigrationDropdown?.Invoke();
    }
    #endregion
    
    #region Victory
    private void OnHoverOverVictory() {
        onHoverOverVictoryDropdown?.Invoke(tooltipPosition);
    }
    private void OnHoverOutVictory() {
        onHoverOutVictoryDropdown?.Invoke();
    }
    #endregion
    
    #region Cooldown
    private void OnHoverOverCooldown() {
        onHoverOverCooldownDropdown?.Invoke(tooltipPosition);
    }
    private void OnHoverOutCooldown() {
        onHoverOutCooldownDropdown?.Invoke();
    }
    #endregion
    
    #region Costs
    private void OnHoverOverCosts() {
        onHoverOverCostsDropdown?.Invoke(tooltipPosition);
    }
    private void OnHoverOutCosts() {
        onHoverOutCostsDropdown?.Invoke();
    }
    #endregion
    
    #region Charges
    private void OnHoverOverCharges() {
        onHoverOverChargesDropdown?.Invoke(tooltipPosition);
    }
    private void OnHoverOutCharges() {
        onHoverOutChargesDropdown?.Invoke();
    }
    #endregion
    
    #region Threat
    private void OnHoverOverThreat() {
        onHoverOverThreatDropdown?.Invoke(tooltipPosition);
    }
    private void OnHoverOutThreat() {
        onHoverOutThreatDropdown?.Invoke();
    }
    #endregion
    
    #region Omnipotent
    private void OnHoverOverOmnipotent() {
        onHoverOverOmnipotentDropdown?.Invoke(tooltipPosition);
    }
    private void OnHoverOutOmnipotent() {
        onHoverOutOmnipotentDropdown?.Invoke();
    }
    #endregion
}
