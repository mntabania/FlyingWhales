using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Generator.Map_Generation.Components;
using Inner_Maps;
using Scenario_Maps;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour {

    public static MapGenerator Instance;
    
    private void Awake() {
        Instance = this;
    }

    #region Random World
    internal IEnumerator InitializeWorld() {
        SaveManager.Instance.SetUseSaveData(false);
        DatabaseManager.Instance.mainSQLDatabase.InitializeDatabase(); //Initialize main SQL database
        MapGenerationComponent[] mapGenerationComponents = {
            new WorldMapGridGeneration(), new WorldMapElevationGeneration(), new SupportingFactionGeneration(), 
            new WorldMapRegionGeneration(), new WorldMapBiomeGeneration(), new WorldMapOuterGridGeneration(),
            new TileFeatureGeneration(), new RegionFeatureGeneration(), new WorldMapLandmarkGeneration(), 
            new FamilyTreeGeneration(), new RegionInnerMapGeneration(), new SettlementGeneration(), 
            new CharacterFinalization(), new LandmarkStructureGeneration(), new ElevationStructureGeneration(), 
            new RegionFeatureActivation(), new MonsterGeneration(), new MapGenerationFinalization(),
        };
        yield return StartCoroutine(InitializeWorldCoroutine(mapGenerationComponents));
    }
    private IEnumerator InitializeWorldCoroutine(MapGenerationComponent[] components) {
        Stopwatch loadingWatch = new Stopwatch();
        loadingWatch.Start();

        string loadingDetails = "Loading details";

        bool componentFailed = false;
        
        MapGenerationData data = new MapGenerationData();
        Stopwatch componentWatch = new Stopwatch();
        float progressPerComponent = 1f / components.Length;
        float currentProgress = 0f;
        for (int i = 0; i < components.Length; i++) {
            MapGenerationComponent currComponent = components[i];
            componentWatch.Start();
            currentProgress += progressPerComponent;
            LevelLoaderManager.Instance.UpdateLoadingBar(currentProgress, 2f);
            yield return StartCoroutine(currComponent.ExecuteRandomGeneration(data));
            componentWatch.Stop();
            loadingDetails += $"\n{currComponent.ToString()} took {componentWatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.";
            if (string.IsNullOrEmpty(currComponent.log) == false) {
                loadingDetails += $"\n{currComponent.log}";
            }
            componentWatch.Reset();
            
            componentFailed = currComponent.succeess == false;
            if (componentFailed) {
                break;
            }
        }
        componentWatch.Stop();
        if (componentFailed) {
            //reload scene
            Debug.LogWarning("A component in world generation failed! Reloading scene...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        } else {
            LevelLoaderManager.Instance.UpdateLoadingBar(1f, 0.5f);
            yield return new WaitForSeconds(0.5f);
            loadingWatch.Stop();
            Debug.Log($"{loadingDetails}\nTotal loading time is {loadingWatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds");

            for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
                Faction faction = FactionManager.Instance.allFactions[i];
                if (faction.isMajorNonPlayer) {
                    faction.DesignateNewLeader(false);
                    if (faction.leader is Character leader) {
                        FactionManager.Instance.RerollFactionLeaderTraitIdeology(faction, leader);    
                    }
                    faction.GenerateInitialOpinionBetweenMembers();
                }
            }
            UtilityScripts.LocationAwarenessUtility.UpdateAllPendingAwareness();
            //yield return StartCoroutine(LocationAwarenessUtility.UpdateAllPendingAwarenessThread());
            for (int j = 0; j < DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements.Count; j++) {
                NPCSettlement settlement = DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements[j];
                if (settlement.locationType == LOCATION_TYPE.VILLAGE) {
                    if(settlement.ruler == null) {
                        settlement.DesignateNewRuler(false);
                    }
                    settlement.GenerateInitialOpinionBetweenResidents();
                }
            }
            
            WorldConfigManager.Instance.mapGenerationData = data;
            AudioManager.Instance.TransitionToWorld();
            
            UIManager.Instance.initialWorldSetupMenu.Initialize();
            LevelLoaderManager.Instance.SetLoadingState(false);
            UIManager.Instance.initialWorldSetupMenu.Show();
            Messenger.Broadcast(Signals.GAME_LOADED);

            yield return new WaitForSeconds(1f);
        }
    }
    #endregion

    #region Scenario World
    public IEnumerator InitializeScenarioWorld(ScenarioMapData scenarioMapData) {
        SaveManager.Instance.SetUseSaveData(false);
        DatabaseManager.Instance.mainSQLDatabase.InitializeDatabase(); //Initialize main SQL database
        MapGenerationComponent[] mapGenerationComponents = {
            new WorldMapGridGeneration(), new SupportingFactionGeneration(), new WorldMapRegionGeneration(), 
            new WorldMapOuterGridGeneration(), new TileFeatureGeneration(), new RegionFeatureGeneration(), 
            //new PlayerSettlementGeneration(), 
            new WorldMapLandmarkGeneration(), new FamilyTreeGeneration(), 
            new RegionInnerMapGeneration(), new SettlementGeneration(), new CharacterFinalization(), new LandmarkStructureGeneration(), 
            new ElevationStructureGeneration(), new RegionFeatureActivation(), new MonsterGeneration(), 
            new FactionFinalization(), new MapGenerationFinalization(), 
            //new PlayerDataGeneration(),
        };
        yield return StartCoroutine(InitializeScenarioWorldCoroutine(mapGenerationComponents, scenarioMapData));
    }
    private IEnumerator InitializeScenarioWorldCoroutine(MapGenerationComponent[] components, ScenarioMapData scenarioMapData) {
        Stopwatch loadingWatch = new Stopwatch();
        loadingWatch.Start();
        string loadingDetails = "Loading details";

        bool componentFailed = false;
        
        MapGenerationData data = new MapGenerationData();
        Stopwatch componentWatch = new Stopwatch();
        float progressPerComponent = 1f / components.Length;
        float currentProgress = 0f;
        for (int i = 0; i < components.Length; i++) {
            MapGenerationComponent currComponent = components[i];
            componentWatch.Start();
            currentProgress += progressPerComponent;
            LevelLoaderManager.Instance.UpdateLoadingBar(currentProgress, 2f);
            yield return StartCoroutine(currComponent.LoadScenarioData(data, scenarioMapData));
            componentWatch.Stop();
            loadingDetails += $"\n{currComponent.ToString()} took {componentWatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.";
            if (string.IsNullOrEmpty(currComponent.log) == false) {
                loadingDetails += $"\n{currComponent.log}";
            }
            componentWatch.Reset();
            
            componentFailed = currComponent.succeess == false;
            if (componentFailed) {
                break;
            }
        }
        componentWatch.Stop();
        if (componentFailed) {
            //reload scene
            Debug.LogWarning("A component in world generation failed! Reloading scene...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        } else {
            LevelLoaderManager.Instance.UpdateLoadingBar(1f, 0.5f);
            yield return new WaitForSeconds(0.5f);
            loadingWatch.Stop();
            Debug.Log(
                $"{loadingDetails}\nTotal loading time is {loadingWatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds");

            for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
                Faction faction = FactionManager.Instance.allFactions[i];
                if (faction.isMajorNonPlayer) {
                    faction.DesignateNewLeader(false);
                    if (faction.leader is Character leader) {
                        FactionManager.Instance.RerollFactionLeaderTraitIdeology(faction, leader);    
                    }
                    faction.GenerateInitialOpinionBetweenMembers();
                }
            }

            UtilityScripts.LocationAwarenessUtility.UpdateAllPendingAwareness();
            //yield return StartCoroutine(LocationAwarenessUtility.UpdateAllPendingAwarenessThread());

            for (int j = 0; j < DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements.Count; j++) {
                NPCSettlement settlement = DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements[j];
                if (settlement.locationType == LOCATION_TYPE.VILLAGE) {
                    if(settlement.ruler == null) {
                        settlement.DesignateNewRuler(false);
                    }
                    settlement.GenerateInitialOpinionBetweenResidents();
                }
            }
            
            WorldConfigManager.Instance.mapGenerationData = data;
            // WorldMapCameraMove.Instance.CenterCameraOn(data.portal.gameObject);
            AudioManager.Instance.TransitionToWorld();
            
            UIManager.Instance.initialWorldSetupMenu.Initialize();
            LevelLoaderManager.Instance.SetLoadingState(false);
            UIManager.Instance.initialWorldSetupMenu.Show();
            Messenger.Broadcast(Signals.GAME_LOADED);
            // if (WorldConfigManager.Instance.isTutorialWorld) {
            //     Messenger.Broadcast(Signals.GAME_LOADED);
            //     UIManager.Instance.initialWorldSetupMenu.loadOutMenu.OnClickContinue();
            //     LevelLoaderManager.Instance.SetLoadingState(false);
            // } else {
            //     LevelLoaderManager.Instance.SetLoadingState(false);
            //     Messenger.Broadcast(Signals.GAME_LOADED);
            // }
            yield return new WaitForSeconds(1f);
        }
    }
    #endregion
    
    #region Saved World
    public IEnumerator InitializeSavedWorld(SaveDataCurrentProgress saveData) {
        //Note: In the Save World, the TileFeatureGeneration is done after the second wave is done loading because there are tile features thats needs the references when it is added
        //Example: The HeatWave feature function PopulateInitialCharactersOutside is called when it is added, inside the GetAllCharactersInsideHexThatMeetCriteria is called, where the innermaphextile is needed, so we must have the references before loading the tile features
        SaveManager.Instance.SetUseSaveData(true);
        WorldSettings.Instance.SetWorldSettingsData(saveData.worldSettingsData);
        DatabaseManager.Instance.mainSQLDatabase.InitializeDatabase(); //Initialize main SQL database
        MapGenerationComponent[] mapGenerationComponents = {
            new WorldMapGridGeneration(), new WorldMapRegionGeneration(),
            new WorldMapOuterGridGeneration(),
            new WorldMapLandmarkGeneration(), new SettlementLoading(), new FamilyTreeGeneration(),
            new RegionInnerMapGeneration(), new SingletonDataGeneration(),
            new LoadFirstWave(), new LoadSecondWave(), new TileFeatureGeneration(), new MapGenerationFinalization(),
            new PlayerDataGeneration(), new LoadAwarenessGeneration(), new LoadCharactersCurrentAction(), new LoadPlayerQuests(),
        };
        yield return StartCoroutine(InitializeSavedWorldCoroutine(mapGenerationComponents, saveData));
    }
    private IEnumerator InitializeSavedWorldCoroutine(MapGenerationComponent[] components, SaveDataCurrentProgress saveData) {
        Stopwatch loadingWatch = new Stopwatch();
        loadingWatch.Start();
        string loadingDetails = "Loading details";

        bool componentFailed = false;
        
        MapGenerationData data = new MapGenerationData();
        Stopwatch componentWatch = new Stopwatch();
        float progressPerComponent = 0.6f / components.Length;
        float currentProgress = 0.4f;
        for (int i = 0; i < components.Length; i++) {
            MapGenerationComponent currComponent = components[i];
            componentWatch.Start();
            currentProgress += progressPerComponent;
            LevelLoaderManager.Instance.UpdateLoadingBar(currentProgress, 2f);
            yield return StartCoroutine(currComponent.LoadSavedData(data, saveData));
            componentWatch.Stop();
            loadingDetails += $"\n{currComponent.ToString()} took {componentWatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.";
            if (string.IsNullOrEmpty(currComponent.log) == false) {
                loadingDetails += $"\n{currComponent.log}";
            }
            componentWatch.Reset();
            
            componentFailed = currComponent.succeess == false;
            if (componentFailed) {
                break;
            }
        }
        componentWatch.Stop();
        if (componentFailed) {
            //reload scene
            Debug.LogWarning("A component in world generation failed! Reloading scene...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        } else {
            LevelLoaderManager.Instance.UpdateLoadingBar(1f, 0.5f);
            yield return new WaitForSeconds(0.5f);
            loadingWatch.Stop();
            Debug.Log($"{loadingDetails}\nTotal loading time is {loadingWatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds");
            
            WorldConfigManager.Instance.mapGenerationData = data;
            AudioManager.Instance.TransitionToWorld();
            
            UIManager.Instance.initialWorldSetupMenu.Initialize();

            Messenger.Broadcast(Signals.GAME_LOADED);
            UIManager.Instance.initialWorldSetupMenu.loadOutMenu.LoadLoadout(saveData.playerSave.archetype);

            DatabaseManager.Instance.ClearVolatileDatabases();
            SaveManager.Instance.saveCurrentProgressManager.CleanUpLoadedData();
            GC.Collect();
            var unloader = Resources.UnloadUnusedAssets();
            while (!unloader.isDone) {
                yield return null;
            }
            yield return new WaitForSeconds(1f);
            LevelLoaderManager.Instance.SetLoadingState(false);
        }
    }
    #endregion
}
