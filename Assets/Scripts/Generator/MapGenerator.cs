using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Globalization;
using Inner_Maps;

public class MapGenerator : MonoBehaviour {

    public static MapGenerator Instance;
    
    private void Awake() {
        Instance = this;
    }

    internal IEnumerator InitializeWorld() {
        MapGenerationComponent[] mapGenerationComponents = {
            new WorldMapGridGeneration(), new WorldMapElevationGeneration(), new SupportingFactionGeneration(), 
            new WorldMapRegionGeneration(), new WorldMapBiomeGeneration(), new WorldMapOuterGridGeneration(),
            new TileFeatureGeneration(), new PortalLandmarkGeneration(), new WorldMapLandmarkGeneration(),
            new FamilyTreeGeneration(), new RegionInnerMapGeneration(), new SettlementGeneration(), 
            new LandmarkStructureGeneration(), new ElevationStructureGeneration(), new MonsterGeneration(), 
            new MapGenerationFinalization(), new PlayerDataGeneration(),
        };
        yield return StartCoroutine(InitializeWorldCoroutine(mapGenerationComponents));
    }
    public void InitializeWorld(Save data) {
        StartCoroutine(InitializeWorldCoroutine(data));
    }
    private IEnumerator InitializeWorldCoroutine(MapGenerationComponent[] components) {
        System.Diagnostics.Stopwatch loadingWatch = new System.Diagnostics.Stopwatch();
        loadingWatch.Start();

        string loadingDetails = "Loading details";

        bool componentFailed = false;
        
        MapGenerationData data = new MapGenerationData();
        System.Diagnostics.Stopwatch componentWatch = new System.Diagnostics.Stopwatch();
        for (int i = 0; i < components.Length; i++) {
            MapGenerationComponent currComponent = components[i];
            componentWatch.Start();
            yield return StartCoroutine(currComponent.Execute(data));
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
        if (componentFailed) {
            //reload scene
            Debug.LogWarning("A component in world generation failed! Reloading scene...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        } else {
            loadingWatch.Stop();
            Debug.Log(
                $"{loadingDetails}\nTotal loading time is {loadingWatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds");

            LevelLoaderManager.Instance.SetLoadingState(false);
            CameraMove.Instance.CenterCameraOn(data.portal.tileLocation.gameObject);
            InnerMapManager.Instance.TryShowLocationMap(data.portal.tileLocation.region);
            InnerMapCameraMove.Instance.CenterCameraOnTile(data.portal.tileLocation);
            AudioManager.Instance.TransitionTo("World Music", 10);

            for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
                Faction faction = FactionManager.Instance.allFactions[i];
                if (faction.isMajorNonPlayer) {
                    faction.DesignateNewLeader(false);
                    faction.GenerateInitialOpinionBetweenMembers();
                }
            }
            for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
                Region region = GridMap.Instance.allRegions[i];
                region.UpdateAwareness();
                for (int j = 0; j < region.tiles.Count; j++) {
                    HexTile tile = region.tiles[j];
                    if (!tile.isCorrupted
                        && tile.landmarkOnTile != null
                        && (tile.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.VILLAGE
                            || tile.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.HOUSES)
                        && tile.settlementOnTile is NPCSettlement npcSettlement) {
                        if(npcSettlement.ruler == null) {
                            npcSettlement.DesignateNewRuler(false);
                        }
                        npcSettlement.GenerateInitialOpinionBetweenResidents();
                    }
                }
            }
            Messenger.Broadcast(Signals.GAME_LOADED);
            yield return new WaitForSeconds(1f);
            // PlayerManager.Instance.player.IncreaseArtifactSlot();
            //PlayerManager.Instance.player.IncreaseSummonSlot();
            GameManager.Instance.StartProgression();
            //UIManager.Instance.SetSpeedTogglesState(false);
            //PlayerUI.Instance.ShowStartingMinionPicker();
        }
    }
    private IEnumerator InitializeWorldCoroutine(Save data) {
        System.Diagnostics.Stopwatch loadingWatch = new System.Diagnostics.Stopwatch();
        //System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
        loadingWatch.Start();

        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Map...");
        GridMap.Instance.SetupInitialData(data.width, data.height);
        yield return null;
        CameraMove.Instance.Initialize();
        InnerMapManager.Instance.Initialize();
        InteractionManager.Instance.Initialize();
        ObjectPoolManager.Instance.InitializeObjectPools();
        yield return null;
        GridMap.Instance.GenerateGrid(data);
        yield return null;
        GridMap.Instance.GenerateOuterGrid(data);
        yield return null;
        Biomes.Instance.UpdateTileVisuals(GridMap.Instance.allTiles);
        yield return null;
        data.LoadRegions();
        data.LoadPlayerArea();
        data.LoadNonPlayerAreas();
        data.LoadFactions();
        data.LoadCharacters();
        // data.LoadSpecialObjects();
        //data.LoadTileObjects();
        yield return null;
        data.LoadCharacterRelationships();
        data.LoadCharacterTraits();
        yield return null;
        data.LoadLandmarks();
        data.LoadRegionCharacters();
        data.LoadRegionAdditionalData();
        yield return null;

        // CameraMove.Instance.CalculateCameraBounds();
        UIManager.Instance.InitializeUI();
        LevelLoaderManager.Instance.UpdateLoadingInfo("Starting Game...");
        yield return null;

        // TokenManager.Instance.Initialize();
        //CharacterManager.Instance.GenerateRelationships();

        yield return null;
        //LandmarkManager.Instance.GenerateAreaMap(LandmarkManager.Instance.enemyOfPlayerArea, false);
        data.LoadAreaMaps();
        data.LoadAreaStructureEntranceTiles();
        //data.LoadTileObjectsPreviousTileAndCurrentTile();
        data.LoadAreaMapsObjectHereOfTiles();
        data.LoadAreaMapsTileTraits();
        //data.LoadTileObjectTraits();
        data.LoadCharacterHomeStructures();
        data.LoadCurrentDate(); //Moved this because some jobs use current date
        data.LoadCharacterInitialPlacements();
        //data.LoadPlayer();

        data.LoadAllJobs();
        //data.LoadTileObjectsDataAfterLoadingAreaMap();

        //Note: Loading npcSettlement items is after loading the inner map because LocationStructure and LocationGridTile is required
        data.LoadPlayerAreaItems();
        data.LoadNonPlayerAreaItems();
        yield return null;
        data.LoadCharacterHistories();

        data.LoadCharacterCurrentStates();
        data.LoadFactionsActiveQuests();

        
        data.LoadNotifications();

        loadingWatch.Stop();
        Debug.Log($"Total loading time is {loadingWatch.ElapsedMilliseconds.ToString()} ms");
        LevelLoaderManager.Instance.SetLoadingState(false);
        //TODO:
        // CameraMove.Instance.CenterCameraOn(PlayerManager.Instance.player.playerNpcSettlement.coreTile.gameObject);
        AudioManager.Instance.TransitionTo("World Music", 10);
        yield return new WaitForSeconds(1f);
        GameManager.Instance.StartProgression();
        UIManager.Instance.SetSpeedTogglesState(true);
        Messenger.Broadcast(Signals.UPDATE_UI);
        yield return null;
        UIManager.Instance.Unpause();
        yield return null;
        UIManager.Instance.Pause();
        Messenger.Broadcast(Signals.GAME_LOADED);
        //data.LoadInvasion();
        //PlayerManager.Instance.player.LoadResearchNewInterventionAbility(data.playerSave);

    }
    public void SetIsCoroutineRunning(bool state) { }
}
