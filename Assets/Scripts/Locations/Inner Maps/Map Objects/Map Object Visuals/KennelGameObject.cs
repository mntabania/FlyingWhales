public class KennelGameObject : TileObjectGameObject {
    public override void Initialize(TileObject tileObject) {
        base.Initialize(tileObject);
        //set the selectable to be the structure that this tile object is part of, since we want the structure to show when this object is clicked.
        selectable = tileObject.gridTileLocation.structure;
    }
}