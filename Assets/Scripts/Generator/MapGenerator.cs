﻿using System;
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

public class MapGenerator : BaseMonoBehaviour {

    public static MapGenerator Instance;
    
    private void Awake() {
        Instance = this;
    }

    #region Random World
    internal IEnumerator InitializeWorld() {
        SaveManager.Instance.SetUseSaveData(false);
        DatabaseManager.Instance.mainSQLDatabase.InitializeDatabase(); //Initialize main SQL database
        MapGenerationComponent[] mapGenerationComponents = {
            new AreaGeneration(), /*new ElevationGeneration(),*/ new SupportingFactionGeneration(), 
            new WorldMapRegionGeneration(), /*new WorldMapBiomeGeneration(),*/ new FamilyTreeGeneration(), new RegionInnerMapGeneration(), /*new ElevationStructureGeneration(),*/ 
            new TileFeatureGeneration(), new RegionFeatureGeneration(), new VillageGeneration(),  new SpecialStructureGeneration(),
            new FactionFinalization(), new CharacterFinalization(), new SettlementFinalization(), new FeaturesActivation(), 
            new MonsterGeneration(), new MapGenerationFinalization(),
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

            WorldConfigManager.Instance.mapGenerationData = data;
            AudioManager.Instance.TransitionToWorld();
            
            UIManager.Instance.initialWorldSetupMenu.Initialize();
            LevelLoaderManager.Instance.SetLoadingState(false);
            Messenger.Broadcast(Signals.GAME_LOADED);
            UIManager.Instance.initialWorldSetupMenu.Show();
            yield return new WaitForSeconds(1f);
        }
    }
    #endregion

    #region Scenario World
    public IEnumerator InitializeScenarioWorld(ScenarioMapData scenarioMapData) {
        SaveManager.Instance.SetUseSaveData(false);
        DatabaseManager.Instance.mainSQLDatabase.InitializeDatabase(); //Initialize main SQL database
        MapGenerationComponent[] mapGenerationComponents = {
            new AreaGeneration(), new SupportingFactionGeneration(), new WorldMapRegionGeneration(), new TileFeatureGeneration(), new RegionFeatureGeneration(), 
            new FamilyTreeGeneration(), new RegionInnerMapGeneration(), new VillageGeneration(), new SpecialStructureGeneration(), new FactionFinalization(), 
            new CharacterFinalization(), new ElevationStructureGeneration(), new SettlementFinalization(), new FeaturesActivation(), new MonsterGeneration(), 
            new MapGenerationFinalization(), 
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
            Debug.Log($"{loadingDetails}\nTotal loading time is {loadingWatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds");

            WorldConfigManager.Instance.mapGenerationData = data;
            AudioManager.Instance.TransitionToWorld();
            
            UIManager.Instance.initialWorldSetupMenu.Initialize();
            LevelLoaderManager.Instance.SetLoadingState(false);
            Messenger.Broadcast(Signals.GAME_LOADED);
            UIManager.Instance.initialWorldSetupMenu.Show();
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
            new AreaGeneration(), new WorldMapRegionGeneration(), new SettlementLoading(), new FamilyTreeGeneration(),
            new RegionInnerMapGeneration(), new SingletonDataGeneration(), new PlayerDataGeneration(),
            new LoadFirstWave(), new LoadSecondWave(), new TileFeatureGeneration(), new MapGenerationFinalization(),
            new LoadCharactersCurrentAction(), new LoadPlayerQuests(),
            /*, new LoadAwarenessGeneration()*/
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

    protected override void OnDestroy() {
        base.OnDestroy();
        Instance = null;
    }
}
