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

    //public GameObject raceWorldOptionItemPrefab;
    //public GameObject biomeWorldOptionItemPrefab;

    public RuinarchToggle defaultRegionToggle;
    public RuinarchToggle[] racesToggles;
    public RuinarchToggle[] biomesToggles;

    //public ScrollRect racesScrollRect;
    //public ScrollRect biomesScrollRect;

    public RuinarchToggle omnipotentModeToggle;
    public RuinarchToggle noThreatModeToggle;
    public RuinarchToggle chaosVictoryModeToggle;

    public GameObject invalidMessage;

    //private List<RaceWorldOptionItem> raceWorldOptionItems;
    //private List<BiomeWorldOptionItem> biomeWorldOptionItems;
    //private List<string> numOfRegions;

    //private RACE[] races = { RACE.HUMANS, RACE.ELVES };
    //private BIOMES[] biomes = { BIOMES.GRASSLAND, BIOMES.FOREST, BIOMES.SNOW, BIOMES.DESERT };

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
        //raceWorldOptionItems = new List<RaceWorldOptionItem>();
        //biomeWorldOptionItems = new List<BiomeWorldOptionItem>();
        //numOfRegions = new List<string>() { "1", "2", "3", "4", "6" };
        //Messenger.AddListener<RACE, bool>(Signals.RACE_WORLD_OPTION_ITEM_CLICKED, OnRaceWorldOptionItemClicked);
        //Messenger.AddListener<BIOMES, bool>(Signals.BIOME_WORLD_OPTION_ITEM_CLICKED, OnBiomeWorldOptionItemClicked);
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
        settingsGO.SetActive(true);
        InitializeData();
    }
    public void Close() {
        settingsGO.SetActive(false);
    }
    private void InitializeData() {
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
    //private void PopulateNumOfRegions() {
    //    numOfRegionsDropdown.ClearOptions();
    //    numOfRegionsDropdown.AddOptions(numOfRegions);
    //}
    //private void PopulateRacesAndToggleOn() {
    //    for (int i = 0; i < races.Length; i++) {
    //        if(races[i] != RACE.NONE) {
    //            RaceWorldOptionItem item = CreateNewRaceWorldOptionItem(races[i]);
    //            item.toggle.isOn = true;
    //        }
    //    }
    //}
    private void ToggleAllRaces(bool state) {
        //for (int i = 0; i < raceWorldOptionItems.Count; i++) {
        //    raceWorldOptionItems[i].toggle.isOn = state;
        //}
        for (int i = 0; i < racesToggles.Length; i++) {
            racesToggles[i].isOn = state;
        }
    }
    //private void PopulateBiomesAndToggleOn() {
    //    for (int i = 0; i < biomes.Length; i++) {
    //        if (biomes[i] != BIOMES.NONE) {
    //            BiomeWorldOptionItem item = CreateNewBiomeWorldOptionItem(biomes[i]);
    //            item.toggle.isOn = true;
    //        }
    //    }
    //}
    private void ToggleAllBiomes(bool state) {
        //for (int i = 0; i < biomeWorldOptionItems.Count; i++) {
        //    biomeWorldOptionItems[i].toggle.isOn = state;
        //}
        for (int i = 0; i < biomesToggles.Length; i++) {
            biomesToggles[i].isOn = state;
        }
    }

    //private RaceWorldOptionItem CreateNewRaceWorldOptionItem(RACE race) {
    //    GameObject go = Instantiate(raceWorldOptionItemPrefab, racesScrollRect.content);
    //    go.transform.localPosition = Vector3.zero;
    //    RaceWorldOptionItem item = go.GetComponent<RaceWorldOptionItem>();
    //    item.SetRace(race);
    //    raceWorldOptionItems.Add(item);
    //    return item;
    //}
    //private BiomeWorldOptionItem CreateNewBiomeWorldOptionItem(BIOMES biome) {
    //    GameObject go = Instantiate(biomeWorldOptionItemPrefab, biomesScrollRect.content);
    //    go.transform.localPosition = Vector3.zero;
    //    BiomeWorldOptionItem item = go.GetComponent<BiomeWorldOptionItem>();
    //    item.SetBiome(biome);
    //    biomeWorldOptionItems.Add(item);
    //    return item;
    //}
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
    #region Number of Regions
    public void OnToggleRegion1(bool state) {
        if (state) {
            worldSettingsData.SetNumOfRegions(1);
        }
    }
    public void OnToggleRegion2(bool state) {
        if (state) {
            worldSettingsData.SetNumOfRegions(2);
        }
    }
    public void OnToggleRegion3(bool state) {
        if (state) {
            worldSettingsData.SetNumOfRegions(3);
        }
    }
    public void OnToggleRegion4(bool state) {
        if (state) {
            worldSettingsData.SetNumOfRegions(4);
        }
    }
    public void OnToggleRegion6(bool state) {
        if (state) {
            worldSettingsData.SetNumOfRegions(6);
        }
    }
    #endregion

    #region Races
    public void OnToggleHumans(bool state) {
        UpdateRaces(RACE.HUMANS, state);
    }
    public void OnToggleElves(bool state) {
        UpdateRaces(RACE.ELVES, state);
    }
    #endregion

    #region Biomes
    public void OnToggleDesert(bool state) {
        UpdateBiomes(BIOMES.DESERT, state);
    }
    public void OnToggleSnow(bool state) {
        UpdateBiomes(BIOMES.SNOW, state);
    }
    public void OnToggleGrassland(bool state) {
        UpdateBiomes(BIOMES.GRASSLAND, state);
    }
    public void OnToggleForest(bool state) {
        UpdateBiomes(BIOMES.FOREST, state);
    }
    #endregion

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
        if (worldSettingsData.AreSettingsValid()) {
            Close();
            MainMenuManager.Instance.StartNewGame();    
        } else {
            //show invalid message
            invalidMessage.gameObject.SetActive(true);
        }
    }
    #endregion
}
