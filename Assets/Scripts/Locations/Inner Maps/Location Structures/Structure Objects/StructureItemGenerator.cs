using System;
using UnityEngine.Tilemaps;
using UnityEngine;
using UnityEngine.Serialization;
using Inner_Maps;

public class StructureItemGenerator : MonoBehaviour {
    [Header("Walls")]
    [Tooltip("This is only relevant if blockWallsTilemap is not null.")]
    [FormerlySerializedAs("_wallType")] [SerializeField] private WALL_TYPE _blockWallType;
    [Tooltip("This is only relevant if structure uses thin walls.")]
    [FormerlySerializedAs("_wallResource")] [SerializeField] private RESOURCE _thinWallResource;

    [SerializeField] protected Tilemap _groundTileMap;
    [SerializeField] protected Tilemap _detailTileMap;
    [SerializeField] protected TilemapRenderer _groundTileMapRenderer;
    [SerializeField] protected TilemapRenderer _detailTileMapRenderer;
    [SerializeField] protected Tilemap _blockWallsTilemap;

    #region Helpers
    [Header("Wall Converter")]
    [SerializeField] private Tilemap wallTileMap;
    [SerializeField] private GameObject leftWall;
    [SerializeField] private GameObject rightWall;
    [SerializeField] private GameObject topWall;
    [SerializeField] private GameObject bottomWall;
    [SerializeField] private GameObject cornerPrefab;

    [Header("Objects")]
    [FormerlySerializedAs("_objectsParent")] public Transform objectsParent;

    private ThinWallGameObject[] wallVisuals;

    //private void UpdateSortingOrders() {
    //    _groundTileMapRenderer.sortingOrder = InnerMapManager.GroundTilemapSortingOrder + 5;
    //    _detailTileMapRenderer.sortingOrder = InnerMapManager.DetailsTilemapSortingOrder;
    //    for (int i = 0; i < wallVisuals.Length; i++) {
    //        ThinWallGameObject wallVisual = wallVisuals[i];
    //        wallVisual.UpdateSortingOrders(_groundTileMapRenderer.sortingOrder + 2);
    //    }
    //}

    [ContextMenu("Convert Walls")]
    public void ConvertWalls() {
        wallTileMap.CompressBounds();
        BoundsInt bounds = wallTileMap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++) {
            for (int y = bounds.yMin; y < bounds.yMax; y++) {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = wallTileMap.GetTile(pos);

                Vector3 worldPos = wallTileMap.CellToWorld(pos);

                if (tile != null) {
                    Vector2 centeredPos = new Vector2(worldPos.x + 0.5f, worldPos.y + 0.5f);
                    if (tile.name.Contains("Door")) {
                        continue; //skip
                    }
                    ThinWallGameObject wallVisual = null;
                    if (tile.name.Contains("Left")) {
                        wallVisual = InstantiateWall(leftWall, centeredPos, wallTileMap.transform, _thinWallResource != RESOURCE.WOOD);
                    }
                    if (tile.name.Contains("Right")) {
                        wallVisual = InstantiateWall(rightWall, centeredPos, wallTileMap.transform, _thinWallResource != RESOURCE.WOOD);
                    }
                    if (tile.name.Contains("Bot")) {
                        wallVisual = InstantiateWall(bottomWall, centeredPos, wallTileMap.transform, _thinWallResource != RESOURCE.WOOD);
                    }
                    if (tile.name.Contains("Top")) {
                        wallVisual = InstantiateWall(topWall, centeredPos, wallTileMap.transform, _thinWallResource != RESOURCE.WOOD);
                    }

                    if (wallVisual != null) {
                        Vector3 cornerPos = centeredPos;
                        if (tile.name.Contains("BotLeft")) {
                            cornerPos.x -= 0.5f;
                            cornerPos.y -= 0.5f;
                            Instantiate(cornerPrefab, cornerPos, Quaternion.identity, wallVisual.transform);
                        } else if (tile.name.Contains("BotRight")) {
                            cornerPos.x += 0.5f;
                            cornerPos.y -= 0.5f;
                            Instantiate(cornerPrefab, cornerPos, Quaternion.identity, wallVisual.transform);
                        } else if (tile.name.Contains("TopLeft")) {
                            cornerPos.x -= 0.5f;
                            cornerPos.y += 0.5f;
                            Instantiate(cornerPrefab, cornerPos, Quaternion.identity, wallVisual.transform);
                        } else if (tile.name.Contains("TopRight")) {
                            cornerPos.x += 0.5f;
                            cornerPos.y += 0.5f;
                            Instantiate(cornerPrefab, cornerPos, Quaternion.identity, wallVisual.transform);
                        }
                        if (_thinWallResource != RESOURCE.WOOD) {
                            //only update asset if wall resource is not wood.
                            wallVisual.UpdateWallAssets(_thinWallResource);
                        }
                    }
                }
            }
        }
    }
    private ThinWallGameObject InstantiateWall(GameObject wallPrefab, Vector3 centeredPos, Transform parent, bool updateWallAsset) {
        GameObject wallGO = Instantiate(wallPrefab, parent);
        wallGO.transform.position = centeredPos;
        ThinWallGameObject wallVisual = wallGO.GetComponent<ThinWallGameObject>();
        if (updateWallAsset) {
            wallVisual.UpdateWallAssets(_thinWallResource);
        }
        return wallVisual;
    }

    [ContextMenu("Convert Objects")]
    public void ConvertObjects() {
        UtilityScripts.Utilities.DestroyChildren(objectsParent);
        _detailTileMap.CompressBounds();
        // Material mat = Resources.Load<Material>("Fonts & Materials/2D Lighting");
        BoundsInt bounds = _detailTileMap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++) {
            for (int y = bounds.yMin; y < bounds.yMax; y++) {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = _detailTileMap.GetTile(pos);
                Vector3 worldPos = _detailTileMap.CellToWorld(pos);

                if (tile != null) {
                    Matrix4x4 m = _detailTileMap.GetTransformMatrix(pos);
                    Vector2 centeredPos = new Vector2(worldPos.x + 0.5f, worldPos.y + 0.5f);

                    GameObject newGo = new GameObject("StructureTemplateObjectData");
                    newGo.layer = LayerMask.NameToLayer("Area Maps");
                    newGo.transform.SetParent(objectsParent);
                    newGo.transform.position = centeredPos;
                    newGo.transform.localRotation = m.rotation;

                    StructureTemplateObjectData stod = newGo.AddComponent<StructureTemplateObjectData>();
                    SpriteRenderer spriteRenderer = newGo.AddComponent<SpriteRenderer>();

                    spriteRenderer.sortingLayerName = "Area Maps";
                    spriteRenderer.sortingOrder = 60;
                    // spriteRenderer.material = mat;

                    int index = tile.name.IndexOf("#", StringComparison.Ordinal);
                    string tileObjectName = tile.name;
                    if (index != -1) {
                        tileObjectName = tile.name.Substring(0, index);
                    }
                    tileObjectName = tileObjectName.ToUpper();
                    TILE_OBJECT_TYPE tileObjectType = (TILE_OBJECT_TYPE)Enum.Parse(typeof(TILE_OBJECT_TYPE), tileObjectName);
                    stod.tileObjectType = tileObjectType;
                    stod.spriteRenderer = spriteRenderer;
                    spriteRenderer.sprite = _detailTileMap.GetSprite(pos);
                }
            }
        }
        _detailTileMap.enabled = false;
        _detailTileMapRenderer.enabled = false;
    }
	#endregion
}