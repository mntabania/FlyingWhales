using System;
using System.Collections.Generic;
using UnityEngine;
using Ruinarch.MVCFramework;
using TMPro;
using UnityEngine.UI;
using UtilityScripts;

public class WorldGenOptionsUIController : MVCUIController, WorldGenOptionsUIView.IListener {
	[SerializeField]
	private WorldGenOptionsUIModel m_worldGenOptionsUIModel;
	private WorldGenOptionsUIView m_worldGenOptionsUIView;

	public FactionSettingVillageEditorUIController factionSettingVillageEditorUIController;
	
	private List<string> _chosenBiomes;

	private System.Action onUpdateVillageCountAction;
	
	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		WorldGenOptionsUIView.Create(_canvas, m_worldGenOptionsUIModel, (p_ui) => {
			m_worldGenOptionsUIView = p_ui;
			m_worldGenOptionsUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);

			factionSettingVillageEditorUIController.onUIINstantiated += OnFactionSettingVillageEditorInstantiated;
			factionSettingVillageEditorUIController.InstantiateUI();
		});
	}
	private void OnFactionSettingVillageEditorInstantiated() {
		factionSettingVillageEditorUIController.SetParent(m_worldGenOptionsUIView.UIModel.parentDisplay);
		factionSettingVillageEditorUIController.HideUI();
		factionSettingVillageEditorUIController.SetOnHideAction(OnDoneEditingVillages);
	}
	private void OnEnable() {
		BiomeDropdownUIItem.onChooseBiome += OnChooseBiome;
		BiomeDropdownUIItem.onClickMinus += OnClickMinusBiome;
		BiomeDropdownUIItem.onHoverOverBiomeItem += OnHoverOverBiomeItem;
		BiomeDropdownUIItem.onHoverOutBiomeItem += OnHoverOutBiomeItem;
		FactionSettingUIItem.onClickMinus += OnDeleteFactionSetting;
		FactionSettingUIItem.onChangeName += OnChangeFactionSettingName;
		FactionSettingUIItem.onClickRandomizeName += OnClickRandomizeFactionName;
		FactionSettingUIItem.onChangeFactionType += OnChangeFactionSettingFactionType;
		FactionSettingUIItem.onClickEditVillages += OnClickEditFactionVillages;
		FactionSettingUIItem.onHoverOverEditVillages += OnHoverOverEditFactionVillages;
		FactionSettingUIItem.onHoverOutEditVillages += OnHoverOutEditFactionVillages;
	}
	private void OnDisable() {
		BiomeDropdownUIItem.onChooseBiome -= OnChooseBiome;
		BiomeDropdownUIItem.onClickMinus -= OnClickMinusBiome;
		BiomeDropdownUIItem.onHoverOverBiomeItem -= OnHoverOverBiomeItem;
		BiomeDropdownUIItem.onHoverOutBiomeItem -= OnHoverOutBiomeItem;
		FactionSettingUIItem.onClickMinus -= OnDeleteFactionSetting;
		FactionSettingUIItem.onChangeName -= OnChangeFactionSettingName;
		FactionSettingUIItem.onClickRandomizeName -= OnClickRandomizeFactionName;
		FactionSettingUIItem.onChangeFactionType -= OnChangeFactionSettingFactionType;
		FactionSettingUIItem.onClickEditVillages -= OnClickEditFactionVillages;
		FactionSettingUIItem.onHoverOverEditVillages -= OnHoverOverEditFactionVillages;
		FactionSettingUIItem.onHoverOutEditVillages -= OnHoverOutEditFactionVillages;
		factionSettingVillageEditorUIController.onUIINstantiated -= OnFactionSettingVillageEditorInstantiated;
	}

	public void InitUI(System.Action p_onUpdateVillageCountAction) {
		InstantiateUI();
		_chosenBiomes = new List<string>();
		onUpdateVillageCountAction = p_onUpdateVillageCountAction;
		
		m_worldGenOptionsUIView.InitializeMapSizeDropdown();
		m_worldGenOptionsUIView.InitializeBiomeItems();
		m_worldGenOptionsUIView.InitializeFactionItems();
		m_worldGenOptionsUIView.InitializeMigrationDropdown();
		m_worldGenOptionsUIView.InitializeVictoryConditionDropdown();
		m_worldGenOptionsUIView.InitializeCooldownDropdown();
		m_worldGenOptionsUIView.InitializeCostsDropdown();
		m_worldGenOptionsUIView.InitializeChargesDropdown();
		m_worldGenOptionsUIView.InitializeThreatDropdown();
		m_worldGenOptionsUIView.InitializeOmnipotentModeDropdown();
		
		OnChangeMapSize(0);
		UpdateAddBiomeBtn();
	}
	public bool IsUIShowing() {
		return m_worldGenOptionsUIView.UIModel.parentDisplay.gameObject.activeInHierarchy;
	}
	public override void HideUI() {
		base.HideUI();
		factionSettingVillageEditorUIController.HideUI();
	}
	public override void ShowUI() {
		base.ShowUI();
		WorldSettings.Instance.worldSettingsData.SetVictoryCondition(VICTORY_CONDITION.Eliminate_All);
		UpdateUIBasedOnCurrentSettings(WorldSettings.Instance.worldSettingsData);
	}
	public void ApplyBiomeSettings() {
		WorldSettings.Instance.worldSettingsData.mapSettings.ApplyBiomeSettings(_chosenBiomes);
	}
	public void ApplyCurrentSettingsToData() {
		//apply chosen biomes to actual data
		// ApplyBiomeSettings();
		WorldSettings.Instance.worldSettingsData.factionSettings.FinalizeFactionTemplates();
	}
	private void UpdateUIBasedOnCurrentSettings(WorldSettingsData p_settings) {
		//Note: Updating map size dropdown will also update faction and biome data automatically because of on change event 
		m_worldGenOptionsUIView.SetMapSizeDropdownValue(UtilityScripts.Utilities.NotNormalizedConversionEnumToString(p_settings.mapSettings.mapSize.ToString()));
		OnChangeMapSize(p_settings.mapSettings.mapSize);
		m_worldGenOptionsUIView.SetMigrationDropdownValue(UtilityScripts.Utilities.NotNormalizedConversionEnumToString(p_settings.villageSettings.migrationSpeed.ToString()));
		m_worldGenOptionsUIView.SetVictoryDropdownValue(UtilityScripts.Utilities.NotNormalizedConversionEnumToString(p_settings.victoryCondition.ToString()));
		m_worldGenOptionsUIView.SetCooldownDropdownValue(UtilityScripts.Utilities.NotNormalizedConversionEnumToString(p_settings.playerSkillSettings.cooldownSpeed.ToString()));
		m_worldGenOptionsUIView.SetCostsDropdownValue(UtilityScripts.Utilities.NotNormalizedConversionEnumToString(p_settings.playerSkillSettings.costAmount.ToString()));
		m_worldGenOptionsUIView.SetChargesDropdownValue(UtilityScripts.Utilities.NotNormalizedConversionEnumToString(p_settings.playerSkillSettings.chargeAmount.ToString()));
		m_worldGenOptionsUIView.SetThreatDropdownValue(UtilityScripts.Utilities.NotNormalizedConversionEnumToString(p_settings.playerSkillSettings.retaliation.ToString()));
		m_worldGenOptionsUIView.SetOmnipotentDropdownValue(UtilityScripts.Utilities.NotNormalizedConversionEnumToString(p_settings.playerSkillSettings.omnipotentMode.ToString()));
	}

	#region Biomes
	private void AddBiome(string p_biome) {
		_chosenBiomes.Add(p_biome);
		Debug.Log($"Added biome {p_biome}. Chosen Biomes are: {_chosenBiomes.ComafyList()}");
	}
	private void RemoveBiome(string p_biome) {
		_chosenBiomes.Remove(p_biome);
		Debug.Log($"Removed biome {p_biome}. Chosen Biomes are: {_chosenBiomes.ComafyList()}");
	}
	private void ReplaceBiome(int p_index, string p_biome) {
		string previousValue = _chosenBiomes[p_index];
		_chosenBiomes.RemoveAt(p_index);
		_chosenBiomes.Insert(p_index, p_biome);
		Debug.Log($"Replaced biome {previousValue} to {p_biome}. Chosen Biomes are: {_chosenBiomes.ComafyList()}");
	}
	private void ResetBiomes() {
		_chosenBiomes.Clear();
		_chosenBiomes.Add("Random");
		m_worldGenOptionsUIView.ResetBiomes();
		Debug.Log($"Reset Biomes. Chosen Biomes are: {_chosenBiomes.ComafyList()}");
	}
	#endregion
	
	#region Biome Item
	private void OnChooseBiome(BiomeDropdownUIItem p_item, string p_biome) {
		int index = p_item.transform.GetSiblingIndex();
		ReplaceBiome(index, p_biome);
	}
	private void OnClickMinusBiome(BiomeDropdownUIItem p_item, string p_biome) {
		RemoveBiome(p_biome);
		m_worldGenOptionsUIView.HideBiomeItem(p_item);
		UpdateAddBiomeBtn();
		p_item.transform.SetSiblingIndex(_chosenBiomes.Count);
	}
	private void UpdateAddBiomeBtn() {
		m_worldGenOptionsUIView.SetAddBiomeBtnState(_chosenBiomes.Count < WorldSettings.Instance.worldSettingsData.mapSettings.GetMaxBiomeCount());
	}
	private void AddDefaultBiomeItem() {
		BiomeDropdownUIItem inactiveItem = m_worldGenOptionsUIView.GetInactiveBiomeDropdown();
		if (inactiveItem != null) {
			inactiveItem.transform.SetSiblingIndex(_chosenBiomes.Count);
			m_worldGenOptionsUIView.ShowBiomeItem(inactiveItem);
			inactiveItem.Reset();
			AddBiome("Random"); //because default choice is Random
			UpdateAddBiomeBtn();
		}
	}
	private void OnHoverOverBiomeItem() {
		Tooltip.Instance.ShowSmallInfo("Choose the type of biome for each region.", pos: m_worldGenOptionsUIView.UIModel.tooltipPosition, "Biomes");
	}
	private void OnHoverOutBiomeItem() {
		Tooltip.Instance.HideSmallInfo();
	}
	#endregion

	#region Faction Settings Item
	private void OnDeleteFactionSetting(FactionTemplate p_FactionTemplate, FactionSettingUIItem p_uiItem) {
		WorldSettings.Instance.worldSettingsData.factionSettings.RemoveFactionSetting(p_FactionTemplate);
		m_worldGenOptionsUIView.HideFactionItem(p_uiItem);
		FactionEmblemRandomizer.SetEmblemAsUnUsed(p_FactionTemplate.factionEmblem);
		UpdateAddFactionBtn();
		UpdateVillageCount();
	}
	private void OnChangeFactionSettingName(FactionTemplate p_FactionTemplate, string p_newName) {
		p_FactionTemplate.ChangeName(p_newName);
	}
	private void OnClickRandomizeFactionName(FactionTemplate p_FactionTemplate, FactionSettingUIItem p_uiItem) {
		p_FactionTemplate.ChangeName(RandomNameGenerator.GenerateFactionName());
		p_uiItem.UpdateName(p_FactionTemplate.name);
	}
	private void OnChangeFactionSettingFactionType(FactionTemplate p_FactionTemplate, string p_factionTypeStr) {
		p_FactionTemplate.ChangeFactionType(p_factionTypeStr);
	}
	private void OnClickEditFactionVillages(FactionTemplate p_FactionTemplate) {
		factionSettingVillageEditorUIController.EditVillageSettings(p_FactionTemplate);
	}
	private void OnHoverOverEditFactionVillages(FactionTemplate p_FactionTemplate) {
		Tooltip.Instance.ShowSmallInfo($"Edit {p_FactionTemplate.name}'s villages");
	}
	private void OnHoverOutEditFactionVillages(FactionTemplate p_FactionTemplate) {
		Tooltip.Instance.HideSmallInfo();
	}
	private void UpdateAddFactionBtn() {
		m_worldGenOptionsUIView.SetAddFactionBtnState(!WorldSettings.Instance.worldSettingsData.HasReachedMaxStartingFactionCount());
	}
	private void ResetFactions() {
		WorldSettings.Instance.worldSettingsData.factionSettings.ClearFactionSettings();
		m_worldGenOptionsUIView.ResetFactionItems();
		FactionEmblemRandomizer.Reset();
	}
	private void AddDefaultFactionSetting() {
		int maxFactions = WorldSettings.Instance.worldSettingsData.mapSettings.GetMaxStartingFactions();
		int maxVillages = WorldSettings.Instance.worldSettingsData.mapSettings.GetMaxStartingVillages();
		int dividedVillages = maxVillages / maxFactions;
		FactionTemplate factionTemplate = WorldSettings.Instance.worldSettingsData.factionSettings.AddFactionSetting(dividedVillages);
		Sprite factionEmblem = FactionEmblemRandomizer.GetUnusedFactionEmblem();
		factionTemplate.SetFactionEmblem(factionEmblem);
		FactionEmblemRandomizer.SetEmblemAsUsed(factionEmblem);
		m_worldGenOptionsUIView.UpdateFactionItems(WorldSettings.Instance.worldSettingsData.factionSettings.factionTemplates);
		UpdateAddFactionBtn();
		UpdateVillageCount();
	}
	private void UpdateVillageCount() {
		int maxVillages = WorldSettings.Instance.worldSettingsData.mapSettings.GetMaxStartingVillages();
		int currentVillageCount = WorldSettings.Instance.worldSettingsData.factionSettings.GetCurrentTotalVillageCountBasedOnFactions();
		m_worldGenOptionsUIView.UpdateVillageCount(currentVillageCount, maxVillages);
		onUpdateVillageCountAction?.Invoke();
	}
	private void OnDoneEditingVillages() {
		UpdateVillageCount();
		m_worldGenOptionsUIView.UpdateFactionItems(WorldSettings.Instance.worldSettingsData.factionSettings.factionTemplates);
	}
	#endregion

	#region WorldGenOptionsUIView.IListener Implementation
	public void OnChangeMapSize(MAP_SIZE p_value) {
		ResetBiomes();
		ResetFactions();
		WorldSettings.Instance.worldSettingsData.mapSettings.SetMapSize(p_value);
		int maxBiomes = WorldSettings.Instance.worldSettingsData.mapSettings.GetMaxBiomeCount();
		for (int i = 1; i < maxBiomes; i++) {
			AddDefaultBiomeItem();
		}
		UpdateAddBiomeBtn();
		
		//reset faction settings
		int maxFactions = WorldSettings.Instance.worldSettingsData.mapSettings.GetMaxStartingFactions();
		for (int i = 0; i < maxFactions; i++) {
			AddDefaultFactionSetting();
		}
		m_worldGenOptionsUIView.UpdateFactionItems(WorldSettings.Instance.worldSettingsData.factionSettings.factionTemplates);
		UpdateAddFactionBtn();
	}
	public void OnChangeMigrationSpeed(MIGRATION_SPEED p_value) {
		WorldSettings.Instance.worldSettingsData.villageSettings.SetMigrationSpeed(p_value);
	}
	public void OnChangeVictoryCondition(VICTORY_CONDITION p_value) {
		WorldSettings.Instance.worldSettingsData.SetVictoryCondition(p_value);
	}
	public void OnChangeSkillCooldownSpeed(SKILL_COOLDOWN_SPEED p_value) {
		WorldSettings.Instance.worldSettingsData.playerSkillSettings.SetCooldownSpeed(p_value);
	}
	public void OnChangeSkillCostAmount(SKILL_COST_AMOUNT p_value) {
		WorldSettings.Instance.worldSettingsData.playerSkillSettings.SetManaCostAmount(p_value);
	}
	public void OnChangeSkillChargeAmount(SKILL_CHARGE_AMOUNT p_value) {
		WorldSettings.Instance.worldSettingsData.playerSkillSettings.SetChargeAmount(p_value);
	}
	public void OnChangeThreatAmount(RETALIATION p_value) {
		WorldSettings.Instance.worldSettingsData.playerSkillSettings.SetRetaliationState(p_value);
	}
	public void OnChangeOmnipotentMode(OMNIPOTENT_MODE p_value) {
		WorldSettings.Instance.worldSettingsData.playerSkillSettings.SetOmnipotentMode(p_value);
	}
	public void OnClickAddBiome() {
		AddDefaultBiomeItem();
	}
	public void OnClickAddFaction() {
		AddDefaultFactionSetting();
	}
	#endregion

	#region Tooltips
	public void OnHoverOverMapSize(UIHoverPosition p_pos) {
		string summary = "<b>Small</b> - Can accommodate 1 faction and 1 village only." +
		                 "\n<b>Medium</b> - Can accommodate 1 faction with up to 2 villages." +
		                 "\n<b>Large</b> - Can accommodate 2 factions with up to 4 villages." +
		                 "\n<b>Extra Large</b> - Can accommodate 3 factions with up to 6 villages.";
		Tooltip.Instance.ShowSmallInfo(summary, pos: p_pos, "Map Size", autoReplaceText: false);
	}
	public void OnHoverOutMapSize() {
		Tooltip.Instance.HideSmallInfo();
	}
	public void OnHoverOverMigration(UIHoverPosition p_pos) {
		string summary = "Set the frequency of Villager Migrations.\n" +
		                 "\n<b>None</b> - Migration disabled, No new Villagers will spawn." +
		                 "\n<b>Slow</b> - Migration rate slowed by half." +
		                 "\n<b>Normal</b> - Normal migration rate.";
		Tooltip.Instance.ShowSmallInfo(summary, pos: p_pos, "Migration Speed", autoReplaceText: false);
	}
	public void OnHoverOutMigration() {
		Tooltip.Instance.HideSmallInfo();
	}
	public void OnHoverOverVictory(UIHoverPosition p_pos) {
		string summary = "Set the game's victory condition.\n" +
		                 "\n<b>Summon Ruinarch</b> - Upgrade Portal to Level 8 to win the game." +
		                 "\n<b>Sandbox</b> - No victory conditions. Play to your heart's content.";
		Tooltip.Instance.ShowSmallInfo(summary, pos: p_pos, "Victory Condition", autoReplaceText: false);
	}
	public void OnHoverOutVictory() {
		Tooltip.Instance.HideSmallInfo();
	}
	public void OnHoverOverCooldown(UIHoverPosition p_pos) {
		string summary = "Set the speed of your ability's recharge.\n" +
		                 "\n<b>None</b> - All actions have no cooldown." +
		                 "\n<b>Half</b> - Cooldown duration reduced to half of normal (rounded up)." +
		                 "\n<b>Normal</b> - Normal cooldown duration." +
		                 "\n<b>Double</b> - Cooldown duration twice as long as normal.";
		Tooltip.Instance.ShowSmallInfo(summary, pos: p_pos, $"{UtilityScripts.Utilities.CooldownIcon()} Cooldown", autoReplaceText: false);
	}
	public void OnHoverOutCooldown() {
		Tooltip.Instance.HideSmallInfo();
	}
	public void OnHoverOverCosts(UIHoverPosition p_pos) {
		string summary = "Set the costs of your abilities.\n" +
						 "\n<b>None</b> - All actions have zero cost." +
						 "\n<b>Half</b> - Costs are reduced to half of normal (rounded up)." +
						 "\n<b>Normal</b> - Normal cost.";
		Tooltip.Instance.ShowSmallInfo(summary, pos: p_pos, $"Costs", autoReplaceText: false);
	}
	public void OnHoverOutCosts() {
		Tooltip.Instance.HideSmallInfo();
	}
	public void OnHoverOverCharges(UIHoverPosition p_pos) {
		string summary = "Set the number of base charges of your abilities.\n" +
		                 "\n<b>Unlimited</b> - All actions have unlimited number of charges." +
		                 "\n<b>Half</b> - Number of charges reduced to half of normal (rounded up)." +
		                 "\n<b>Normal</b> - Normal number of charges." +
		                 "\n<b>Double</b> - Number of charges double of normal.";
		Tooltip.Instance.ShowSmallInfo(summary, pos: p_pos, $"{UtilityScripts.Utilities.ChargesIcon()} Charges", autoReplaceText: false);
	}
	public void OnHoverOutCharges() {
		Tooltip.Instance.HideSmallInfo();
	}
	public void OnHoverOverThreat(UIHoverPosition p_pos) {
		string summary = "Enable or Disable Retaliation. If Disabled, Angels will no longer spawn and attack your base.";
		Tooltip.Instance.ShowSmallInfo(summary, pos: p_pos, $"Retaliation", autoReplaceText: false);
	}
	public void OnHoverOutThreat() {
		Tooltip.Instance.HideSmallInfo();
	}
	public void OnHoverOverOmnipotent(UIHoverPosition p_pos) {
		string summary = "Enable or Disable Omnipotent Mode. Omnipotent Mode allows you to use all skills, regardless of archetype";
		Tooltip.Instance.ShowSmallInfo(summary, pos: p_pos, $"Omnipotent Mode", autoReplaceText: false);
	}
	public void OnHoverOutOmnipotent() {
		Tooltip.Instance.HideSmallInfo();
	}
	#endregion
}