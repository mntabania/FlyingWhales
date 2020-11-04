using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;

public class MapGenerator : MonoBehaviour {

    public static MapGenerator Instance = null;

    private void Awake() {
        Instance = this;
    }

    internal void InitializeWorld() {
        StartCoroutine(InitializeWorldCoroutine());
    }
    internal void InitializeWorld(WorldSaveData data) {
        StartCoroutine(InitializeWorldCoroutine(data));
    }

    private IEnumerator InitializeWorldCoroutine() {
        System.Diagnostics.Stopwatch loadingWatch = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
        loadingWatch.Start();

        LevelLoaderManager.UpdateLoadingInfo("Generating Map...");
        yield return null;
        GridMap.Instance.GenerateGrid();
        CameraMove.Instance.CalculateCameraBounds();
        ObjectPoolManager.Instance.InitializeObjectPools();
        CameraMove.Instance.SetWholemapCameraValues();
        Minimap.Instance.Initialize();
        EquatorGenerator.Instance.GenerateEquator((int)GridMap.Instance.width, (int)GridMap.Instance.height, GridMap.Instance.hexTiles);
        Biomes.Instance.GenerateElevation(GridMap.Instance.hexTiles, (int)GridMap.Instance.width, (int)GridMap.Instance.height);

        LevelLoaderManager.UpdateLoadingInfo("Generating Biomes...");
        yield return null;
        Biomes.Instance.GenerateBiome(GridMap.Instance.hexTiles);
        Biomes.Instance.LoadPassableObjects(GridMap.Instance.hexTiles, GridMap.Instance.outerGridList);

        LevelLoaderManager.UpdateLoadingInfo("Generating Regions...");
        yield return null;
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

        LevelLoaderManager.UpdateLoadingInfo("Generating Factions...");
        yield return null;
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

        LevelLoaderManager.UpdateLoadingInfo("Generating Landmarks...");
        yield return null;
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
        PathfindingManager.Instance.CreateGrid(GridMap.Instance.map, (int)GridMap.Instance.width, (int)GridMap.Instance.height);

        FactionManager.Instance.OccupyLandmarksInFactionRegions();

        LevelLoaderManager.UpdateLoadingInfo("Starting Game...");
        yield return null;
        ObjectManager.Instance.Initialize();
        //LandmarkManager.Instance.ConstructAllLandmarkObjects();

        //LandmarkManager.Instance.GenerateMaterials();

        //RoadManager.Instance.FlattenRoads();
        //Biomes.Instance.GenerateTileTags();
        //GridMap.Instance.GenerateNeighboursWithSameTag();
        Biomes.Instance.UpdateTileVisuals(GridMap.Instance.allTiles);
        Biomes.Instance.GenerateTileBiomeDetails(GridMap.Instance.hexTiles);


        GameManager.Instance.StartProgression();
        LandmarkManager.Instance.InitializeLandmarks();
        CharacterManager.Instance.GenerateCharactersForTesting(3);
        //FactionManager.Instance.GenerateFactionCharacters();
        //FactionManager.Instance.GenerateMonsters();
        //StorylineManager.Instance.GenerateStoryLines();
        //CharacterManager.Instance.SchedulePrisonerConversion();
        //CameraMove.Instance.CenterCameraOn(FactionManager.Instance.allTribes.FirstOrDefault().settlements.FirstOrDefault().tileLocation.gameObject);
        CameraMove.Instance.UpdateMinimapTexture();
        loadingWatch.Stop();
        Debug.Log(string.Format("Total loading time is {0} ms", loadingWatch.ElapsedMilliseconds));
        LevelLoaderManager.SetLoadingState(false);
    }

    private IEnumerator InitializeWorldCoroutine(WorldSaveData data) {
        System.Diagnostics.Stopwatch loadingWatch = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
        loadingWatch.Start();

        LevelLoaderManager.UpdateLoadingInfo("Loading Map...");
        yield return null;
        GridMap.Instance.GenerateGrid(data);
        CameraMove.Instance.CalculateCameraBounds();
        Minimap.Instance.Initialize();
        ObjectPoolManager.Instance.InitializeObjectPools();
        CameraMove.Instance.SetWholemapCameraValues();
        //EquatorGenerator.Instance.GenerateEquator((int)GridMap.Instance.width, (int)GridMap.Instance.height, GridMap.Instance.hexTiles);
        //Biomes.Instance.GenerateElevation(GridMap.Instance.hexTiles, (int)GridMap.Instance.width, (int)GridMap.Instance.height);

        //LevelLoaderManager.UpdateLoadingInfo("Loading Biomes...");
        //yield return null;
        

        LevelLoaderManager.UpdateLoadingInfo("Loading Regions...");
        yield return null;
        st.Start();
        GridMap.Instance.LoadRegions(data);
        st.Stop();

        GridMap.Instance.GenerateOuterGrid();
        GridMap.Instance.DivideOuterGridRegions();

        Biomes.Instance.LoadPassableObjects(GridMap.Instance.hexTiles, GridMap.Instance.outerGridList);

        RoadManager.Instance.GenerateTilePassableTypes();

        UIManager.Instance.InitializeUI();
        ObjectManager.Instance.Initialize();

        LevelLoaderManager.UpdateLoadingInfo("Loading Factions...");
        yield return null;
        st.Start();
        FactionManager.Instance.LoadFactions(data);
        st.Stop();

        GridMap.Instance.OccupyRegions(data);

        LevelLoaderManager.UpdateLoadingInfo("Loading Landmarks...");
        yield return null;
        st.Start();
        LandmarkManager.Instance.LoadLandmarks(data);
        st.Stop();

        LandmarkManager.Instance.LoadAreas(data);

        //LandmarkManager.Instance.GeneratePlayerLandmarks(playerRegion);
        PathfindingManager.Instance.LoadSettings(data.pathfindingSettings);
        PathfindingManager.Instance.CreateGrid(GridMap.Instance.map, (int) GridMap.Instance.width, (int) GridMap.Instance.height);

        FactionManager.Instance.OccupyLandmarksInFactionRegions();

        LevelLoaderManager.UpdateLoadingInfo("Starting Game...");
        yield return null;

        Biomes.Instance.UpdateTileVisuals(GridMap.Instance.allTiles);
        Biomes.Instance.GenerateTileBiomeDetails(GridMap.Instance.hexTiles);


        GameManager.Instance.StartProgression();
        LandmarkManager.Instance.InitializeLandmarks();
        //CharacterManager.Instance.GenerateCharactersForTesting(1);
        CharacterManager.Instance.LoadCharacters(data);
        CharacterManager.Instance.LoadRelationships(data);

        CameraMove.Instance.UpdateMinimapTexture();
        loadingWatch.Stop();
        Debug.Log(string.Format("Total loading time is {0} ms", loadingWatch.ElapsedMilliseconds));
        LevelLoaderManager.SetLoadingState(false);
    }

    internal void ReloadScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
