using System.Collections.Generic;
using System.Linq;

public class RegionDivisionComponent {

    public List<RegionDivision> divisions;

    public RegionDivisionComponent() {
        divisions = new List<RegionDivision>();
    }
    public void AddRegionDivision(RegionDivision p_division) {
        divisions.Add(p_division);
    }
}

public class RegionDivision {
    public BIOMES biome { get; private set; }
    public List<HexTile> tiles { get; }

    public RegionDivision(BIOMES p_biome) {
        biome = p_biome;
        tiles = new List<HexTile>();
    }
    public RegionDivision(BIOMES p_biome, List<HexTile> p_tiles) {
        biome = p_biome;
        tiles = p_tiles;
    }
    public void AddTile(HexTile p_tile) {
        tiles.Add(p_tile);
    }
}
