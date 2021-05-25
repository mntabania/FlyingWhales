using Inner_Maps.Location_Structures;
using UnityEngine;
namespace Inner_Maps.Map_Objects.Map_Object_Visuals {
    public class OreVeinGameObject : TileObjectGameObject{
        [SerializeField] private SpriteSpriteDictionary _oreVeinSprite;
        
        public override void Initialize(TileObject tileObject) {
            base.Initialize(tileObject);
            LockHoverObject(); //this is so that hover highlight will not be shown on hover.
        }
        public override void UpdateTileObjectVisual(TileObject tileObject) {
            base.UpdateTileObjectVisual(tileObject);
            if (tileObject.gridTileLocation != null) {
                Sprite caveSprite = tileObject.gridTileLocation.parentMap.structureTilemap.GetSprite(tileObject.gridTileLocation.localPlace);
                if (caveSprite == null && !GameManager.Instance.gameHasStarted) {
                    //this is for cave tiles that have not yet finished generating.
                    caveSprite = tileObject.gridTileLocation.parentMap.detailsTilemap.GetSprite(tileObject.gridTileLocation.localPlace);
                }
                if (caveSprite != null && _oreVeinSprite.ContainsKey(caveSprite)) {
                    tileObject.gridTileLocation.parentMap.structureTilemap.SetColor(tileObject.gridTileLocation.localPlace, Color.white);
                    SetVisual(_oreVeinSprite[caveSprite]);
                } else {
                    tileObject.gridTileLocation.parentMap.structureTilemap.SetColor(tileObject.gridTileLocation.localPlace, Color.cyan);
                }
            }
        }
    }
}