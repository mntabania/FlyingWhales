using UnityEngine;
using UnityEngine.Tilemaps;
using UtilityScripts;

[CreateAssetMenu(fileName = "New Tile Object Data", menuName = "Scriptable Objects/Tile Object Data")]
public class TileObjectScriptableObject : ScriptableObject {
    [Header("Assets")] 
    public TileBase defaultTileMapAsset;
    public Sprite defaultSprite;
    public TileObjectTileSetting tileObjectAssets;
    public TileObjectTileSetting corruptedTileObjectAssets;

    public TileObjectScriptableObject() {
        tileObjectAssets = new TileObjectTileSetting();
        corruptedTileObjectAssets = new TileObjectTileSetting();
    }

    public TileBase GetTileBaseToUse(BIOMES p_biome) {
        if (!tileObjectAssets.biomeAssets.ContainsKey(p_biome)) {
            p_biome = BIOMES.NONE;
        }
        return CollectionUtilities.GetRandomElement(tileObjectAssets.biomeAssets[p_biome].tileBase);
    }
}
