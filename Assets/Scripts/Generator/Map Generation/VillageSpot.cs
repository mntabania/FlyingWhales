using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class VillageSpot {
    public Area mainSpot { get; private set; }
    public List<Area> reservedAreas { get; private set; }

    public VillageSpot(Area p_spot, List<Area> p_areas) {
        mainSpot = p_spot;
        reservedAreas = new List<Area>(p_areas);
    }
    public VillageSpot(Area p_spot) {
        mainSpot = p_spot;
        reservedAreas = new List<Area> {p_spot};
    }
    public VillageSpot(SaveDataVillageSpot p_data) {
        mainSpot = GameUtilities.GetHexTileGivenCoordinates(p_data.mainArea, GridMap.Instance.map);
        reservedAreas = GameUtilities.GetHexTilesGivenCoordinates(p_data.reservedAreas, GridMap.Instance.map);
    }
    public override string ToString() {
        return mainSpot.ToString();
    }
    private void ColorVillageSpots(Color p_color) {
        p_color.a = 0.8f;
        for (int i = 0; i < reservedAreas.Count; i++) {
            Area area = reservedAreas[i];
            ColorArea(area, p_color);
        }
        Color color = Color.black;
        color.a = 0.8f;
        ColorArea(mainSpot, color);
    }
    public void ColorArea(Area p_area, Color p_color) {
        for (int i = 0; i < p_area.gridTileComponent.gridTiles.Count; i++) {
            LocationGridTile tile = p_area.gridTileComponent.gridTiles[i];
            tile.parentMap.perlinTilemap.SetTile(tile.localPlace, InnerMapManager.Instance.assetManager.grassTile);
            tile.parentMap.perlinTilemap.SetColor(tile.localPlace, p_color);
        }
    }
    public void AddWaterAreas(List<Area> p_areas) {
        reservedAreas.AddRange(p_areas);
        // color.a = 0.8f;
        // for (int i = 0; i < p_areas.Count; i++) {
        //     Area area = p_areas[i];
        //     ColorArea(area, color);
        // }
    }
    public void AddCaveAreas(List<Area> p_areas) {
        reservedAreas.AddRange(p_areas);
        // color.a = 0.8f;
        // for (int i = 0; i < p_areas.Count; i++) {
        //     Area area = p_areas[i];
        //     ColorArea(area, color);
        // }
    }
}

public class SaveDataVillageSpot : SaveData<VillageSpot> {
    public Point mainArea;
    public Point[] reservedAreas;
    public override void Save(VillageSpot data) {
        base.Save(data);
        mainArea = new Point(data.mainSpot.areaData.xCoordinate, data.mainSpot.areaData.yCoordinate);
        reservedAreas = new Point[data.reservedAreas.Count];
        for (int i = 0; i < data.reservedAreas.Count; i++) {
            Area area = data.reservedAreas[i];
            reservedAreas[i] = new Point(area.areaData.xCoordinate, area.areaData.yCoordinate);
        }
    }
    public override VillageSpot Load() {
        return new VillageSpot(this);
    }
}