using System.Collections.Generic;
using UnityEngine;

public static class AreaExtensions {
    public static void PopulateAreasInRange(this Area p_area, List<Area> areasInRange, int p_range, bool sameRegionOnly = true) {
        CubeCoordinate cube = OddRToCube(new Vector2Int(p_area.areaData.xCoordinate, p_area.areaData.yCoordinate));
        for (int dx = -p_range; dx <= p_range; dx++) {
            for (int dy = Mathf.Max(-p_range, -dx - p_range); dy <= Mathf.Min(p_range, -dx + p_range); dy++) {
                int dz = -dx - dy;
                HexCoordinate hex = CubeToOddR(new Vector3Int(cube.x + dx, cube.y + dy, cube.z + dz));
                if (hex.x >= 0 && hex.y >= 0 && hex.x < GridMap.Instance.width && hex.y < GridMap.Instance.height && !(hex.x == p_area.areaData.xCoordinate && hex.y == p_area.areaData.yCoordinate)) {
                    Area hextile = GridMap.Instance.map[hex.x, hex.y];
                    if(!sameRegionOnly || hextile.region == p_area.region) {
                        areasInRange.Add(hextile);
                    }
                }
            }
        }
    }
    
    private static HexCoordinate CubeToOddR(Vector3Int cube) {
        int modifier = 0;
        if (cube.z % 2 == 1) {
            modifier = 1;
        }
        int col = cube.x + (cube.z - (modifier)) / 2;
        int row = cube.z;
        return new HexCoordinate(col, row);
    }
    private static CubeCoordinate OddRToCube(Vector2Int hex) {
        int modifier = 0;
        if (hex.y % 2 == 1) {
            modifier = 1;
        }

        int x = hex.x - (hex.y - (modifier)) / 2;
        int z = hex.y;
        int y = -x - z;
        return new CubeCoordinate(x, y, z);
    }
}
