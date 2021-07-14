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
    public GameObject subHoverGO;
    public RuinarchText hoverText;
    public RuinarchText hoverTitle;
    public RuinarchText subHoverText;
    public RuinarchText subHoverTitle;
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
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                worldPickerItem.Enable(); //Enable for now for testing
#else
                worldPickerItem.Disable();
#endif
            } else {
                worldPickerItem.Disable();
            }
        }
        //enable custom
        worldPickerItems[0].Enable();
        worldPickerItems[0].toggle.isOn = true;
        // worldPickerItems[0].OnToggle(true);
    }
    public void OnHoverEnterWorldPicker(WorldPickerItem item) {
        ShowHover(UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(item.worldType.ToString()), item.description, item.isScenario);
    }
    public void OnHoverExitWorldPicker(WorldPickerItem item) {
        if(toggledWorldPicker != null && toggledWorldPicker.description != string.Empty) {
            ShowHover(UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(toggledWorldPicker.worldType.ToString()), toggledWorldPicker.description, toggledWorldPicker.isScenario);
        } else {
            HideHover();
        }
    }
    public void OnToggleWorldPicker(WorldPickerItem item, bool state) {
        if (state) {
            toggledWorldPicker = item;
            ShowHover(item.worldType.ToString(), item.description, item.isScenario);
        }
    }
    public void ShowHover(string title, string text, bool p_isScenario) {
        if (title != string.Empty && text != string.Empty) {
            if (p_isScenario) {
                subHoverTitle.text = title;
                subHoverText.text = text;
                hoverTitle.text = "Scenarios\n";
                //hoverText.text = "Scenarios are shorter games with a variety of different situations and victory conditions. Unlike Custom, you will start with a bigger loadout with some optional configurable slots.\n\nHowever, you don't get to upgrade the Portal, so you won't get permanent access to more Powers. Unleash Power is still available in the Portal so you can still get Bonus Charges.";
                hoverText.text = "Scenarios have been temporarily disabled while we rework them to adapt to new features. These will be back soon!";
                hoverText.color = Color.red;
                subHoverGO.SetActive(false);
                LayoutRebuilder.ForceRebuildLayoutImmediate(subHoverGO.transform as RectTransform);
            } else {
                subHoverGO.SetActive(false);
                
                hoverTitle.text = "Custom Game\n";
                hoverText.text = "Configure game settings and generate a random world.";
                hoverText.color = new Color(248f, 225f, 169f);
            }
            hoverGO.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(hoverGO.transform as RectTransform);
        } else {
            HideHover();
        }
    }
    public void HideHover() {
        hoverGO.SetActive(false);
        subHoverGO.SetActive(false);
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
