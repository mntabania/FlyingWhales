using System.Collections.Generic;
using Inner_Maps;
using Locations.Settlements;
using UtilityScripts;

public static class LocationGridTileListExtension {
    public static List<LocationGridTile> GetTilesCharacterCanGoTo(this List<LocationGridTile> p_tiles, Character p_character) {
        List<LocationGridTile> foundTiles = null;
        for (int i = 0; i < p_tiles.Count; i++) {
            LocationGridTile tile = p_tiles[i];
            if (p_character.movementComponent.HasPathToEvenIfDiffRegion(tile)) {
                if (foundTiles == null) { foundTiles = new List<LocationGridTile>(); }
                foundTiles.Add(tile);
            }
        }
        return foundTiles;
    }
    public static void PopulateListWithTilesCharacterCanGoTo(this List<LocationGridTile> p_tiles, Character p_character, List<LocationGridTile> p_outList) {
        for (int i = 0; i < p_tiles.Count; i++) {
            LocationGridTile tile = p_tiles[i];
            if (p_character.movementComponent.HasPathToEvenIfDiffRegion(tile)) {
                p_outList.Add(tile);
            }
        }
    }
    public static LocationGridTile GetFirstTileCharacterCanGoTo(this List<LocationGridTile> p_tiles, Character p_character) {
        for (int i = 0; i < p_tiles.Count; i++) {
            LocationGridTile tile = p_tiles[i];
            if (p_character.movementComponent.HasPathToEvenIfDiffRegion(tile)) {
                return tile;
            }
        }
        return null;
    }
    public static LocationGridTile GetRandomPassableTile(this List<LocationGridTile> p_tiles) {
        LocationGridTile chosenTile = null;
        List<LocationGridTile> shuffled = RuinarchListPool<LocationGridTile>.Claim();
        CollectionUtilities.Shuffle(p_tiles, shuffled);
        for (int i = 0; i < shuffled.Count; i++) {
            LocationGridTile tile = shuffled[i];
            if (tile.IsPassable()) {
                chosenTile = tile;
                break;
            }
        }
        RuinarchListPool<LocationGridTile>.Release(shuffled);
        return chosenTile;
    }
}
