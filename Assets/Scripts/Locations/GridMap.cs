using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

public class GridMap : MonoBehaviour {
	public static GridMap Instance;

	public GameObject goHex;
    [Space(10)]
    [Header("Map Settings")]
    public int width;
	public int height;
    [FormerlySerializedAs("_borderParent")] public Transform borderParent;
    [SerializeField] internal int _borderThickness;

    public float xOffset;
    public float yOffset;

    public int tileSize;
    [Space(10)]
    public HashSet<HexTile> outerGridList;
    public List<HexTile> normalHexTiles;
	public HexTile[,] map;
    public Region[] allRegions { get; private set; }


    #region getters/setters
    public List<HexTile> allTiles { get; private set; }
    #endregion

    void Awake(){
		Instance = this;
	}

    #region Grid Generation
    public void SetupInitialData(int width, int height) {
        this.width = width;
        this.height = height;
    }

    public void SetMap(HexTile[,] map, List<HexTile> normalHexTiles, List<HexTile> allTiles) {
        this.map = map;
        this.normalHexTiles = normalHexTiles;
        this.allTiles = allTiles;
    }
    internal void GenerateGrid(Save data) {
        this.width = data.width;
        this.height = data.height;
        float newX = xOffset * (width / 2);
        float newY = yOffset * (height / 2);
        this.transform.localPosition = new Vector2(-newX, -newY);
        map = new HexTile[width, height];
        normalHexTiles = new List<HexTile>();
        //int totalTiles = (int)width * (int)height;
        for (int i = 0; i < data.hextileSaves.Count; i++) {
            SaveDataHextile saveDataHextile = data.hextileSaves[i];
            int x = saveDataHextile.xCoordinate;
            int y = saveDataHextile.yCoordinate;
            float xPosition = x * xOffset;

            float yPosition = y * yOffset;
            if (y % 2 == 1) {
                xPosition += xOffset / 2;
            }

            GameObject hex = Instantiate(goHex) as GameObject;
            hex.transform.parent = this.transform;
            hex.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
            hex.transform.localScale = new Vector3(tileSize, tileSize, 0f);
            hex.name = $"{x},{y}";
            HexTile currHex = hex.GetComponent<HexTile>();
            normalHexTiles.Add(currHex);
            currHex.Initialize();
            saveDataHextile.Load(currHex);
            map[x, y] = currHex;
        }
        normalHexTiles.ForEach(o => o.FindNeighbours(map));
    }

    public void SetOuterGridList(HashSet<HexTile> outerTiles) {
        outerGridList = outerTiles;
        allTiles.AddRange(outerTiles);
    }
    internal void GenerateOuterGrid(Save data) {
        _borderThickness = data.borderThickness;
        int newWidth = (int) width + (_borderThickness * 2);
        int newHeight = (int) height + (_borderThickness * 2);

        float newX = xOffset * (int) (newWidth / 2);
        float newY = yOffset * (int) (newHeight / 2);

        outerGridList = new HashSet<HexTile>();
        borderParent.transform.localPosition = new Vector2(-newX, -newY);
        for (int i = 0; i < data.outerHextileSaves.Count; i++) {
            SaveDataHextile saveDataHextile = data.outerHextileSaves[i];
            int x = saveDataHextile.xCoordinate + _borderThickness;
            int y = saveDataHextile.yCoordinate + _borderThickness;

            if ((x >= _borderThickness && x < newWidth - _borderThickness) && (y >= _borderThickness && y < newHeight - _borderThickness)) {
                continue;
            }
            float xPosition = x * xOffset;

            float yPosition = y * yOffset;
            if (y % 2 == 1) {
                xPosition += xOffset / 2;
            }

            GameObject hex = GameObject.Instantiate(goHex) as GameObject;
            hex.transform.SetParent(borderParent.transform);
            hex.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
            hex.transform.localScale = new Vector3(tileSize, tileSize, 0f);
            HexTile currHex = hex.GetComponent<HexTile>();
            currHex.Initialize();
            saveDataHextile.Load(currHex);
            currHex.name = $"{currHex.xCoordinate},{currHex.yCoordinate}";

            outerGridList.Add(currHex);
            currHex.DisableColliders();
        }
        //Biomes.Instance.UpdateTileVisuals(outerGridList);
        // outerGridList.ForEach(o => o.FindNeighboursForBorders());
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
    internal HexTile GetHexTile(int xCoordinate, int yCoordinate) {
        for (int i = 0; i < normalHexTiles.Count; i++) {
            if (xCoordinate == normalHexTiles[i].xCoordinate && yCoordinate == normalHexTiles[i].yCoordinate) {
                return normalHexTiles[i];
            }
        }
        return null;
    }
    internal GameObject GetHex(string hexName, List<HexTile> tiles) {
        for (int i = 0; i < tiles.Count; i++) {
            if (hexName == tiles[i].name) {
                return tiles[i].gameObject;
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
        allRegions = regions;
        for (int i = 0; i < allRegions.Length; i++) {
            allRegions[i].FinalizeData();
            // allRegions[i].HideBorders();
        }
    }
    private Region[] CreateRandomRegions(int regionCount, List<HexTile> tiles, ref List<HexTile> remainingTiles) {
        List<HexTile> regionCoreTileChoices = new List<HexTile>(tiles
            .Where(x => x.IsAtEdgeOfMap() == false && x.HasNeighbourAtEdgeOfMap())
        );
        Region[] createdRegions = new Region[regionCount];
        for (int i = 0; i < regionCount; i++) {
            if (regionCoreTileChoices.Count == 0) {
                throw new System.Exception("No more core tiles for regions!");
            }
            HexTile chosenTile = regionCoreTileChoices[Random.Range(0, regionCoreTileChoices.Count)];
            Region newRegion = new Region(chosenTile);
            int range = 2;
            List<HexTile> tilesInRange = chosenTile.GetTilesInRange(range);
            regionCoreTileChoices.Remove(chosenTile);
            remainingTiles.Remove(chosenTile);
            for (int j = 0; j < tilesInRange.Count; j++) {
                regionCoreTileChoices.Remove(tilesInRange[j]);
            }
            createdRegions[i] = newRegion;
        }
        return createdRegions;
    }
    private List<HexTile> GetNonRegionCoreTiles(List<HexTile> tiles) {
        List<HexTile> nonCoreTiles = new List<HexTile>();
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            if (currTile.region == null || currTile.region.coreTile != currTile) {
                nonCoreTiles.Add(currTile);
            }
        }
        return nonCoreTiles;
    }
    private Region GetNearestRegionFromTile(HexTile tile) {
        Region nearestRegion = null;
        float nearestDistance = 99999f;
        for (int k = 0; k < allRegions.Length; k++) {
            Region currRegion = allRegions[k];
            float dist = Vector2.Distance(tile.transform.position, currRegion.coreTile.transform.position);
            if (dist < nearestDistance) {
                nearestRegion = currRegion;
                nearestDistance = dist;
            }
        }
        return nearestRegion;
    }
    private void RedetermineRegionCoreTiles() {
        for (int i = 0; i < allRegions.Length; i++) {
            Region region = allRegions[i];
            region.RedetermineCore();
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
    public Region GetRegionByName(string name) {
        for (int i = 0; i < allRegions.Length; i++) {
            if (allRegions[i].name == name) {
                return allRegions[i];
            }
        }
        return null;
    }
    #endregion
}


