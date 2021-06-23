using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;
using UtilityScripts;

public class GridMap : BaseMonoBehaviour {
	public static GridMap Instance;

    [Space(10)]
    [Header("Map Settings")]
    public int width;
	public int height;
    [SerializeField] internal int _borderThickness;
    
    [Space(10)]
    public Area[,] map;
    public List<Area> edgeAreas;

    #region getters
    public List<Area> allAreas => DatabaseManager.Instance.areaDatabase.allAreas;
    public Region[] allRegions => DatabaseManager.Instance != null ? DatabaseManager.Instance.regionDatabase.allRegions : null;
    public Region mainRegion => allRegions[0];
    #endregion
    
    void Awake(){
		Instance = this;
    }

    #region Grid Generation
    public void SetupInitialData(int width, int height) {
        this.width = width;
        this.height = height;
    }

    public void SetMap(Area[,] p_map, List<Area> p_areas) {
        this.map = p_map;
        for (int i = 0; i < p_areas.Count; i++) {
            Area hexTile = p_areas[i];
            DatabaseManager.Instance.areaDatabase.RegisterArea(hexTile);
        }
        edgeAreas = new List<Area>();
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Area area = map[x, y];
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1) {
                    edgeAreas.Add(area);
                }
            }    
        }
    }
    public Area GetTileFromCoordinates(int x, int y) {
        return map[x, y];
    }
    #endregion

    #region Grid Utilities
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
        if (hex.y % 2 == 1) {
            modifier = 1;
        }

        int x = hex.x - (hex.y - (modifier)) / 2;
        int z = hex.y;
        int y = -x - z;
        return new CubeCoordinate(x, y, z);
    }
    #endregion

    #region Regions
    public void SetRegions(Region[] regions) {
        DatabaseManager.Instance.regionDatabase.RegisterRegions(regions);
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
        base.OnDestroy();
        Instance = null;
    }
}


