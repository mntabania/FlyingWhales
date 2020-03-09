using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class STCManager : MonoBehaviour {

    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap groundWallTilemap;
    [SerializeField] private Tilemap structureWallTilemap;
    [SerializeField] private Tilemap objectsTilemap;
    [SerializeField] private Tilemap detailsTilemap;

    [Header("Parents")]
    [SerializeField] private Transform connectorsParent;
    [SerializeField] private Transform furnitureParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject connectorPrefab;
    [SerializeField] private GameObject furnitureSpotPrefab;

    [SerializeField] private List<TileBase> allTileAssets;

    private string templatePath;

    private void Awake() {
#if UNITY_EDITOR
        templatePath = $"{Application.dataPath}/StreamingAssets/Structure Templates/";
#endif
    }

    private TileTemplateData[] GetTileData(Tilemap tilemap, BoundsInt bounds) {
        TileTemplateData[] data = new TileTemplateData[bounds.size.x * bounds.size.y];
        int count = 0;

        for (int x = bounds.xMin; x < bounds.xMax; x++) {
            for (int y = bounds.yMin; y < bounds.yMax; y++) {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tb = tilemap.GetTile(pos);
                Matrix4x4 matrix = tilemap.GetTransformMatrix(pos);

                int normalizedX = x;
                int normalizedY = y;

                if (bounds.xMin != 0) {
                    if (bounds.xMin < 0) {
                        //x is negative
                        normalizedX += Mathf.Abs(bounds.xMin);
                    } else {
                        //x is positive
                        normalizedX -= Mathf.Abs(bounds.xMin);
                    }
                }

                if (bounds.yMin != 0) {
                    if (bounds.yMin < 0) {
                        //y is negative
                        normalizedY += Mathf.Abs(bounds.yMin);
                    } else {
                        //y is positive
                        normalizedY -= Mathf.Abs(bounds.yMin);
                    }
                }

                data[count] = new TileTemplateData(tb, matrix, new Vector3(normalizedX, normalizedY, 0));
                count++;
            }
        }
        return data;
    }
    /// <summary>
    /// Convinience function, for making the tiles have all positive coordinates
    /// </summary>
    public void PlaceAtOrigin() {
        groundTilemap.CompressBounds();
        BoundsInt bounds = groundTilemap.cellBounds;
        int shiftXBy = 0; //shift x position of all objects by n
        int shiftYBy = 0;//shift y position of all objects by n
        if (bounds.xMin != 0) {
            shiftXBy = bounds.xMin * -1;
        }

        if (bounds.yMin != 0) {
            shiftYBy = bounds.yMin * -1;
        }

        Vector3Int shiftBy = new Vector3Int(shiftXBy, shiftYBy, 0);
        Debug.Log($"Shifting map by {shiftBy}");
        Tilemap[] tilemaps = GetComponentsInChildren<Tilemap>();
        for (int i = 0; i < tilemaps.Length; i++) {
            ShiftTilemapPosition(tilemaps[i], bounds);
        }

        //shift connectors
        BuildingSpotDataMonobehaviour[] connectors = connectorsParent.GetComponentsInChildren<BuildingSpotDataMonobehaviour>();
        for (int i = 0; i < connectors.Length; i++) {
            Vector3 currPos = connectors[i].transform.localPosition;
            //Vector3 actualPos = new Vector3(currPos.x - 0.5f, currPos.y - 0.5f, 0f);
            Vector3 newPos = new Vector3(currPos.x + shiftBy.x, currPos.y + shiftBy.y, 0f);
            connectors[i].transform.localPosition = newPos;
        }

        //shift Furniture spots
        FurnitureSpotMono[] furnitureSpots = connectorsParent.GetComponentsInChildren<FurnitureSpotMono>();
        for (int i = 0; i < furnitureSpots.Length; i++) {
            Vector3 currPos = furnitureSpots[i].transform.localPosition;
            //Vector3 actualPos = new Vector3(currPos.x - 0.5f, currPos.y - 0.5f, 0f);
            Vector3 newPos = new Vector3(currPos.x + shiftBy.x, currPos.y + shiftBy.y, 0f);
            furnitureSpots[i].transform.localPosition = newPos;
        }
    }
    private void ShiftTilemapPosition(Tilemap map, BoundsInt bounds) {
        TileTemplateData[] allData = GetTileData(map, bounds);

        map.ClearAllTiles();

        for (int i = 0; i < allData.Length; i++) {
            TileTemplateData currData = allData[i];
            Vector3Int newCoords = new Vector3Int((int)(currData.tilePosition.x), (int)(currData.tilePosition.y), 0);
            map.SetTile(newCoords, GetTileAsset(currData.tileAssetName));
            map.SetTransformMatrix(newCoords, currData.matrix);
        }
    }
    public void SaveTemplate() {
        groundTilemap.CompressBounds();
        PlaceAtOrigin();
        //get all tiles based on the bounds of the ground tile
        TileTemplateData[] groundTiles = GetTileData(groundTilemap, groundTilemap.cellBounds);
        TileTemplateData[] groundWallTiles = GetTileData(groundWallTilemap, groundTilemap.cellBounds);
        TileTemplateData[] wallTiles = GetTileData(structureWallTilemap, groundTilemap.cellBounds);
        TileTemplateData[] objectTiles = GetTileData(objectsTilemap, groundTilemap.cellBounds);
        TileTemplateData[] detailTiles = GetTileData(detailsTilemap, groundTilemap.cellBounds);

        BuildingSpotDataMonobehaviour[] buildingSpotMonos = UtilityScripts.GameUtilities.GetComponentsInDirectChildren<BuildingSpotDataMonobehaviour>(connectorsParent.gameObject);
        FurnitureSpotMono[] furnitureSpotMonos = UtilityScripts.GameUtilities.GetComponentsInDirectChildren<FurnitureSpotMono>(furnitureParent.gameObject);

        BuildingSpotData[] connectors = new BuildingSpotData[buildingSpotMonos.Length];
        for (int i = 0; i < buildingSpotMonos.Length; i++) {
            BuildingSpotDataMonobehaviour currMono = buildingSpotMonos[i];
            connectors[i] = currMono.Convert();
        }

        FurnitureSpot[] furnitureSpots = new FurnitureSpot[furnitureSpotMonos.Length];
        for (int i = 0; i < furnitureSpotMonos.Length; i++) {
            FurnitureSpotMono currMono = furnitureSpotMonos[i];
            furnitureSpots[i] = currMono.GetFurnitureSpot();
        }

        Debug.Log($"Got {groundTiles.Length} tiles");

        StructureTemplate newTemplate = new StructureTemplate("Structure_Template", groundTiles, groundWallTiles, wallTiles, objectTiles, detailTiles,
            new Point(groundTilemap.cellBounds.size.x, groundTilemap.cellBounds.size.y), connectors, furnitureSpots);

        string dataAsJson = JsonUtility.ToJson(newTemplate);

#if UNITY_EDITOR
        string path = EditorUtility.SaveFilePanel("Save Structure Template", templatePath, "Structure_Template", "json");
        if (path == "")
            return;
        File.WriteAllText(path, dataAsJson);
        AssetDatabase.Refresh();
#endif
    }
    public void LoadTemplate() {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel("Load Structure Template", templatePath, "json");
        if (path.Length != 0) {
            string dataAsJson = File.ReadAllText(path);
            StructureTemplate loaded = JsonUtility.FromJson<StructureTemplate>(dataAsJson);
            LoadTemplate(loaded);
            Debug.Log($"Loaded {path}");
        }
#endif
    }
    private BuildingSpotDataMonobehaviour GetBuildingSpotWithID(int id, BuildingSpotDataMonobehaviour[] pool) {
        for (int i = 0; i < pool.Length; i++) {
            BuildingSpotDataMonobehaviour currSpot = pool[i];
            if (currSpot.id == id) {
                return currSpot;
            }
        }
        throw new System.Exception($"There is no building spot with id {id}");
    }
    private void LoadTemplate(StructureTemplate st) {
        ClearTiles();
        DrawTiles(groundTilemap, st.groundTiles);
        DrawTiles(groundWallTilemap, st.groundWallTiles);
        DrawTiles(structureWallTilemap, st.structureWallTiles);
        DrawTiles(objectsTilemap, st.objectTiles);
        DrawTiles(detailsTilemap, st.detailTiles);

        //connectors
        UtilityScripts.Utilities.DestroyChildren(connectorsParent);
        if (st.connectors != null) {
            BuildingSpotDataMonobehaviour[] createdConnectors = new BuildingSpotDataMonobehaviour[st.connectors.Length];
            BuildingSpotData[] ordered = st.connectors.ToList().OrderBy(x => x.id).ToArray();
            for (int i = 0; i < ordered.Length; i++) {
                BuildingSpotData connector = ordered[i];
                GameObject newConnector = GameObject.Instantiate(connectorPrefab, connectorsParent);
                newConnector.transform.localPosition = new Vector3(connector.location.x + 0.5f, connector.location.y + 0.5f, 0);
                BuildingSpotDataMonobehaviour cm = newConnector.GetComponent<BuildingSpotDataMonobehaviour>();
                cm.id = connector.id;
                createdConnectors[i] = cm;
            }

            //assign adjacent ids
            for (int i = 0; i < createdConnectors.Length; i++) {
                BuildingSpotDataMonobehaviour currConnector = createdConnectors[i];
                BuildingSpotData data = ordered[i];
                if (data.adjacentSpots != null) {
                    for (int j = 0; j < data.adjacentSpots.Length; j++) {
                        int currAdjacentID = data.adjacentSpots[j];
                        BuildingSpotDataMonobehaviour adjacentSpot = GetBuildingSpotWithID(currAdjacentID, createdConnectors);
                        currConnector.adjacentSpots.Add(adjacentSpot);
                    }
                }
            }
        }

        //furniture spots
        UtilityScripts.Utilities.DestroyChildren(furnitureParent);
        if (st.furnitureSpots != null) {
            for (int i = 0; i < st.furnitureSpots.Length; i++) {
                FurnitureSpot spot = st.furnitureSpots[i];
                GameObject newSpot = GameObject.Instantiate(furnitureSpotPrefab, furnitureParent);
                newSpot.transform.localPosition = new Vector3(spot.location.x + 0.5f, spot.location.y + 0.5f, 0);
                FurnitureSpotMono cm = newSpot.GetComponent<FurnitureSpotMono>();
                cm.allowedFurnitureTypes = spot.allowedFurnitureTypes;
                cm.furnitureSettings = spot.furnitureSettings;
            }
        }
    }
    private void DrawTiles(Tilemap tilemap, TileTemplateData[] data) {
        if (data == null) {
            return;
        }
        for (int i = 0; i < data.Length; i++) {
            TileTemplateData currData = data[i];
            Vector3Int pos = new Vector3Int((int)currData.tilePosition.x, (int)currData.tilePosition.y, 0);
            tilemap.SetTile(pos, GetTileAsset(currData.tileAssetName));
            tilemap.SetTransformMatrix(pos, currData.matrix);
        }
    }
    private TileBase GetTileAsset(string name) {
        for (int i = 0; i < allTileAssets.Count; i++) {
            TileBase currTile = allTileAssets[i];
            if (currTile.name == name) {
                return currTile;
            }
        }
        return null;
    }
    public void ClearTiles() {
        groundTilemap.ClearAllTiles();
        groundWallTilemap.ClearAllTiles();
        structureWallTilemap.ClearAllTiles();
        objectsTilemap.ClearAllTiles();
        detailsTilemap.ClearAllTiles();

        UtilityScripts.Utilities.DestroyChildren(connectorsParent);
    }
    public void LoadAllTilesAssets() {
        allTileAssets = Resources.LoadAll("Tile Map Assets", typeof(TileBase)).Cast<TileBase>().ToList();
        //foreach (var t in textures)
        //    Debug.Log(t.name);
    }

    #region Building Spots
    public void CreateNewBuildingSpot() {
        GameObject newSpot = GameObject.Instantiate(connectorPrefab, connectorsParent);
        newSpot.transform.localPosition = Vector3.zero;
    }
    #endregion

    #region Furniture Spots
    public FurnitureSpotMono CreateNewFurnitureSpot() {
        GameObject newConnector = GameObject.Instantiate(furnitureSpotPrefab, furnitureParent);
        newConnector.transform.localPosition = Vector3.zero;
        return newConnector.GetComponent<FurnitureSpotMono>();
        //ConnectorMono cm = newConnector.GetComponent<ConnectorMono>();
        //cm.connectionDirection = connector.neededDirection;
        //cm.connectionType = connector.allowedStructureType;
    }
    public FurnitureSpotMono CreateNewFurnitureSpot(Vector3 position) {
        GameObject newConnector = GameObject.Instantiate(furnitureSpotPrefab, furnitureParent);
        newConnector.transform.localPosition = position;
        return newConnector.GetComponent<FurnitureSpotMono>();
        //ConnectorMono cm = newConnector.GetComponent<ConnectorMono>();
        //cm.connectionDirection = connector.neededDirection;
        //cm.connectionType = connector.allowedStructureType;
    }
    public void CreateFurnitureSpotsFromPlacedObjects() {
        groundTilemap.CompressBounds();
        PlaceAtOrigin();
        TileTemplateData[] data = GetTileData(objectsTilemap, groundTilemap.cellBounds);
        for (int i = 0; i < data.Length; i++) {
            TileTemplateData currData = data[i];
            if (!string.IsNullOrEmpty(currData.tileAssetName)) {
                List<FURNITURE_TYPE> allowedFurnitureTypes = new List<FURNITURE_TYPE>();
                if (currData.tileAssetName.Contains("Bed")) {
                    allowedFurnitureTypes.Add(FURNITURE_TYPE.BED);
                } else if (currData.tileAssetName.Contains("Table")) {
                    allowedFurnitureTypes.Add(FURNITURE_TYPE.TABLE);
                } else if (currData.tileAssetName.Contains("Desk")) {
                    allowedFurnitureTypes.Add(FURNITURE_TYPE.DESK);
                } else if (currData.tileAssetName.Contains("Guitar")) {
                    allowedFurnitureTypes.Add(FURNITURE_TYPE.GUITAR);
                }
                FurnitureSpotMono mono = CreateNewFurnitureSpot(currData.centeredTilePosition);
                mono.allowedFurnitureTypes = allowedFurnitureTypes.ToArray();
            }
        }

    }
    #endregion

    #region For Testing
    private Dictionary<STRUCTURE_TYPE, int> testingStructures = new Dictionary<STRUCTURE_TYPE, int>() {
        { STRUCTURE_TYPE.DWELLING, 3 },
    };
    public List<StructureTemplate> GetStructureTemplates(string folderName) {
        List<StructureTemplate> templates = new List<StructureTemplate>();
        string path = $"{templatePath}{folderName}/";
        if (Directory.Exists(path)) {
            DirectoryInfo info = new DirectoryInfo(path);
            FileInfo[] files = info.GetFiles();
            for (int i = 0; i < files.Length; i++) {
                FileInfo currInfo = files[i];
                if (currInfo.Extension.Equals(".json")) {
                    string dataAsJson = File.ReadAllText(currInfo.FullName);
                    StructureTemplate loaded = JsonUtility.FromJson<StructureTemplate>(dataAsJson);
                    templates.Add(loaded);
                }
            }
        }
        return templates;
    }
    #endregion
}


[System.Serializable]
public class StructureTemplate {
    public string name;
    public Point size;
    public TileTemplateData[] groundTiles;
    public TileTemplateData[] groundWallTiles;
    public TileTemplateData[] structureWallTiles;
    public TileTemplateData[] objectTiles;
    public TileTemplateData[] detailTiles;
    public BuildingSpotData[] connectors;
    public FurnitureSpot[] furnitureSpots;


    public StructureTemplate(string _name, TileTemplateData[] _ground, TileTemplateData[] _groundWalls, TileTemplateData[] _walls,
        TileTemplateData[] _objects, TileTemplateData[] _details, Point _size, BuildingSpotData[] _connectors, FurnitureSpot[] _furnitureSpots) {
        name = _name;
        size = _size;
        groundTiles = _ground;
        groundWallTiles = _groundWalls;
        structureWallTiles = _walls;
        objectTiles = _objects;
        detailTiles = _details;
        connectors = _connectors;
        furnitureSpots = _furnitureSpots;
    }


    #region Building Spots
    public bool HasEnoughBuildSpotsForArea(NPCSettlement npcSettlement) {
        return connectors.Length >= npcSettlement.structures.Count;
    }
    public bool TryGetOpenBuildingSpot(out BuildingSpotData spot) {
        for (int i = 0; i < connectors.Length; i++) {
            if (connectors[i].isOpen) {
                spot = connectors[i];
                return true;
            }
        }
        spot = default(BuildingSpotData);
        return false;
    }
    public BuildingSpotData GetRandomBuildingSpot() {
        return connectors[Random.Range(0, connectors.Length)];
    }
    public BuildingSpotData GetBuildingSpotWithID(int id) {
        for (int i = 0; i < connectors.Length; i++) {
            BuildingSpotData currSpot = connectors[i];
            if (currSpot.id == id) {
                return currSpot;
            }
        }
        throw new System.Exception($"There is no building spot with id {id}");
    }
    public bool TryGetBuildingSpotDataAtLocation(Vector3 location, out BuildingSpotData spot) {
        for (int i = 0; i < connectors.Length; i++) {
            BuildingSpotData currSpot = connectors[i];
            if (currSpot.location.x == location.x && currSpot.location.y == location.y) {
                spot = currSpot;
                return true;
            }
        }
        spot = default(BuildingSpotData);
        return false;
    }
    #endregion

    #region Utilities
    public void UpdatePositionsGivenOrigin(Vector3Int origin) {
        UpdatePositionsGivenOrigin(groundTiles, origin);
        UpdatePositionsGivenOrigin(structureWallTiles, origin);
        UpdatePositionsGivenOrigin(objectTiles, origin);
        UpdatePositionsGivenOrigin(detailTiles, origin);
        UpdatePositionsGivenOrigin(connectors, origin);
    }
    private void UpdatePositionsGivenOrigin(TileTemplateData[] data, Vector3Int origin) {
        for (int i = 0; i < data.Length; i++) {
            TileTemplateData currData = data[i];
            currData.tilePosition = new Vector3(currData.tilePosition.x + origin.x, currData.tilePosition.y + origin.y);
            data[i] = currData;
        }
    }
    private void UpdatePositionsGivenOrigin(BuildingSpotData[] data, Vector3Int origin) {
        for (int i = 0; i < data.Length; i++) {
            BuildingSpotData currData = data[i];
            currData.location = new Vector3Int(currData.location.x + origin.x, currData.location.y + origin.y, 0);
            data[i] = currData;
        }
    }
    #endregion
}

[System.Serializable]
public struct TileTemplateData {

    public Vector3 tilePosition;
    public string tileAssetName;
    public Matrix4x4 matrix;

    public Vector3 centeredTilePosition {
        get { return new Vector3(tilePosition.x + 0.5f, tilePosition.y + 0.5f, tilePosition.z); }
    }
    public TileTemplateData(TileBase tb, Matrix4x4 m, Vector3 pos) {
        if (tb == null) {
            tileAssetName = string.Empty;
        } else {
            tileAssetName = tb.name;
        }
        tilePosition = pos;
        //rotation = m.rotation.eulerAngles;
        matrix = m;
    }

    public static TileTemplateData Empty {
        get {
            return new TileTemplateData() {
                tilePosition = Vector3.zero,
                tileAssetName = string.Empty,
                matrix = Matrix4x4.zero
            };
        }
    }

}

