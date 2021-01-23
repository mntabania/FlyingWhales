using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ruinarch.Custom_UI;
using TMPro;

public class WorldSettings : MonoBehaviour {
    public static WorldSettings Instance;

    public WorldSettingsData worldSettingsData { get; private set; }

    public GameObject settingsGO;

    public RuinarchToggle defaultWorldToggle;

    public GameObject mainWindow;
    public WorldGenOptionsUIController worldGenOptionsUIController;
    public Button btnContinue;

    public GameObject hoverGO;
    public RuinarchText hoverText;
    public RuinarchText hoverTitle;
    public WorldPickerItem[] worldPickerItems;
    private WorldPickerItem toggledWorldPicker;
    public Transform parentDisplay;

    [Header("Monster Migration")]
    public MonsterMigrationBiomeDictionary monsterMigrationBiomeDictionary;

    [Header("Scenarios")]
    public ScenarioSettingsDataDictionary scenarioSettingsDictionary;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            worldSettingsData = new WorldSettingsData();
            worldGenOptionsUIController.InitUI(OnUpdateVillageCount);
            worldGenOptionsUIController.HideUI();
            worldGenOptionsUIController.SetParent(parentDisplay);
        } else {
            Destroy(this.gameObject);
        }
    }

    #region General
    public void Open() {
        settingsGO.SetActive(true);
        mainWindow.SetActive(true);
        InitializeData();
        worldGenOptionsUIController.HideUI();
        UpdateAvailableWorldTypes();
        UpdateContinueBtnInteractable();
    }
    public void Close() {
        settingsGO.SetActive(false);
    }
    private void InitializeData() {
        defaultWorldToggle.isOn = true;
    }
    private void UpdateBiomes(BIOMES biome, bool state) {
        if (state) {
            worldSettingsData.mapSettings.AddBiome(biome);
        } else {
            worldSettingsData.mapSettings.RemoveBiome(biome);
        }
    }
    public void SetWorldSettingsData(WorldSettingsData data) {
        worldSettingsData = data;
    }
    private void OnUpdateVillageCount() {
        UpdateContinueBtnInteractable();
    }
    private void UpdateContinueBtnInteractable() {
        if (worldGenOptionsUIController.IsUIShowing()) {
            btnContinue.interactable = worldSettingsData.AreSettingsValid(out var invalidityReason);    
        } else {
            btnContinue.interactable = true;
        }
        
    }
    #endregion

    #region UI References
    public void OnClickContinue() {
        if (mainWindow.activeSelf) {
            //Still in world picker
            if(toggledWorldPicker.worldType == WorldSettingsData.World_Type.Custom) {
                //show custom map customizer
                mainWindow.SetActive(false);
                worldGenOptionsUIController.ShowUI();
            } else {
                //load scenario map
                worldSettingsData.ApplySettingsBasedOnScenarioType(toggledWorldPicker.worldType);
                Close();
                MainMenuManager.Instance.StartGame();
            }
        } else if (worldGenOptionsUIController.IsUIShowing()) {
            worldSettingsData.SetWorldType(WorldSettingsData.World_Type.Custom);
            worldGenOptionsUIController.ApplyCurrentSettingsToData();
            worldSettingsData.ApplyCustomWorldSettings();
            //Already in customize window
            if (worldSettingsData.AreSettingsValid(out var invalidityReason)) {
                //Generate Custom Map
                Close();
                MainMenuManager.Instance.StartGame();
            } else {
                MainMenuUI.Instance.generalConfirmation.ShowGeneralConfirmation("Invalid Settings", UtilityScripts.Utilities.InvalidColorize(invalidityReason));
            }
        }
        UpdateContinueBtnInteractable();
    }
    public void OnClickBack() {
        if (mainWindow.activeSelf) {
            Close();
        } else if (worldGenOptionsUIController.IsUIShowing()) {
            mainWindow.SetActive(true);
            worldGenOptionsUIController.HideUI();
        }
        UpdateContinueBtnInteractable();
    }
    #endregion

    #region World Picker
    private void UpdateAvailableWorldTypes() {
        for (int i = 0; i < worldPickerItems.Length; i++) {
            WorldPickerItem worldPickerItem = worldPickerItems[i];
            if (SaveManager.Instance.currentSaveDataPlayer.IsWorldUnlocked(worldPickerItem.worldType) ||
                SaveManager.Instance.unlockAllWorlds) {
                worldPickerItem.Enable();
            } else {
                worldPickerItem.Disable();
            }
        }
    }
    public void OnHoverEnterWorldPicker(WorldPickerItem item) {
        ShowHover(UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(item.worldType.ToString()), item.description);
    }
    public void OnHoverExitWorldPicker(WorldPickerItem item) {
        if(toggledWorldPicker != null && toggledWorldPicker.description != string.Empty) {
            ShowHover(UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(toggledWorldPicker.worldType.ToString()), toggledWorldPicker.description);
        } else {
            HideHover();
        }
    }
    public void OnToggleWorldPicker(WorldPickerItem item, bool state) {
        if (state) {
            toggledWorldPicker = item;
            ShowHover(item.worldType.ToString(), item.description);
        }
    }
    public void ShowHover(string title, string text) {
        if(title != string.Empty && text != string.Empty) {
            hoverTitle.text = title;
            hoverText.text = text;
            hoverGO.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(hoverGO.transform as RectTransform);
        }
    }
    public void HideHover() {
        hoverGO.SetActive(false);
    }
    #endregion

    #region Monster Migration
    public MonsterMigrationBiomeData GetMonsterMigrationBiomeDataByBiomeType(BIOMES p_biomeType) {
        if (monsterMigrationBiomeDictionary.ContainsKey(p_biomeType)) {
            return monsterMigrationBiomeDictionary[p_biomeType];
        }
        return null;
    }
    #endregion

    #region Scenarios
    public ScenarioData GetScenarioDataByWorldType(WorldSettingsData.World_Type p_worldType) {
        if (scenarioSettingsDictionary.ContainsKey(p_worldType)) {
            return scenarioSettingsDictionary[p_worldType];
        }
        return null;
    }
    #endregion
}
