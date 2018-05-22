﻿using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;

public class MapGenerator : MonoBehaviour {

    public static MapGenerator Instance = null;

    private void Awake() {
        Instance = this;
    }

    internal void InitializeWorld() {
        System.Diagnostics.Stopwatch loadingWatch = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
        loadingWatch.Start();

        //LevelLoaderManager.Instance.UpdateLoadingInfo("Generating Map...");

        GridMap.Instance.GenerateGrid();
        CameraMove.Instance.CalculateCameraBounds();
        Minimap.Instance.Initialize();
        ObjectPoolManager.Instance.InitializeObjectPools();
        CameraMove.Instance.SetWholemapCameraValues();
        EquatorGenerator.Instance.GenerateEquator();
        Biomes.Instance.GenerateElevation();

        //LevelLoaderManager.Instance.UpdateLoadingInfo("Generating Biomes...");

        Biomes.Instance.GenerateBiome();
        Biomes.Instance.LoadPassableObjects(GridMap.Instance.hexTiles, GridMap.Instance.outerGridList);

        //LevelLoaderManager.Instance.UpdateLoadingInfo("Generating Regions...");

        st.Start();
        GridMap.Instance.GenerateRegions(GridMap.Instance.numOfRegions, GridMap.Instance.refinementLevel);
        st.Stop();

        //if (regionGenerationFailed) {
        //    //Debug.LogWarning("Region generation ran into a problem, reloading scene...");
        //    Messenger.Cleanup();
        //    ReloadScene();
        //    return;
        //} else {
        //    Debug.Log(string.Format("Region Generation took {0} ms to complete", st.ElapsedMilliseconds));
        //}

        //Biomes.Instance.UpdateTileVisuals();
        //Biomes.Instance.GenerateTileBiomeDetails();
        //return;

        //st.Start();
        //Biomes.Instance.DetermineIslands();
        //st.Stop();
        //Debug.Log(string.Format("Island Connections took {0} ms to complete", st.ElapsedMilliseconds));

        //RoadManager.Instance.FlattenRoads();
        //Biomes.Instance.LoadElevationSprites();
        //Biomes.Instance.GenerateTileBiomeDetails();

        //return;
        RoadManager.Instance.GenerateTilePassableTypes();
        //GridMap.Instance.BottleneckBorders();

        GridMap.Instance.GenerateOuterGrid();
        GridMap.Instance.DivideOuterGridRegions();

        UIManager.Instance.InitializeUI();
        ObjectManager.Instance.Initialize();

        //LevelLoaderManager.Instance.UpdateLoadingInfo("Generating Factions...");

        Region playerRegion = null;
        st.Start();
        FactionManager.Instance.GenerateInitialFactions(ref playerRegion);
        st.Stop();

        //if (factionGenerationFailed) {
        //    //reset
        //    Debug.LogWarning("Faction generation ran into a problem, reloading scene...");
        //    Messenger.Cleanup();
        //    ReloadScene();
        //    return;
        //} else {
        //    Debug.Log(string.Format("Faction Generation took {0} ms to complete", st.ElapsedMilliseconds));
        //}

        //st.Start();
        //bool landmarkGenerationFailed = !LandmarkManager.Instance.GenerateLandmarks();
        //st.Stop();

        //LevelLoaderManager.Instance.UpdateLoadingInfo("Generating Landmarks...");
        st.Start();
        LandmarkManager.Instance.GenerateFactionLandmarks();
        st.Stop();
        //if (landmarkGenerationFailed) {
        //    //reset
        //    Debug.LogWarning("Landmark generation ran into a problem, reloading scene...");
        //    Messenger.Cleanup();
        //    ReloadScene();
        //    return;
        //} else {
        //    Debug.Log(string.Format("Landmark Generation took {0} ms to complete", st.ElapsedMilliseconds));
        //}

        //st.Start();
        //bool roadGenerationFailed = !RoadManager.Instance.GenerateRoads();
        //st.Stop();

        //if (roadGenerationFailed) {
        //    //reset
        //    Debug.LogWarning("Road generation ran into a problem, reloading scene...");
        //    Messenger.Cleanup();
        //    ReloadScene();
        //    return;
        //} else {
        //    Debug.Log(string.Format("Road Generation took {0} ms to complete", st.ElapsedMilliseconds));
        //}
        LandmarkManager.Instance.GeneratePlayerLandmarks(playerRegion);
        PathfindingManager.Instance.CreateGrid();

        //FactionManager.Instance.OccupyLandmarksInFactionRegions();
        //ObjectManager.Instance.Initialize();
        //LandmarkManager.Instance.ConstructAllLandmarkObjects();

        //LandmarkManager.Instance.GenerateMaterials();

        //RoadManager.Instance.FlattenRoads();
        //Biomes.Instance.GenerateTileTags();
        //GridMap.Instance.GenerateNeighboursWithSameTag();
        Biomes.Instance.UpdateTileVisuals(GridMap.Instance.allTiles);
        Biomes.Instance.GenerateTileBiomeDetails(GridMap.Instance.hexTiles);
        

        GameManager.Instance.StartProgression();
        LandmarkManager.Instance.InitializeLandmarks();
        //CharacterManager.Instance.GenerateCharactersForTesting(1);
        //FactionManager.Instance.GenerateFactionCharacters();
        //FactionManager.Instance.GenerateMonsters();
        //StorylineManager.Instance.GenerateStoryLines();
        //CharacterManager.Instance.SchedulePrisonerConversion();
        //CameraMove.Instance.CenterCameraOn(FactionManager.Instance.allTribes.FirstOrDefault().settlements.FirstOrDefault().tileLocation.gameObject);
        CameraMove.Instance.UpdateMinimapTexture();
        loadingWatch.Stop();
        Debug.Log(string.Format("Total loading time is {0} ms", loadingWatch.ElapsedMilliseconds));
    }

    internal void LoadWorld(Save save) {
        System.Diagnostics.Stopwatch loadingWatch = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
        loadingWatch.Start();

        GridMap.Instance.GenerateGrid();
        CameraMove.Instance.CalculateCameraBounds();
        Minimap.Instance.Initialize();
        ObjectPoolManager.Instance.InitializeObjectPools();
        CameraMove.Instance.SetWholemapCameraValues();
        EquatorGenerator.Instance.GenerateEquator();
        Biomes.Instance.GenerateElevation();
        Biomes.Instance.GenerateBiome();
        Biomes.Instance.LoadPassableObjects(GridMap.Instance.hexTiles, GridMap.Instance.outerGridList);

        st.Start();
        GridMap.Instance.GenerateRegions(GridMap.Instance.numOfRegions, GridMap.Instance.refinementLevel);
        st.Stop();

        //if (regionGenerationFailed) {
        //    Debug.LogWarning("Region generation ran into a problem, reloading scene...");
        //    Messenger.Cleanup();
        //    ReloadScene();
        //    return;
        //} else {
        //    Debug.Log(string.Format("Region Generation took {0} ms to complete", st.ElapsedMilliseconds));
        //}

        st.Start();
        Biomes.Instance.DetermineIslands();
        st.Stop();
        Debug.Log(string.Format("Island Connections took {0} ms to complete", st.ElapsedMilliseconds));

        //RoadManager.Instance.FlattenRoads();
        //Biomes.Instance.LoadElevationSprites();
        //Biomes.Instance.GenerateTileBiomeDetails();

        //return;
        RoadManager.Instance.GenerateTilePassableTypes();
        //GridMap.Instance.BottleneckBorders();

        GridMap.Instance.GenerateOuterGrid();
        GridMap.Instance.DivideOuterGridRegions();

        UIManager.Instance.InitializeUI();

        ObjectManager.Instance.Initialize();
        //Load Initial Factions

        //Load all landmarks
        LandmarkManager.Instance.LoadAllLandmarksFromSave(save);
        
        PathfindingManager.Instance.CreateGrid();

        Biomes.Instance.UpdateTileVisuals(GridMap.Instance.allTiles);
        Biomes.Instance.GenerateTileBiomeDetails(GridMap.Instance.hexTiles);

        GameManager.Instance.StartProgression();

        CameraMove.Instance.UpdateMinimapTexture();
        loadingWatch.Stop();
        Debug.Log(string.Format("Total loading time is {0} ms", loadingWatch.ElapsedMilliseconds));
    }

    internal void ReloadScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
