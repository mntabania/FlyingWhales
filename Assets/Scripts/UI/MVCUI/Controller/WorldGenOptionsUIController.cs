using System.Collections.Generic;
using UnityEngine;
using Ruinarch.MVCFramework;

public class WorldGenOptionsUIController : MVCUIController, WorldGenOptionsUIView.IListener {
	[SerializeField]
	private WorldGenOptionsUIModel m_worldGenOptionsUIModel;
	private WorldGenOptionsUIView m_worldGenOptionsUIView;

	public FactionSettingVillageEditorUIController factionSettingVillageEditorUIController;
	
	private List<string> _chosenBiomes;
	
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
		FactionSettingUIItem.onClickMinus -= OnDeleteFactionSetting;
		FactionSettingUIItem.onChangeName -= OnChangeFactionSettingName;
		FactionSettingUIItem.onClickRandomizeName -= OnClickRandomizeFactionName;
		FactionSettingUIItem.onChangeFactionType -= OnChangeFactionSettingFactionType;
		FactionSettingUIItem.onClickEditVillages -= OnClickEditFactionVillages;
		FactionSettingUIItem.onHoverOverEditVillages -= OnHoverOverEditFactionVillages;
		FactionSettingUIItem.onHoverOutEditVillages -= OnHoverOutEditFactionVillages;
		factionSettingVillageEditorUIController.onUIINstantiated -= OnFactionSettingVillageEditorInstantiated;
	}

	public void InitUI() {
		InstantiateUI();
		_chosenBiomes = new List<string>();
		m_worldGenOptionsUIView.InitializeMapSizeDropdown();
		m_worldGenOptionsUIView.InitializeBiomeItems();
		m_worldGenOptionsUIView.InitializeFactionItems();
		m_worldGenOptionsUIView.InitializeMigrationDropdown();
		m_worldGenOptionsUIView.InitializeVictoryConditionDropdown();
		m_worldGenOptionsUIView.InitializeCooldownDropdown();
		m_worldGenOptionsUIView.InitializeCostsDropdown();
		m_worldGenOptionsUIView.InitializeChargesDropdown();
		m_worldGenOptionsUIView.InitializeThreatDropdown();
		
		OnChangeMapSize(0);
		UpdateAddBiomeBtn();
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
		m_worldGenOptionsUIView.SetAddBiomeBtnState(_chosenBiomes.Count < WorldSettings.Instance.worldSettingsData.GetMaxBiomeCount());
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
	#endregion

	#region Faction Settings Item
	private void OnDeleteFactionSetting(FactionSetting p_factionSetting, FactionSettingUIItem p_uiItem) {
		WorldSettings.Instance.worldSettingsData.RemoveFactionSetting(p_factionSetting);
		m_worldGenOptionsUIView.HideFactionItem(p_uiItem);
		FactionEmblemRandomizer.SetEmblemAsUnUsed(p_factionSetting.factionEmblem);
		UpdateAddFactionBtn();
		UpdateVillageCount();
	}
	private void OnChangeFactionSettingName(FactionSetting p_factionSetting, string p_newName) {
		p_factionSetting.ChangeName(p_newName);
	}
	private void OnClickRandomizeFactionName(FactionSetting p_factionSetting, FactionSettingUIItem p_uiItem) {
		p_factionSetting.ChangeName(RandomNameGenerator.GenerateFactionName());
		p_uiItem.UpdateName(p_factionSetting.name);
	}
	private void OnChangeFactionSettingFactionType(FactionSetting p_factionSetting, FACTION_TYPE p_factionType) {
		p_factionSetting.ChangeFactionType(p_factionType);
	}
	private void OnClickEditFactionVillages(FactionSetting p_factionSetting) {
		factionSettingVillageEditorUIController.EditVillageSettings(p_factionSetting);
	}
	private void OnHoverOverEditFactionVillages(FactionSetting p_factionSetting) {
		Tooltip.Instance.ShowSmallInfo($"Edit {p_factionSetting.name}'s villages");
	}
	private void OnHoverOutEditFactionVillages(FactionSetting p_factionSetting) {
		Tooltip.Instance.HideSmallInfo();
	}
	private void UpdateAddFactionBtn() {
		m_worldGenOptionsUIView.SetAddFactionBtnState(!WorldSettings.Instance.worldSettingsData.HasReachedMaxFactionCount());
	}
	private void ResetFactions() {
		WorldSettings.Instance.worldSettingsData.ClearFactionSettings();
		m_worldGenOptionsUIView.ResetFactionItems();
		FactionEmblemRandomizer.Reset();
	}
	private void AddDefaultFactionSetting() {
		int maxFactions = WorldSettings.Instance.worldSettingsData.GetMaxFactions();
		int maxVillages = WorldSettings.Instance.worldSettingsData.GetMaxVillages();
		int dividedVillages = maxVillages / maxFactions;
		FactionSetting factionSetting = WorldSettings.Instance.worldSettingsData.AddFactionSetting(dividedVillages);
		Sprite factionEmblem = FactionEmblemRandomizer.GetUnusedFactionEmblem();
		factionSetting.SetFactionEmblem(factionEmblem);
		FactionEmblemRandomizer.SetEmblemAsUsed(factionEmblem);
		m_worldGenOptionsUIView.UpdateFactionItems(WorldSettings.Instance.worldSettingsData.factionSettings);
		UpdateAddFactionBtn();
		UpdateVillageCount();
	}
	private void UpdateVillageCount() {
		int maxVillages = WorldSettings.Instance.worldSettingsData.GetMaxVillages();
		int currentVillageCount = WorldSettings.Instance.worldSettingsData.GetCurrentVillageCount();
		m_worldGenOptionsUIView.UpdateVillageCount(currentVillageCount, maxVillages);
	}
	private void OnDoneEditingVillages() {
		UpdateVillageCount();
		m_worldGenOptionsUIView.UpdateFactionItems(WorldSettings.Instance.worldSettingsData.factionSettings);
	}
	#endregion

	#region WorldGenOptionsUIView.IListener Implementation
	public void OnChangeMapSize(MAP_SIZE p_value) {
		ResetBiomes();
		ResetFactions();
		WorldSettings.Instance.worldSettingsData.SetMapSize(p_value);
		int maxBiomes = WorldSettings.Instance.worldSettingsData.GetMaxBiomeCount();
		for (int i = 1; i < maxBiomes; i++) {
			AddDefaultBiomeItem();
		}
		UpdateAddBiomeBtn();
		
		//reset faction settings
		int maxFactions = WorldSettings.Instance.worldSettingsData.GetMaxFactions();
		for (int i = 0; i < maxFactions; i++) {
			AddDefaultFactionSetting();
		}
		m_worldGenOptionsUIView.UpdateFactionItems(WorldSettings.Instance.worldSettingsData.factionSettings);
		UpdateAddFactionBtn();
	}
	public void OnChangeMigrationSpeed(MIGRATION_SPEED p_value) {
		WorldSettings.Instance.worldSettingsData.SetMigrationSpeed(p_value);
	}
	public void OnChangeVictoryCondition(VICTORY_CONDITION p_value) {
		WorldSettings.Instance.worldSettingsData.SetVictoryCondition(p_value);
	}
	public void OnChangeSkillCooldownSpeed(SKILL_COOLDOWN_SPEED p_value) {
		WorldSettings.Instance.worldSettingsData.SetCooldownSpeed(p_value);
	}
	public void OnChangeSkillCostAmount(SKILL_COST_AMOUNT p_value) {
		WorldSettings.Instance.worldSettingsData.SetSkillCostAmount(p_value);
	}
	public void OnChangeSkillChargeAmount(SKILL_CHARGE_AMOUNT p_value) {
		WorldSettings.Instance.worldSettingsData.SetChargeAmount(p_value);
	}
	public void OnChangeThreatAmount(THREAT_AMOUNT p_value) {
		WorldSettings.Instance.worldSettingsData.SetThreatAmount(p_value);
	}
	public void OnClickAddBiome() {
		AddDefaultBiomeItem();
	}
	public void OnClickAddFaction() {
		AddDefaultFactionSetting();
	}
	#endregion
}