﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class AreaInnerTileMap : MonoBehaviour {

    public int width;
    public int height;
    private const float cellSize = 64f;

    private LocationGridTile gate;
    private Cardinal_Direction outsideDirection;

    [SerializeField] private Grid grid;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Canvas worldUICanvas;
    [SerializeField] private Transform tilemapsParent;

    [Header("Tile Maps")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap detailsTilemap;
    [SerializeField] private Tilemap strcutureTilemap;
    [SerializeField] private Tilemap objectsTilemap;
    [SerializeField] private Tilemap roadTilemap;

    [Header("Tiles")]
    [SerializeField] private RuleTile outsideTile;
    [SerializeField] private TileBase insideTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase structureTile;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase characterTile;
    [SerializeField] private TileBase supplyIconTile;
    [SerializeField] private TileBase corpseIconTile;
    [SerializeField] private TileBase bedIconTile;
    [SerializeField] private TileBase tableIconTile;
    [SerializeField] private TileBase oreIconTile;
    [SerializeField] private TileBase roadTile;
    [SerializeField] private ItemTileBaseDictionary itemTiles;
    [SerializeField] private FoodTileBaseDictionary foodTiles;

    [Header("Structure Tiles")]
    [SerializeField] private TileBase leftWall;
    [SerializeField] private TileBase rightWall;
    [SerializeField] private TileBase topWall;
    [SerializeField] private TileBase bottomWall;
    [SerializeField] private TileBase topLeftCornerWall;
    [SerializeField] private TileBase botLeftCornerWall;
    [SerializeField] private TileBase topRightCornerWall;
    [SerializeField] private TileBase botRightCornerWall;

    [Header("Dungeon Tiles")]
    [SerializeField] private TileBase dungeonWallTile;
    [SerializeField] private TileBase dungeonFloorTile;

    [Header("Oustide Tiles")]
    [SerializeField] private TileBase grassTile;
    [SerializeField] private TileBase soilTile;
    [SerializeField] private TileBase stoneTile;

    [Header("Oustide Detail Tiles")]
    [SerializeField] private TileBase bigTreeTile;
    [SerializeField] private TileBase treeTile;
    [SerializeField] private TileBase shrubTile;
    [SerializeField] private TileBase flowerTile;
    [SerializeField] private TileBase rockTile;
    [SerializeField] private TileBase randomGarbTile;

    [Header("Inside Detail Tiles")]
    [SerializeField] private TileBase crateBarrelTile;

    [Header("Characters")]
    [SerializeField] private RectTransform charactersParent;

    [Header("Events")]
    [SerializeField] private GameObject eventPopupPrefab;
    [SerializeField] private RectTransform eventPopupParent;

    [Header("Other")]
    [SerializeField] private RectTransform scrollviewContent;
    [SerializeField] private GameObject travelLinePrefab;
    [SerializeField] private Transform travelLineParent;

    public int x;
    public int y;
    public int radius;

    public float offsetX;
    public float offsetY;

    public Area area { get; private set; }
    public LocationGridTile[,] map { get; private set; }
    public List<LocationGridTile> allTiles { get; private set; }
    public List<LocationGridTile> outsideTiles { get; private set; }
    public List<LocationGridTile> insideTiles { get; private set; }

    public Tilemap charactersTM {
        get { return objectsTilemap; }
    }

    //private LocationGridTile exitTile;
    private bool isHovering;

    private enum Cardinal_Direction { North, South, East, West };
    private Dictionary<STRUCTURE_TYPE, List<Point>> structureSettings = new Dictionary<STRUCTURE_TYPE, List<Point>>() {
         { STRUCTURE_TYPE.DWELLING,
            new List<Point>(){
                new Point(4, 3),
                new Point(3, 4),
                new Point(3, 3),
                //new Point(4, 4),
            }
        },
          { STRUCTURE_TYPE.EXPLORE_AREA,
            new List<Point>(){
                //new Point(3, 5),
                //new Point(5, 4),
                new Point(4, 5),
                new Point(4, 6),
                new Point(5, 6),
                new Point(5, 5),
                new Point(6, 5), 
                new Point(6, 4)
            }
        },
        { STRUCTURE_TYPE.WAREHOUSE,
            new List<Point>(){
                new Point(4, 8),
                new Point(8, 4),
            }
        },
        { STRUCTURE_TYPE.INN,
            new List<Point>(){
                new Point(4, 6),
                new Point(6, 4),
            }
        },
        { STRUCTURE_TYPE.DUNGEON,
            new List<Point>(){
                new Point(5, 8),
                new Point(4, 9),
                new Point(8, 5),
                new Point(9, 4),
            }
        },
        { STRUCTURE_TYPE.EXIT,
            new List<Point>(){
                new Point(3, 3),
            }
        },
    };

    #region Map Generation
    public void Initialize(Area area) {
        this.area = area;
        this.name = area.name + "'s Inner Map";
        canvas.worldCamera = AreaMapCameraMove.Instance.areaMapsCamera;
        worldUICanvas.worldCamera = AreaMapCameraMove.Instance.areaMapsCamera;
        GenerateInnerStructures();
    }
    private void GenerateGrid() {
        map = new LocationGridTile[width, height];
        allTiles = new List<LocationGridTile>();
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                groundTilemap.SetTile(new Vector3Int(x, y, 0), insideTile);
                LocationGridTile tile = new LocationGridTile(x, y, groundTilemap);
                allTiles.Add(tile);
                map[x, y] = tile;
            }
        }
        allTiles.ForEach(x => x.FindNeighbours(map));
    }
    private void SplitMap() {
        outsideTiles = new List<LocationGridTile>();
        insideTiles = new List<LocationGridTile>();
        //Cardinal_Direction[] choices = Utilities.GetEnumValues<Cardinal_Direction>();
        Cardinal_Direction outsideDirection = Cardinal_Direction.West;
        IntRange xOutRange = new IntRange();
        IntRange yOutRange = new IntRange();

        this.outsideDirection = outsideDirection;

        int edgeRange = 12;
        float outerMapPercentage = 0.3f;
        if (area.areaType == AREA_TYPE.HUMAN_SETTLEMENT) {
            outerMapPercentage = 0.15f;
        } else if (area.areaType == AREA_TYPE.DUNGEON) {
            outerMapPercentage = 0.3f;
        }
        switch (outsideDirection) {
            case Cardinal_Direction.North:
                edgeRange = (int)(height * outerMapPercentage);
                xOutRange = new IntRange(0, width);
                yOutRange = new IntRange(height - edgeRange, height);
                break;
            case Cardinal_Direction.South:
                edgeRange = (int)(height * outerMapPercentage);
                xOutRange = new IntRange(0, width);
                yOutRange = new IntRange(0, edgeRange);
                break;
            case Cardinal_Direction.East:
                edgeRange = (int)(width * outerMapPercentage);
                xOutRange = new IntRange(width - edgeRange, width);
                yOutRange = new IntRange(0, height);
                break;
            case Cardinal_Direction.West:
                edgeRange = (int)(width * outerMapPercentage);
                xOutRange = new IntRange(0, edgeRange);
                yOutRange = new IntRange(0, height);
                break;
            default:
                break;
        }

        int leftMostCoordinate = 0;
        for (int i = 0; i < allTiles.Count; i++) {
            LocationGridTile currTile = allTiles[i];
            if (xOutRange.IsInRange(currTile.localPlace.x) && yOutRange.IsInRange(currTile.localPlace.y)) {
                //outside
                currTile.SetIsInside(false);
                groundTilemap.SetTile(currTile.localPlace, outsideTile);
                if (currTile.localPlace.x >= leftMostCoordinate) {
                    outsideTiles.Add(currTile);
                }
            } else {
                //inside
                currTile.SetIsInside(true);
                groundTilemap.SetTile(new Vector3Int(currTile.localPlace.x, currTile.localPlace.y, 0), insideTile);
                insideTiles.Add(currTile);
            }
        }
    }
    private void ConstructWalls() {
        List<LocationGridTile> outerTiles = new List<LocationGridTile>();
        for (int i = 0; i < insideTiles.Count; i++) {
            LocationGridTile currTile = insideTiles[i];
            if (currTile.HasOutsideNeighbour() || currTile.IsAtEdgeOfMap()) {
                outerTiles.Add(currTile);
            }
        }

        //randomly choose a gate from the outer tiles
        List<LocationGridTile> gateChoices = outerTiles.Where(
            x => Utilities.IsInRange(x.localPlace.x - 3, 0, width)
            && Utilities.IsInRange(x.localPlace.x + 3, 0, width)
            && Utilities.IsInRange(x.localPlace.y - 3, 0, height)
            && Utilities.IsInRange(x.localPlace.y + 3, 0, height)
            && !x.IsAtEdgeOfMap()
            ).ToList();
        LocationGridTile chosenGate = gateChoices[Random.Range(0, gateChoices.Count)];
        outerTiles.Remove(chosenGate);
        chosenGate.SetTileType(LocationGridTile.Tile_Type.Gate);
        insideTiles.Remove(chosenGate); //NOTE: I remove the tiles that become gates from inside tiles, so as not to include them when determining tiles with structures
        gate = chosenGate;
        detailsTilemap.SetTile(gate.localPlace, null);

        for (int i = 0; i < insideTiles.Count; i++) {
            LocationGridTile currTile = insideTiles[i];
            if (area.areaType == AREA_TYPE.DUNGEON) {
                wallTilemap.SetTile(currTile.localPlace, dungeonWallTile);
                detailsTilemap.SetTile(currTile.localPlace, null);
                if (outerTiles.Contains(currTile)) {
                    currTile.SetTileType(LocationGridTile.Tile_Type.Wall);
                }
            } else {
                if (outerTiles.Contains(currTile)) {
                    currTile.SetTileType(LocationGridTile.Tile_Type.Wall);
                    wallTilemap.SetTile(currTile.localPlace, wallTile);
                    detailsTilemap.SetTile(currTile.localPlace, null);
                }
            }
        }

        for (int i = 0; i < outerTiles.Count; i++) {
            insideTiles.Remove(outerTiles[i]); //NOTE: I remove the tiles that become walls from inside tiles, so as not to include them when determining tiles with structures
        }
    }
    #endregion

    #region Structures
    public void GenerateInnerStructures() {
        groundTilemap.ClearAllTiles();
        objectsTilemap.ClearAllTiles();
        strcutureTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        detailsTilemap.ClearAllTiles();
        roadTilemap.ClearAllTiles();
        scrollviewContent.sizeDelta = new Vector2(cellSize * width, cellSize * height);
        eventPopupParent.sizeDelta = new Vector2(cellSize * width, cellSize * height);
        scrollviewContent.anchoredPosition = Vector2.zero;
        eventPopupParent.anchoredPosition = Vector2.zero;
        GenerateGrid();
        SplitMap();
        MapPerlinDetails(outsideTiles);
        MapPerlinDetails(insideTiles);
        ConstructWalls();
        //PlaceStructures(area.GetStructures(true, true).Take(10).ToDictionary(k => k.Key, v => v.Value), insideTiles);
        PlaceStructures(area.GetStructures(true, true), insideTiles);
        //DrawDwellingTileAssets();
        PlaceStructures(area.GetStructures(false, true), outsideTiles);
        if (area.areaType != AREA_TYPE.DUNGEON) {
            GenerateRoads();
        }
        AssignOuterAreas();
        if (area.HasStructure(STRUCTURE_TYPE.EXPLORE_AREA)) {
            //GenerateExploreAreas();
            ConnectExploreAreas();
        }
    }
    private void PlaceStructures(Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures, List<LocationGridTile> sourceTiles) {
        List<LocationGridTile> elligibleTiles = new List<LocationGridTile>(sourceTiles.Where(x => !x.IsAtEdgeOfMap() && !x.HasNeighborAtEdgeOfMap() && !x.HasNeighborGate()));
        int leftMostCoordinate = elligibleTiles.Min(t => t.localPlace.x);
        int rightMostCoordinate = elligibleTiles.Max(t => t.localPlace.x);
        int topMostCoordinate = elligibleTiles.Max(t => t.localPlace.y);
        int botMostCoordinate = elligibleTiles.Min(t => t.localPlace.y);
        structures = structures.OrderBy(x => x.Key).ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in structures) {
            if (!structureSettings.ContainsKey(kvp.Key)) {
                //if (!structureSettings.ContainsKey(kvp.Key) || kvp.Key == STRUCTURE_TYPE.EXPLORE_AREA) { //skip explore areas
                continue; //skip
            }
            for (int i = 0; i < kvp.Value.Count; i++) {
                LocationStructure currStruct = kvp.Value[i];
                List<LocationGridTile> choices;
                Point currPoint = new Point(0,0);
                if (kvp.Key == STRUCTURE_TYPE.EXIT) {
                    currPoint = structureSettings[kvp.Key][0];
                    choices = GetTilesForExitStructure(sourceTiles, currPoint);
                } else {
                    List<Point> pointChoices = GetValidStructureSettings(kvp.Key, elligibleTiles, rightMostCoordinate, topMostCoordinate);
                    if (pointChoices.Count == 0) {
                        //continue;
                        throw new System.Exception("No More Tiles for" + kvp.Key + " at " + area.name);
                    }
                    currPoint = pointChoices[Random.Range(0, pointChoices.Count)];

                    choices = elligibleTiles.Where(
                    t => t.localPlace.x + currPoint.X <= rightMostCoordinate
                    && t.localPlace.y + currPoint.Y <= topMostCoordinate
                    && Utilities.ContainsRange(elligibleTiles, GetTiles(currPoint, t))).ToList();
                   
                }

                LocationGridTile chosenStartingTile = choices[Random.Range(0, choices.Count)];
                List<LocationGridTile> tiles = GetTiles(currPoint, chosenStartingTile);
                for (int j = 0; j < tiles.Count; j++) {
                    LocationGridTile currTile = tiles[j];
                    currTile.SetStructure(currStruct);
                    elligibleTiles.Remove(currTile);
                    detailsTilemap.SetTile(currTile.localPlace, null);
                    List<LocationGridTile> neighbourTiles = new List<LocationGridTile>();
                    switch (kvp.Key) {
                        case STRUCTURE_TYPE.EXIT:
                            groundTilemap.SetTile(currTile.localPlace, dungeonFloorTile);
                            //detailsTilemap.SetTile(currTile.localPlace, null);
                            currTile.SetTileType(LocationGridTile.Tile_Type.Structure);
                            break;
                        case STRUCTURE_TYPE.EXPLORE_AREA:
                            groundTilemap.SetTile(currTile.localPlace, dungeonFloorTile);
                            currTile.SetTileType(LocationGridTile.Tile_Type.Structure);
                            neighbourTiles = GetTilesInRadius(currTile, 1, false, true);
                            for (int k = 0; k < neighbourTiles.Count; k++) {
                                elligibleTiles.Remove(neighbourTiles[k]);
                                detailsTilemap.SetTile(neighbourTiles[k].localPlace, null);
                                neighbourTiles[k].SetTileState(LocationGridTile.Tile_State.Empty);
                            }
                            break;
                        //case STRUCTURE_TYPE.DWELLING:
                        //    //strcutureTilemap.SetTile(currTile.localPlace, structureTile);
                        //    groundTilemap.SetTile(currTile.localPlace, floorTile);
                        //    currTile.SetTileType(LocationGridTile.Tile_Type.Structure);
                        //    break;
                        default:
                            strcutureTilemap.SetTile(currTile.localPlace, structureTile);
                            groundTilemap.SetTile(currTile.localPlace, floorTile);
                            currTile.SetTileType(LocationGridTile.Tile_Type.Structure);
                            neighbourTiles = GetTilesInRadius(currTile, 1, false, true);
                            for (int k = 0; k < neighbourTiles.Count; k++) {
                                elligibleTiles.Remove(neighbourTiles[k]);
                                detailsTilemap.SetTile(neighbourTiles[k].localPlace, null);
                                neighbourTiles[k].SetTileState(LocationGridTile.Tile_State.Empty);
                            }
                            break;
                    }
                }
            }
        }
    }
    private void DrawDwellingTileAssets() {
        List<LocationStructure> dwellings = area.GetStructuresOfType(STRUCTURE_TYPE.DWELLING);
        if (dwellings == null) {
            return;
        }
        for (int i = 0; i < dwellings.Count; i++) {
            LocationStructure currDwelling = dwellings[i];
            for (int j = 0; j < currDwelling.tiles.Count; j++) {
                LocationGridTile currTile = currDwelling.tiles[j];
                List<TileNeighbourDirection> sameStructNeighbours = currTile.GetSameStructureNeighbourDirections();
                if (!sameStructNeighbours.Contains(TileNeighbourDirection.Left)) {
                    if (!sameStructNeighbours.Contains(TileNeighbourDirection.Top)) {
                        strcutureTilemap.SetTile(currTile.localPlace, topLeftCornerWall);
                    } else if (!sameStructNeighbours.Contains(TileNeighbourDirection.Bottom)) {
                        strcutureTilemap.SetTile(currTile.localPlace, botLeftCornerWall);
                    } else {
                        strcutureTilemap.SetTile(currTile.localPlace, leftWall);
                    }
                } else if (!sameStructNeighbours.Contains(TileNeighbourDirection.Right)) {
                    if (!sameStructNeighbours.Contains(TileNeighbourDirection.Top)) {
                        strcutureTilemap.SetTile(currTile.localPlace, topRightCornerWall);
                    } else if (!sameStructNeighbours.Contains(TileNeighbourDirection.Bottom)) {
                        strcutureTilemap.SetTile(currTile.localPlace, botRightCornerWall);
                    } else {
                        strcutureTilemap.SetTile(currTile.localPlace, rightWall);
                    }
                } else if (!sameStructNeighbours.Contains(TileNeighbourDirection.Bottom)) {
                    strcutureTilemap.SetTile(currTile.localPlace, bottomWall);
                } else if (!sameStructNeighbours.Contains(TileNeighbourDirection.Top)) {
                    strcutureTilemap.SetTile(currTile.localPlace, topWall);
                }
            }
            
        }
    }
    private void AssignOuterAreas() {
        if (area.areaType == AREA_TYPE.DUNGEON) {
            for (int i = 0; i < insideTiles.Count; i++) {
                LocationGridTile currTile = insideTiles[i];
                if (currTile.structure == null) {
                    wallTilemap.SetTile(currTile.localPlace, dungeonWallTile);
                } else {
                    wallTilemap.SetTile(currTile.localPlace, null);
                }
            }
        } else {
            if (area.HasStructure(STRUCTURE_TYPE.WORK_AREA)) {
                List<LocationGridTile> workAreaTiles = new List<LocationGridTile>();
                for (int i = 0; i < insideTiles.Count; i++) {
                    LocationGridTile currTile = insideTiles[i];
                    if (currTile.structure == null && !currTile.HasNeighborAtEdgeOfMap()) {
                        //detailsTilemap.SetTile(currTile.localPlace, insideDetailTile);
                        currTile.SetStructure(area.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA));
                        workAreaTiles.Add(currTile);
                    }
                }
                InsideMapDetails(workAreaTiles);
            } else {
                Debug.LogWarning(area.name + " doesn't have a structure for work area");
            }
        }
        if (area.HasStructure(STRUCTURE_TYPE.WILDERNESS)) {
            for (int i = 0; i < outsideTiles.Count; i++) {
                LocationGridTile currTile = outsideTiles[i];
                if (currTile.IsAtEdgeOfMap()) {
                    continue; //skip
                }
                if (currTile.structure == null) {
                    //detailsTilemap.SetTile(currTile.localPlace, outsideDetailTile);
                    if (!Utilities.IsInRange(currTile.localPlace.x, 0, 7)) {
                        currTile.SetStructure(area.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
                    }
                }

            }
        } else {
            Debug.LogWarning(area.name + " doesn't have a structure for wilderness");
        }
    }
    private List<Point> GetValidStructureSettings(STRUCTURE_TYPE type, List<LocationGridTile> elligibleTiles, int rightMostCoordinate, int topMostCoordinate) {
        List<Point> valid = new List<Point>();
        for (int i = 0; i < structureSettings[type].Count; i++) {
            Point currPoint = structureSettings[type][i];
            if(elligibleTiles.Where(
                t => t.localPlace.x + currPoint.X <= rightMostCoordinate
                && t.localPlace.y + currPoint.Y <= topMostCoordinate
                && Utilities.ContainsRange(elligibleTiles, GetTiles(currPoint, t))).Count() != 0) {
                valid.Add(currPoint);
            }
        }
        return valid;
    }
    private void GenerateRoads() {
        //List<LocationGridTile> mainRoad = new List<LocationGridTile>();
        //List<LocationGridTile> rightWalls = Utilities.Shuffle(GetColumn(width - 1));
        //for (int i = 0; i < rightWalls.Count; i++) {
        //    LocationGridTile currEndPoint = rightWalls[i];
        //    List<LocationGridTile> path = PathGenerator.Instance.GetPath(gate, currEndPoint);
        //    if (path != null) {
        //        for (int j = 0; j < path.Count; j++) {
        //            LocationGridTile currTile = path[j];
        //            if (currTile.tileType == LocationGridTile.Tile_Type.Structure) {
        //                continue; //skip
        //            }
        //            groundTilemap.SetTile(currTile.localPlace, insideTile);
        //            detailsTilemap.SetTile(currTile.localPlace, null);
        //            currTile.SetTileType(LocationGridTile.Tile_Type.Road);
        //            if (!mainRoad.Contains(currTile)) {
        //                mainRoad.Add(currTile);
        //            }
        //        }
        //        break;
        //    }
        //}


        List<LocationStructure> buildings = area.GetStructuresAtLocation(true).OrderBy(x => x.GetNearestDistanceTo(gate)).ToList();
        List<LocationGridTile> roadTiles = new List<LocationGridTile>();
        for (int i = 0; i < buildings.Count; i++) {
            LocationStructure currBuilding = buildings[i];
            if (currBuilding.structureType == STRUCTURE_TYPE.WORK_AREA) {
                continue; //skip work area
            }
            if (currBuilding.HasRoadTo(gate)) {
                continue; //skip, no need to create any road
            }

            LocationGridTile nearestTile = currBuilding.GetNearestTileTo(gate);
            if (nearestTile == null) {
                //continue;
                throw new System.Exception("There is no nearest tile to gate at " + area.name);
            }
            List<LocationGridTile> choices = new List<LocationGridTile>();
            choices.Add(gate);
            choices.AddRange(roadTiles);
            choices = choices.OrderBy(x => x.GetDistanceTo(nearestTile)).ToList();
            for (int k = 0; k < choices.Count; k++) {
                LocationGridTile currChoice = choices[k];
                List<LocationGridTile> path = PathGenerator.Instance.GetPath(nearestTile, currChoice);
                if (path != null) {
                    path.Reverse();
                    for (int j = 0; j < path.Count; j++) {
                        LocationGridTile currTile = path[j];
                        if (currTile.tileType == LocationGridTile.Tile_Type.Road) {
                            break; //skip
                        }
                        if (currTile.tileType == LocationGridTile.Tile_Type.Structure) {
                            continue; //skip
                        }
                        //groundTilemap.SetTile(currTile.localPlace, insideTile);
                        //roadTilemap.SetTile(currTile.localPlace, roadTile);
                        roadTilemap.SetTile(currTile.localPlace, insideTile);
                        detailsTilemap.SetTile(currTile.localPlace, null);
                        currTile.SetTileState(LocationGridTile.Tile_State.Empty);
                        currTile.SetTileType(LocationGridTile.Tile_Type.Road);
                        if (!roadTiles.Contains(currTile)) {
                            roadTiles.Add(currTile);
                        }
                    }
                    break;
                }
            }
        }
    }
    #endregion

    #region Exit Structure
    private List<LocationGridTile> GetTilesForExitStructure(List<LocationGridTile> sourceTiles, Point currPoint) {
        int leftMostCoordinate = sourceTiles.Min(t => t.localPlace.x);
        int rightMostCoordinate = sourceTiles.Max(t => t.localPlace.x);
        int topMostCoordinate = sourceTiles.Max(t => t.localPlace.y);
        int botMostCoordinate = sourceTiles.Min(t => t.localPlace.y);

        string summary = "Generating exit structure for " + area.name;
        summary += "\nLeft most coordinate is " + leftMostCoordinate;
        summary += "\nRight most coordinate is " + rightMostCoordinate;
        summary += "\nTop most coordinate is " + topMostCoordinate;
        summary += "\nBot most coordinate is " + botMostCoordinate;

        List<LocationGridTile> choices;

        Cardinal_Direction chosenEdge = outsideDirection;
        summary += "\nChosen edge is " + chosenEdge.ToString();
        switch (chosenEdge) {
            case Cardinal_Direction.North:
                choices = sourceTiles.Where(
                t => (t.localPlace.x == gate.localPlace.x && t.localPlace.y + currPoint.Y == topMostCoordinate + 1)
                && Utilities.ContainsRange(sourceTiles, GetTiles(currPoint, t))).ToList();
                break;
            case Cardinal_Direction.South:
                choices = sourceTiles.Where(
                t => (t.localPlace.x == gate.localPlace.x && t.localPlace.y == botMostCoordinate)
                && Utilities.ContainsRange(sourceTiles, GetTiles(currPoint, t))).ToList();
                break;
            case Cardinal_Direction.East:
                choices = sourceTiles.Where(
                t => (t.localPlace.x + currPoint.X == rightMostCoordinate + 1 && t.localPlace.y == gate.localPlace.y)
                && Utilities.ContainsRange(sourceTiles, GetTiles(currPoint, t))).ToList();
                break;
            case Cardinal_Direction.West:
                choices = sourceTiles.Where(
                t => (t.localPlace.x == leftMostCoordinate && t.localPlace.y + 1 == gate.localPlace.y)
                && Utilities.ContainsRange(sourceTiles, GetTiles(currPoint, t))).ToList();
                break;
            default:
                choices = sourceTiles.Where(
                t => t.localPlace.x + currPoint.X <= rightMostCoordinate
                && t.localPlace.y + currPoint.Y <= topMostCoordinate
                && Utilities.ContainsRange(sourceTiles, GetTiles(currPoint, t))).ToList();
                break;
        }
        Debug.Log(summary);
        return choices;
    }
    #endregion

    #region Explore Areas
    private void ConnectExploreAreas() {
        List<LocationStructure> exploreAreas = area.GetStructuresOfType(STRUCTURE_TYPE.EXPLORE_AREA);
        exploreAreas = Utilities.Shuffle(exploreAreas);
        for (int i = 0; i < exploreAreas.Count; i++) {
            LocationStructure currArea = exploreAreas[i];
            LocationStructure otherArea = exploreAreas.ElementAtOrDefault(i + 1);
            if (otherArea == null) {
                break;
            }
            //connect currArea to otherArea
            List<LocationGridTile> currAreaOuter = GetOuterTilesFrom(currArea.tiles);
            List<LocationGridTile> otherAreaOuter = GetOuterTilesFrom(otherArea.tiles);

            LocationGridTile chosenCurrArea = currAreaOuter[Random.Range(0, currAreaOuter.Count)];
            LocationGridTile chosenOtherArea = otherAreaOuter[Random.Range(0, otherAreaOuter.Count)];

            List<LocationGridTile> path = PathGenerator.Instance.GetPath(chosenCurrArea, chosenOtherArea);
            if (path != null) {
                for (int j = 0; j < path.Count; j++) {
                    if (path[j].structure == null) {
                        wallTilemap.SetTile(path[j].localPlace, null);
                        groundTilemap.SetTile(path[j].localPlace, dungeonFloorTile);
                    }
                }
            }
        }

        //connect nearest area to gate
        LocationGridTile nearestTile = null;
        float nearestDist = 99999f;
        for (int i = 0; i < exploreAreas.Count; i++) {
            LocationStructure currArea = exploreAreas[i];
            LocationGridTile tile = currArea.GetNearestTileTo(gate);
            float dist = Vector2.Distance(tile.localLocation, gate.localLocation);
            if (dist < nearestDist) {
                nearestTile = tile;
                nearestDist = dist;
            }
        }

        List<LocationGridTile> p = PathGenerator.Instance.GetPath(gate, nearestTile);
        if (p != null) {
            for (int j = 0; j < p.Count; j++) {
                if (p[j].structure == null) {
                    wallTilemap.SetTile(p[j].localPlace, null);
                    groundTilemap.SetTile(p[j].localPlace, dungeonFloorTile);
                }
            }
        }

    }
    private void GenerateExploreAreas() {
        List<LocationStructure> exploreAreas = area.GetStructuresOfType(STRUCTURE_TYPE.EXPLORE_AREA);
        List<ExploreArea> createdExploreAreas = new List<ExploreArea>();
        int xCoord = gate.localPlace.x + Random.Range(3, 5);
        List<LocationGridTile> column = GetColumn(xCoord);
        //get random tile that is 3 or 4 tiles from the entrance
        Queue<LocationGridTile> tilesToCheck = new Queue<LocationGridTile>();
        tilesToCheck.Enqueue(column[Random.Range(0, tilesToCheck.Count)]);

        while (exploreAreas.Count > createdExploreAreas.Count) {

        }
    }
    //private void GenerateExploreAreas() {
    //    //wallTilemap.gameObject.SetActive(false);
    //    //detailsTilemap.gameObject.SetActive(false);
    //    //strcutureTilemap.gameObject.SetActive(false);
    //    //objectsTilemap.gameObject.SetActive(false);
    //    List<LocationStructure> exploreAreas = area.GetStructuresOfType(STRUCTURE_TYPE.EXPLORE_AREA);
    //    List<LocationGridTile> elligibleTiles = new List<LocationGridTile>(insideTiles);
    //    List<ExploreArea> createdExploreAreas = new List<ExploreArea>();
    //    int exploreAreaCount = exploreAreas.Count;
    //    for (int i = 0; i < exploreAreaCount; i++) {
    //        LocationGridTile chosenTile = elligibleTiles[Random.Range(0, elligibleTiles.Count)];
    //        ExploreArea newEA = new ExploreArea();
    //        newEA.coreTile = chosenTile;
    //        createdExploreAreas.Add(newEA);
    //        List<LocationGridTile> nearTiles = GetTilesInRadius(chosenTile, 6, true, true);
    //        Utilities.ListRemoveRange(elligibleTiles, nearTiles);
    //    }

    //    elligibleTiles = new List<LocationGridTile>(insideTiles);
    //    for (int i = 0; i < createdExploreAreas.Count; i++) {
    //        elligibleTiles.Remove(createdExploreAreas[i].coreTile);
    //    }

    //    for (int i = 0; i < elligibleTiles.Count; i++) {
    //        LocationGridTile currTile = elligibleTiles[i];
    //        ExploreArea nearestArea = GetNearestExploreArea(createdExploreAreas, currTile);
    //        nearestArea.AddTile(currTile);
    //    }

    //    for (int i = 0; i < createdExploreAreas.Count; i++) {
    //        ExploreArea currArea = createdExploreAreas[i];
    //        List<LocationGridTile> converted = currArea.ConvertToActualArea(map);
    //        for (int j = 0; j < converted.Count; j++) {
    //            LocationGridTile currTile = converted[j];
    //            wallTilemap.SetTile(currTile.localPlace, null);
    //        }
    //    }
    //}
    private ExploreArea GetNearestExploreArea(List<ExploreArea> areas, LocationGridTile tile) {
        ExploreArea nearestArea = null;
        float nearestDist = 99999f;
        for (int i = 0; i < areas.Count; i++) {
            ExploreArea currArea = areas[i];
            float dist = currArea.DistanceFromCore(tile);
            if (dist < nearestDist) {
                nearestArea = currArea;
                nearestDist = dist;
            }
        }
        return nearestArea;
    }
    #endregion

    #region Details
    private void MapPerlinDetails(List<LocationGridTile> tiles) {
        offsetX = Random.Range(0f, 99999f);
        offsetY = Random.Range(0f, 99999f);
        int minX = tiles.Min(t => t.localPlace.x);
        int maxX = tiles.Max(t => t.localPlace.x);
        int minY = tiles.Min(t => t.localPlace.y);
        int maxY = tiles.Max(t => t.localPlace.y);

        int width = maxX - minX;
        int height = maxY - minY;

        for (int x = minX; x <= maxX; x++) {
            for (int y = minY; y <= maxY; y++) {
                LocationGridTile currTile = map[x, y];

                float xCoord = (float)x / width * 11f + offsetX;
                float yCoord = (float)y / height * 11f + offsetY;

                float xCoordDetail = (float)x / width * 8f + offsetX;
                float yCoordDetail = (float)y / height * 8f + offsetY;

                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                float sampleDetail = Mathf.PerlinNoise(xCoordDetail, yCoordDetail);

                if (sample < 0.5f) {
                    currTile.groundType = LocationGridTile.Ground_Type.Grass;
                    groundTilemap.SetTile(currTile.localPlace, grassTile);
                } else if (sample >= 0.5f && sample < 0.8f) {
                    currTile.groundType = LocationGridTile.Ground_Type.Soil;
                    groundTilemap.SetTile(currTile.localPlace, soilTile);
                } else {
                    currTile.groundType = LocationGridTile.Ground_Type.Stone;
                    groundTilemap.SetTile(currTile.localPlace, stoneTile);
                }

                if (!currTile.hasDetail) {
                    if (sampleDetail < 0.5f) {
                        if (currTile.groundType == LocationGridTile.Ground_Type.Grass) {
                            List<LocationGridTile> overlappedTiles = GetTiles(new Point(2, 2), currTile, tiles);
                            int invalidOverlap = overlappedTiles.Where(t => t.hasDetail).Count();
                            if (!currTile.IsAtEdgeOfMap() && !currTile.HasNeighborAtEdgeOfMap() && invalidOverlap == 0 && overlappedTiles.Count == 4 && Random.Range(0, 100) < 5) {
                                //big tree
                                for (int i = 0; i < overlappedTiles.Count; i++) {
                                    LocationGridTile ovTile = overlappedTiles[i];
                                    ovTile.hasDetail = true;
                                    detailsTilemap.SetTile(ovTile.localPlace, null);
                                    ovTile.SetTileState(LocationGridTile.Tile_State.Occupied);
                                }
                                detailsTilemap.SetTile(currTile.localPlace, bigTreeTile);
                                currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
                            } else {
                                currTile.hasDetail = true;
                                detailsTilemap.SetTile(currTile.localPlace, treeTile);
                                currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
                            }
                        }
                    } else {
                        currTile.hasDetail = false;
                        detailsTilemap.SetTile(currTile.localPlace, null);
                    }
                    //groundTilemap.SetColor(currTile.localPlace, new Color(sample, sample, sample));
                }
            }
        }

        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile currTile = tiles[i];
            if (!currTile.hasDetail) {
                if (Random.Range(0, 100) < 3) {
                    currTile.hasDetail = true;
                    detailsTilemap.SetTile(currTile.localPlace, flowerTile);
                    currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
                } else if (Random.Range(0, 100) < 4) {
                    currTile.hasDetail = true;
                    detailsTilemap.SetTile(currTile.localPlace, rockTile);
                    currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
                } else if (Random.Range(0, 100) < 3) {
                    currTile.hasDetail = true;
                    detailsTilemap.SetTile(currTile.localPlace, randomGarbTile);
                }
            }
        }
    }
    private void InsideMapDetails(List<LocationGridTile> insideTiles) {
        //5% of tiles that are adjacent to thin and thick walls should have crates or barrels
        List<LocationGridTile> tilesForBarrels = new List<LocationGridTile>();
        for (int i = 0; i < insideTiles.Count; i++) {
            LocationGridTile currTile = insideTiles[i];
            if (currTile.tileType != LocationGridTile.Tile_Type.Road && currTile.IsAdjacentToWall()) {
                tilesForBarrels.Add(currTile);
            }
        }

        for (int i = 0; i < tilesForBarrels.Count; i++) {
            LocationGridTile currTile = tilesForBarrels[i];
            if (Random.Range(0, 100) < 5) {
                currTile.hasDetail = true;
                detailsTilemap.SetTile(currTile.localPlace, crateBarrelTile);
                currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
            }
        }

        for (int i = 0; i < insideTiles.Count; i++) {
            LocationGridTile currTile = insideTiles[i];
            if (currTile.tileType != LocationGridTile.Tile_Type.Road && !currTile.hasDetail && Random.Range(0, 100) < 3) {
                //3% of tiles should have random garbage
                currTile.hasDetail = true;
                detailsTilemap.SetTile(currTile.localPlace, randomGarbTile);
            }
        }
    }
    #endregion

    #region Movement & Mouse Interaction
    public void LateUpdate() {
        //return;
        if (UIManager.Instance.IsMouseOnUI()) {
            return;
        }
        Vector3 mouseWorldPos = (worldUICanvas.worldCamera.ScreenToWorldPoint(Input.mousePosition));
        Vector3 localPos = grid.WorldToLocal(mouseWorldPos);
        Vector3Int coordinate = grid.LocalToCell(localPos);
        if (coordinate.x >= 0 && coordinate.x < width
            && coordinate.y >= 0 && coordinate.y < height) {
            LocationGridTile hoveredTile = map[coordinate.x, coordinate.y];
            if (hoveredTile.objHere != null) {
                ShowTileData(hoveredTile);
                if (Input.GetMouseButtonDown(0)) {
                    if (hoveredTile.objHere is Character) {
                        UIManager.Instance.ShowCharacterInfo(hoveredTile.objHere as Character);
                    }
                }
            } else {
                if (Input.GetMouseButtonDown(0)) {
                    Messenger.Broadcast(Signals.HIDE_MENUS);
                }
                UIManager.Instance.HideSmallInfo();
            }
        } else {
            UIManager.Instance.HideSmallInfo();
        }
    }
    #endregion

    #region Points of Interest
    public void PlaceObject(IPointOfInterest obj, LocationGridTile tile) {
        TileBase tileToUse = null;
        switch (obj.poiType) {
            case POINT_OF_INTEREST_TYPE.ITEM:
                tileToUse = itemTiles[(obj as SpecialToken).specialTokenType];
                break;
            case POINT_OF_INTEREST_TYPE.SUPPLY_PILE:
                tileToUse = supplyIconTile;
                break;
            case POINT_OF_INTEREST_TYPE.CORPSE:
                OnPlaceCorpseOnTile(obj as Corpse, tile);
                //tileToUse = corpseIconTile;
                break;
            case POINT_OF_INTEREST_TYPE.FOOD:
                tileToUse = foodTiles[(obj as Food).foodType];
                break;
            case POINT_OF_INTEREST_TYPE.CHARACTER:
                OnPlaceCharacterOnTile(obj as Character, tile);
                //tileToUse = characterTile;
                break;
            case POINT_OF_INTEREST_TYPE.TILE_OBJECT:
                if (obj.ToString() == "Ore") {
                    tileToUse = oreIconTile;
                } else if (obj.ToString() == "Table") {
                    tileToUse = tableIconTile;
                } else if (obj.ToString() == "Bed") {
                    tileToUse = bedIconTile;
                } else {
                    tileToUse = characterTile;
                }
                break;
            default:
                tileToUse = characterTile;
                break;
        }
        objectsTilemap.SetTile(tile.localPlace, tileToUse);
        tile.SetObjectHere(obj);
    }
    public void RemoveObject(LocationGridTile tile) {
        tile.RemoveObjectHere();
        if (tile.prefabHere != null) {
            CharacterPortrait portrait = tile.prefabHere.GetComponent<CharacterPortrait>();
            if (portrait != null) {
                portrait.SetImageRaycastTargetState(true);
            }
            ObjectPoolManager.Instance.DestroyObject(tile.prefabHere);
        }
        objectsTilemap.SetTile(tile.localPlace, null);
    }
    private void OnPlaceCharacterOnTile(Character character, LocationGridTile tile) {
        Vector3 pos = new Vector3(tile.localPlace.x + 0.5f, tile.localPlace.y + 0.5f);
        GameObject portraitGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("CharacterMarker", pos, Quaternion.identity, charactersParent);
        CharacterMarker portrait = portraitGO.GetComponent<CharacterMarker>();
        portrait.SetCharacter(character, tile);
        portrait.SetHoverAction(ShowTileData, UIManager.Instance.HideSmallInfo);
        RectTransform rect = portraitGO.transform as RectTransform;
        rect.anchoredPosition = pos;
        tile.SetPrefabHere(portraitGO);
    }
    private void OnPlaceCorpseOnTile(Corpse corpse, LocationGridTile tile) {
        Vector3 pos = new Vector3(tile.localPlace.x, tile.localPlace.y);
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool("CorpseObject", pos, Quaternion.identity, charactersParent);
        CorpseObject obj = go.GetComponent<CorpseObject>();
        obj.SetCorpse(corpse);
        RectTransform rect = go.transform as RectTransform;
        rect.anchorMax = Vector2.zero;
        rect.anchorMin = Vector2.zero;
        go.layer = LayerMask.NameToLayer("Area Maps");
        rect.anchoredPosition = pos;
        tile.SetPrefabHere(go);
    }
    #endregion

    #region Utilities
    private List<LocationGridTile> GetTiles(Point size, LocationGridTile startingTile, List<LocationGridTile> mustBeIn = null) {
        List<LocationGridTile> tiles = new List<LocationGridTile>();
        for (int x = startingTile.localPlace.x; x < startingTile.localPlace.x + size.X; x++) {
            for (int y = startingTile.localPlace.y; y < startingTile.localPlace.y + size.Y; y++) {
                if (x > map.GetUpperBound(0) || y > map.GetUpperBound(1)) {
                    continue; //skip
                }
                if (mustBeIn != null && !mustBeIn.Contains(map[x, y])) {
                    continue; //skip
                }
                tiles.Add(map[x, y]);
            }
        }
        return tiles;
    }
    private Cardinal_Direction GetOppositeDirection(Cardinal_Direction dir) {
        switch (dir) {
            case Cardinal_Direction.North:
                return Cardinal_Direction.South;
            case Cardinal_Direction.South:
                return Cardinal_Direction.North;
            case Cardinal_Direction.East:
                return Cardinal_Direction.West;
            case Cardinal_Direction.West:
                return Cardinal_Direction.East;
            default:
                return Cardinal_Direction.North;
        }
    }
    private List<LocationGridTile> GetOuterTilesFrom(List<LocationGridTile> sourceTiles) {
        List<LocationGridTile> tiles = new List<LocationGridTile>();
        for (int i = 0; i < sourceTiles.Count; i++) {
            LocationGridTile currTile = sourceTiles[i];
            for (int j = 0; j < currTile.ValidTiles.Count; j++) {
                LocationGridTile neighbour = currTile.ValidTiles.ElementAt(j);
                if (!sourceTiles.Contains(neighbour)) {
                    tiles.Add(currTile);
                    break;
                }
            }
        }
        return tiles;
    }
    private List<LocationGridTile> GetColumn(int x) {
        List<LocationGridTile> column = new List<LocationGridTile>();
        for (int i = 0; i < height; i++) {
            LocationGridTile currTile = map[x, i];
            column.Add(currTile);
        }
        return column;
    }
    #endregion

    #region Other
    public void Open() {
        this.gameObject.SetActive(true);
    }
    public void Close() {
        this.gameObject.SetActive(false);
        //if (UIManager.Instance.areaInfoUI.isShowing) {
        //    UIManager.Instance.areaInfoUI.ToggleMapMenu(false);
        //}
        isHovering = false;
    }
    private void ShowTileData(LocationGridTile tile) {
        string summary = tile.localPlace.ToString();
        summary += "\nTile Type: " + tile.tileType.ToString();
        summary += "\nTile State: " + tile.tileState.ToString();
        summary += "\nContent: " + tile.objHere?.ToString() ?? "None";
        if (tile.objHere != null) {
            summary += "\n\tObject State: " + tile.objHere.state.ToString();
        }
        //if (tile.structure != null) {
            summary += "\nStructure: " + tile.structure?.ToString() ?? "None";
        //}
        UIManager.Instance.ShowSmallInfo(summary);
    }
    private void ShowTileData(Character character, LocationGridTile tile) {
        ShowTileData(tile);
    }
    public List<LocationGridTile> GetTilesInRadius(LocationGridTile centerTile, int radius, bool includeCenterTile = false, bool includeTilesInDifferentStructure = false) {
        List<LocationGridTile> tiles = new List<LocationGridTile>();
        int mapSizeX = map.GetUpperBound(0);
        int mapSizeY = map.GetUpperBound(1);
        int x = centerTile.localPlace.x;
        int y = centerTile.localPlace.y;
        if (includeCenterTile) {
            tiles.Add(centerTile);
        }
        for (int dx = x - radius; dx <= x + radius; dx++) {
            for (int dy = y - radius; dy <= y + radius; dy++) {
                if(dx >= 0 && dx <= mapSizeX && dy >= 0 && dy <= mapSizeY) {
                    if(dx == x && dy == y) {
                        continue;
                    }
                    LocationGridTile result = map[dx, dy];
                    if(!includeTilesInDifferentStructure && result.structure != centerTile.structure) { continue; }
                    tiles.Add(result);
                }
            }
        }
        return tiles;
    }
    #endregion

    #region Travel Lines
    public void DrawLine(LocationGridTile startTile, LocationGridTile destination, Character character) {
        GameObject travelLine = ObjectPoolManager.Instance.InstantiateObjectFromPool
            (travelLinePrefab.name, Vector3.zero, Quaternion.identity, travelLineParent);
        travelLine.GetComponent<AreaMapTravelLine>().DrawLine(startTile, destination, character, this);
        
        Debug.Log(GameManager.Instance.TodayLogString() + "Drawing line at " + area.name + "'s map. From " + startTile.localPlace.ToString() + " to " + destination.localPlace.ToString());
    }
    public void DrawLineToExit(LocationGridTile startTile, Character character) {
        GameObject travelLine = ObjectPoolManager.Instance.InstantiateObjectFromPool
            (travelLinePrefab.name, Vector3.zero, Quaternion.identity, travelLineParent);
        LocationGridTile exitTile = area.structures[STRUCTURE_TYPE.EXIT][0].tiles[Random.Range(0, area.structures[STRUCTURE_TYPE.EXIT][0].tiles.Count)];
        travelLine.GetComponent<AreaMapTravelLine>().DrawLine(startTile, exitTile, character, this);
        Debug.Log(GameManager.Instance.TodayLogString() + "Drawing line at " + area.name + "'s map. From " + startTile.localPlace.ToString() + " to exit" + exitTile.localPlace.ToString());
    }
    [ContextMenu("Draw Line For Testing")]
    public void DrawLineForTesting() {
        LocationGridTile startTile = new LocationGridTile(0, 4, groundTilemap);
        LocationGridTile destinationTile = new LocationGridTile(20, 15, groundTilemap);

        GameObject travelLine = GameObject.Instantiate(travelLinePrefab, travelLineParent);
        travelLine.GetComponent<AreaMapTravelLine>().DrawLine(startTile, destinationTile, null, this);

        //(newLine.transform as RectTransform).anchoredPosition = new Vector2(32f * startTile.localPlace.x, 32f * startTile.localPlace.y);
        //float angle = Mathf.Atan2(destinationTile.worldLocation.y - startTile.worldLocation.y, destinationTile.worldLocation.x - startTile.worldLocation.x) * Mathf.Rad2Deg;
        //newLine.transform.eulerAngles = new Vector3(newLine.transform.rotation.x, newLine.transform.rotation.y, angle);

        //float distance = Vector3.Distance(startTile.worldLocation, destinationTile.worldLocation);
        //(newLine.transform as RectTransform).sizeDelta = new Vector2((distance + 1) * 32f, 15f);
    }
    [ContextMenu("Move Travel Line Content")]
    public void MoveTravelLineContent() {
        Debug.Log(grid.CellToWorld(new Vector3Int(0, 0, 0)).ToString());
        //(travelLineParent.transform as RectTransform).anchoredPosition = canvas.worldCamera.WorldToViewportPoint();
    }
    #endregion

    #region Events
    public void ShowEventPopupAt(LocationGridTile location, Log log) {
        if (location == null) {
            Debug.LogWarning(GameManager.Instance.TodayLogString() + "Passed location is null! Not showing event popup for log: " + Utilities.LogReplacer(log));
            return;
        }
        Vector3 pos = new Vector3(location.localPlace.x + 0.5f, location.localPlace.y + 0.5f);
        //Vector3 worldPos = groundTilemap.CellToWorld(location.localPlace);
        //Vector3 screenPos = worldUICanvas.worldCamera.WorldToScreenPoint(worldPos);
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(eventPopupPrefab.name, Vector3.zero, Quaternion.identity, eventPopupParent);
        go.transform.localScale = Vector3.one;
        (go.transform as RectTransform).anchoredPosition = pos;
        //(go.transform as RectTransform).OverlayPosition(worldPos, worldUICanvas.worldCamera);
        //go.transform.SetParent(eventPopupParent);


        EventPopup popup = go.GetComponent<EventPopup>();
        popup.Initialize(log, location, worldUICanvas);
        Messenger.Broadcast(Signals.EVENT_POPPED_UP, popup);
    }
    [Header("Event Popup testing")]
    [SerializeField] private int xLocation;
    [SerializeField] private int yLocation;
    [ContextMenu("Create Event Popup For testing")]
    public void CreateEventPopupForTesting() {
        LocationGridTile startTile = map[xLocation, yLocation];
        ShowEventPopupAt(startTile, null);
    }
    #endregion

    [ContextMenu("Get Radius")]
    public void GetRadius() {
        List<LocationGridTile> tiles = GetTilesInRadius(map[x, y], radius);
        for (int i = 0; i < tiles.Count; i++) {
            Debug.Log(tiles[i].localPlace.x + "," + tiles[i].localPlace.y);
        }
    }

    [ContextMenu("Get Path")]
    public void GetPath() {
        List<LocationGridTile> tiles = PathGenerator.Instance.GetPath(map[0, 0], map[5, 5]);
        if (tiles != null) {
            for (int i = 0; i < tiles.Count; i++) {
                Debug.Log(tiles[i].localPlace.x + "," + tiles[i].localPlace.y);
                groundTilemap.SetTile(tiles[i].localPlace, dungeonFloorTile);
            }
        } else {
            Debug.Log("No Path!");
        }
        
    }
}

public class ExploreArea {

    public LocationGridTile coreTile;
    public List<LocationGridTile> tiles;
    public Color color;

    public ExploreArea() {
        tiles = new List<LocationGridTile>();
        color = Random.ColorHSV();
    }

    public void AddTile(LocationGridTile tile) {
        tiles.Add(tile);
        //tile.parentTileMap.SetColor(tile.localPlace, color);
    }

    public float DistanceFromCore(LocationGridTile tile) {
        return Vector2.Distance(coreTile.localLocation, tile.localLocation);
    }

    public List<LocationGridTile> ConvertToActualArea(LocationGridTile[,] map) {
        List<LocationGridTile> near = GetTilesInRadius(map, coreTile, 4, true, true);
        List<LocationGridTile> converted = new List<LocationGridTile>();
        converted.Add(coreTile);
        for (int i = 0; i < near.Count; i++) {
            LocationGridTile currTile = near[i];
            if (tiles.Contains(currTile)) {
                converted.Add(currTile);
            }
        }
        return converted;
    }

    public List<LocationGridTile> GetTilesInRadius(LocationGridTile[,] map, LocationGridTile centerTile, int radius, bool includeCenterTile = false, bool includeTilesInDifferentStructure = false) {
        List<LocationGridTile> tiles = new List<LocationGridTile>();
        int mapSizeX = map.GetUpperBound(0);
        int mapSizeY = map.GetUpperBound(1);
        int x = centerTile.localPlace.x;
        int y = centerTile.localPlace.y;
        if (includeCenterTile) {
            tiles.Add(centerTile);
        }
        for (int dx = x - radius; dx <= x + radius; dx++) {
            for (int dy = y - radius; dy <= y + radius; dy++) {
                if (dx >= 0 && dx <= mapSizeX && dy >= 0 && dy <= mapSizeY) {
                    if (dx == x && dy == y) {
                        continue;
                    }
                    LocationGridTile result = map[dx, dy];
                    if (!includeTilesInDifferentStructure && result.structure != centerTile.structure) { continue; }
                    tiles.Add(result);
                }
            }
        }
        return tiles;
    }
}