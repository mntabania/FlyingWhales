public class CryptChest : TileObject{
    public CryptChest() {
        Initialize(TILE_OBJECT_TYPE.CRYPT_CHEST);
    }
    public CryptChest(SaveDataTileObject data) {
        Initialize(data);
    }
}