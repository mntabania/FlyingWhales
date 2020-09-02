using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;
using UtilityScripts;

public class GridMap : BaseMonoBehaviour {
	public static GridMap Instance;

	public GameObject goHex;
    [Space(10)]
    [Header("Map Settings")]
    public int width;
	public int height;
    [FormerlySerializedAs("_borderParent")] public Transform borderParent;
    [SerializeField] internal int _borderThickness;
    
    [Space(10)]
    public HashSet<HexTile> outerGridList;
    public HexTile[,] map;

    #region getters
    public List<HexTile> normalHexTiles => DatabaseManager.Instance.hexTileDatabase.allHexTiles;
    public Region[] allRegions => DatabaseManager.Instance != null ? DatabaseManager.Instance.regionDatabase.allRegions : null;
    #endregion
    
    void Awake(){
		Instance = this;
    }

    #region Grid Generation
    public void SetupInitialData(int width, int height) {
        this.width = width;
        this.height = height;
    }

    public void SetMap(HexTile[,] map, List<HexTile> normalHexTiles) {
        this.map = map;
        for (int i = 0; i < normalHexTiles.Count; i++) {
            HexTile hexTile = normalHexTiles[i];
            DatabaseManager.Instance.hexTileDatabase.RegisterHexTile(hexTile);
        }
    }
    public void SetOuterGridList(HashSet<HexTile> outerTiles) {
        outerGridList = outerTiles;
    }
    public HexTile GetTileFromCoordinates(int x, int y) {
        if ((x < 0 || x > width - 1) || (y < 0 || y > height - 1)) {
            //outer tile
            return GetBorderTile(x, y);
        } else {
            return map[x, y];
        }
    }
    private HexTile GetBorderTile(int x, int y) {
        for (int i = 0; i < outerGridList.Count; i++) {
            HexTile currTile = outerGridList.ElementAt(i);
            if (currTile.xCoordinate == x && currTile.yCoordinate == y) {
                return currTile;
            }
        }
        return null;
    }
    #endregion

    #region Grid Utilities
    internal HexTile GetHexTile(int id) {
        for (int i = 0; i < normalHexTiles.Count; i++) {
            if (normalHexTiles[i].id == id) {
                return normalHexTiles[i];
            }
        }
        return null;
    }
    public List<HexTile> GetTilesInRange(HexTile center, int range) {
        List<HexTile> tilesInRange = new List<HexTile>();
        CubeCoordinate cube = OddRToCube(new HexCoordinate(center.xCoordinate, center.yCoordinate));
        Debug.Log($"Center in cube coordinates: {cube.x},{cube.y},{cube.z}");
        for (int dx = -range; dx <= range; dx++) {
            for (int dy = Mathf.Max(-range, -dx - range); dy <= Mathf.Min(range, -dx + range); dy++) {
                int dz = -dx - dy;
                HexCoordinate hex = CubeToOddR(new CubeCoordinate(cube.x + dx, cube.y + dy, cube.z + dz));
                //Debug.Log("Hex neighbour: " + hex.col.ToString() + "," + hex.row.ToString());
                if (hex.col >= 0 && hex.row >= 0 && !(hex.col == center.xCoordinate && hex.row == center.yCoordinate)) {
                    tilesInRange.Add(map[hex.col, hex.row]);
                }
            }
        }
        return tilesInRange;
    }
    public HexCoordinate CubeToOddR(CubeCoordinate cube) {
        int modifier = 0;
        if (cube.z % 2 == 1) {
            modifier = 1;
        }
        int col = cube.x + (cube.z - (modifier)) / 2;
        int row = cube.z;
        return new HexCoordinate(col, row);
    }
    public CubeCoordinate OddRToCube(HexCoordinate hex) {
        int modifier = 0;
        if (hex.row % 2 == 1) {
            modifier = 1;
        }

        int x = hex.col - (hex.row - (modifier)) / 2;
        int z = hex.row;
        int y = -x - z;
        return new CubeCoordinate(x, y, z);
    }
    #endregion

    #region Regions
    public void SetRegions(Region[] regions) {
        DatabaseManager.Instance.regionDatabase.RegisterRegions(regions);
        for (int i = 0; i < allRegions.Length; i++) {
            allRegions[i].FinalizeData();
        }
    }
    public Region GetRegionByID(int id) {
        for (int i = 0; i < allRegions.Length; i++) {
            if(allRegions[i].id == id) {
                return allRegions[i];
            }
        }
        return null;
    }
    public Region GetRegionByPersistentID(string id) {
        for (int i = 0; i < allRegions.Length; i++) {
            if (allRegions[i].persistentID == id) {
                return allRegions[i];
            }
        }
        return null;
    }
    public Region GetRegionByName(string name) {
        for (int i = 0; i < allRegions.Length; i++) {
            if (allRegions[i].name == name) {
                return allRegions[i];
            }
        }
        return null;
    }
    public void UpdateAwarenessInAllRegions() {
        for (int i = 0; i < allRegions.Length; i++) {
            allRegions[i].UpdateAwareness();
        }
    }
    public Region GetRandomRegionWithFeature(string feature) {
        List<Region> choices = new List<Region>();
        for (int i = 0; i < allRegions.Length; i++) {
            Region region = allRegions[i];
            if (region.HasTileWithFeature(feature)) {
                choices.Add(region);
            }
        }
        if (choices.Count > 0) {
            return CollectionUtilities.GetRandomElement(choices);
        }
        return null;
    }
    public Region GetRandomRegion() {
        if (allRegions.Length > 0) {
            return CollectionUtilities.GetRandomElement(allRegions);
        }
        return null;
    }
    #endregion

    protected override void OnDestroy() {
        if (allRegions != null) {
            for (int i = 0; i < allRegions.Length; i++) {
                Region region = allRegions[i];
                region?.CleanUp();
            }    
        }
        map = null;
        outerGridList?.Clear();
        outerGridList = null;
        base.OnDestroy();
        Instance = null;
    }
}


