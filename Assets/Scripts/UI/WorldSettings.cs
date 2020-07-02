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

    public GameObject raceWorldOptionItemPrefab;
    public GameObject biomeWorldOptionItemPrefab;

    public TMP_Dropdown numOfRegionsDropdown;

    public ScrollRect racesScrollRect;
    public ScrollRect biomesScrollRect;

    public RuinarchToggle omnipotentModeToggle;
    public RuinarchToggle noThreatModeToggle;
    public RuinarchToggle chaosVictoryModeToggle;

    private List<RaceWorldOptionItem> raceWorldOptionItems;
    private List<BiomeWorldOptionItem> biomeWorldOptionItems;
    private List<string> numOfRegions;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        } else {
            Destroy(this.gameObject);
        }
    }
    private void Start() {
        worldSettingsData = new WorldSettingsData();
        raceWorldOptionItems = new List<RaceWorldOptionItem>();
        biomeWorldOptionItems = new List<BiomeWorldOptionItem>();
        numOfRegions = new List<string>() { "1", "2", "3", "4", "6" };
        Messenger.AddListener<RACE, bool>(Signals.RACE_WORLD_OPTION_ITEM_CLICKED, OnRaceWorldOptionItemClicked);
        Messenger.AddListener<BIOMES, bool>(Signals.BIOME_WORLD_OPTION_ITEM_CLICKED, OnBiomeWorldOptionItemClicked);
    }

    #region Listeners
    private void OnRaceWorldOptionItemClicked(RACE race, bool state) {
        UpdateRaces(race, state);
    }
    private void OnBiomeWorldOptionItemClicked(BIOMES biome, bool state) {
        UpdateBiomes(biome, state);
    }
    #endregion

    #region General
    public void Open() {
        InitializeData();
        settingsGO.SetActive(true);
    }
    public void Close() {
        settingsGO.SetActive(false);
    }
    private void InitializeData() {
        if (raceWorldOptionItems.Count <= 0) {
            worldSettingsData.ClearRaces();
            PopulateRacesAndToggleOn();
        } else {
            ToggleAllRaces(true);
        }
        if (biomeWorldOptionItems.Count <= 0) {
            worldSettingsData.ClearBiomes();
            PopulateBiomesAndToggleOn();
        } else {
            ToggleAllBiomes(true);
        }

        PopulateNumOfRegions();
        numOfRegionsDropdown.value = 2;

        omnipotentModeToggle.isOn = false;
        noThreatModeToggle.isOn = false;
        chaosVictoryModeToggle.isOn = false;
    }
    private void PopulateNumOfRegions() {
        numOfRegionsDropdown.ClearOptions();
        numOfRegionsDropdown.AddOptions(numOfRegions);
    }
    private void PopulateRacesAndToggleOn() {
        RACE[] races = (RACE[]) System.Enum.GetValues(typeof(RACE));
        for (int i = 0; i < races.Length; i++) {
            if(races[i] != RACE.NONE) {
                RaceWorldOptionItem item = CreateNewRaceWorldOptionItem(races[i]);
                item.toggle.isOn = true;
            }
        }
    }
    private void ToggleAllRaces(bool state) {
        for (int i = 0; i < raceWorldOptionItems.Count; i++) {
            raceWorldOptionItems[i].toggle.isOn = state;
        }
    }
    private void PopulateBiomesAndToggleOn() {
        BIOMES[] biomes = (BIOMES[]) System.Enum.GetValues(typeof(BIOMES));
        for (int i = 0; i < biomes.Length; i++) {
            if (biomes[i] != BIOMES.NONE) {
                BiomeWorldOptionItem item = CreateNewBiomeWorldOptionItem(biomes[i]);
                item.toggle.isOn = true;
            }
        }
    }
    private void ToggleAllBiomes(bool state) {
        for (int i = 0; i < biomeWorldOptionItems.Count; i++) {
            biomeWorldOptionItems[i].toggle.isOn = state;
        }
    }

    private RaceWorldOptionItem CreateNewRaceWorldOptionItem(RACE race) {
        GameObject go = Instantiate(raceWorldOptionItemPrefab, racesScrollRect.content);
        go.transform.localPosition = Vector3.zero;
        RaceWorldOptionItem item = go.GetComponent<RaceWorldOptionItem>();
        item.SetRace(race);
        raceWorldOptionItems.Add(item);
        return item;
    }
    private BiomeWorldOptionItem CreateNewBiomeWorldOptionItem(BIOMES biome) {
        GameObject go = Instantiate(biomeWorldOptionItemPrefab, biomesScrollRect.content);
        go.transform.localPosition = Vector3.zero;
        BiomeWorldOptionItem item = go.GetComponent<BiomeWorldOptionItem>();
        item.SetBiome(biome);
        biomeWorldOptionItems.Add(item);
        return item;
    }
    private void UpdateRaces(RACE race, bool state) {
        if (state) {
            worldSettingsData.AddRace(race);
        } else {
            worldSettingsData.RemoveRace(race);
        }
    }
    private void UpdateBiomes(BIOMES biome, bool state) {
        if (state) {
            worldSettingsData.AddBiome(biome);
        } else {
            worldSettingsData.RemoveBiome(biome);
        }
    }
    #endregion

    #region UI References
    public void OnNumOfRegionsDropdownValueChange(int index) {
        worldSettingsData.SetNumOfRegions(int.Parse(numOfRegionsDropdown.options[index].text));
    }
    public void OnToggleOmnipotentMode(bool state) {
        worldSettingsData.SetOmnipotentMode(state);
    }
    public void OnToggleNoThreatMode(bool state) {
        worldSettingsData.SetNoThreatMode(state);
    }
    public void OnToggleChaosVictoryMode(bool state) {
        worldSettingsData.SetChaosVictoryMode(state);
    }
    public void OnClickContinue() {
        Close();
        MainMenuManager.Instance.StartNewGame();
    }
    #endregion
}
