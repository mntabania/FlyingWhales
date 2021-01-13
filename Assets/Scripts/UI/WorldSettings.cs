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

    public RuinarchToggle defaultRegionToggle;
    public RuinarchToggle[] racesToggles;
    public RuinarchToggle[] biomesToggles;

    public RuinarchToggle omnipotentModeToggle;
    public RuinarchToggle noThreatModeToggle;
    public RuinarchToggle chaosVictoryModeToggle;

    public RuinarchToggle defaultWorldToggle;

    public GameObject mainWindow;
    public WorldGenOptionsUIController worldGenOptionsUIController;

    public GameObject hoverGO;
    public RuinarchText hoverText;
    public RuinarchText hoverTitle;
    public WorldPickerItem[] worldPickerItems;
    private WorldPickerItem toggledWorldPicker;
    public Transform parentDisplay;
    

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            worldSettingsData = new WorldSettingsData();
            worldGenOptionsUIController.InitUI();
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
    }
    public void Close() {
        settingsGO.SetActive(false);
    }
    private void InitializeData() {
        defaultWorldToggle.isOn = true;
    }
    private void InitializeCustomUI() {
        //if (raceWorldOptionItems.Count <= 0) {
        //    worldSettingsData.ClearRaces();
        //    PopulateRacesAndToggleOn();
        //} else {
        //    ToggleAllRaces(true);
        //}
        //if (biomeWorldOptionItems.Count <= 0) {
        //    worldSettingsData.ClearBiomes();
        //    PopulateBiomesAndToggleOn();
        //} else {
        //    ToggleAllBiomes(true);
        //}

        ToggleAllRaces(true);
        ToggleAllBiomes(true);

        //PopulateNumOfRegions();
        //numOfRegionsDropdown.value = 2;
        defaultRegionToggle.isOn = true;

        omnipotentModeToggle.isOn = false;
        noThreatModeToggle.isOn = false;
        chaosVictoryModeToggle.isOn = false;
    }
    private void ToggleAllRaces(bool state) {
        for (int i = 0; i < racesToggles.Length; i++) {
            racesToggles[i].isOn = state;
        }
    }
    private void ToggleAllBiomes(bool state) {
        for (int i = 0; i < biomesToggles.Length; i++) {
            biomesToggles[i].isOn = state;
        }
    }
    private void UpdateBiomes(BIOMES biome, bool state) {
        if (state) {
            worldSettingsData.AddBiome(biome);
        } else {
            worldSettingsData.RemoveBiome(biome);
        }
    }
    public void SetWorldSettingsData(WorldSettingsData data) {
        worldSettingsData = data;
    }
    #endregion

    #region UI References
    // public void OnToggleOmnipotentMode(bool state) {
    //     worldSettingsData.SetOmnipotentMode(state);
    // }
    // public void OnToggleNoThreatMode(bool state) {
    //     worldSettingsData.SetNoThreatMode(state);
    // }
    public void OnClickContinue() {
        if (mainWindow.activeSelf) {
            //Still in world picker
            if(toggledWorldPicker.worldType == WorldSettingsData.World_Type.Custom) {
                //show custom map customizer
                mainWindow.SetActive(false);
                worldGenOptionsUIController.ShowUI();
                InitializeCustomUI();
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
    }
    public void OnClickBack() {
        if (mainWindow.activeSelf) {
            Close();
        } else if (worldGenOptionsUIController.IsUIShowing()) {
            mainWindow.SetActive(true);
            worldGenOptionsUIController.HideUI();
        }
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
}
