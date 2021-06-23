#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Inner_Maps;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace UtilityScripts {
    public class EditorUtilities {
        [MenuItem("Assets/Create Missing Tile Object Scriptable Objects")]
        public static void CreateMissingTileObjectAssets() {
            List<TILE_OBJECT_TYPE> tileObjectTypes = CollectionUtilities.GetEnumValues<TILE_OBJECT_TYPE>().ToList(); //new List<TILE_OBJECT_TYPE>() {TILE_OBJECT_TYPE.ANIMAL_MEAT};
            List<TILE_OBJECT_TYPE> missingTileObjects = new List<TILE_OBJECT_TYPE>();

            List<TileBase> allTileBases = GetAllTileBases();
            List<Sprite> allObjectSprites = GetAllObjectSprites();
            for (int i = 0; i < tileObjectTypes.Count; i++) {
                TILE_OBJECT_TYPE tileObjectType = tileObjectTypes[i];
                string path = $"Assets/Resources/Tile Object Data/{tileObjectType.ToString()}.asset";
                
                TileObjectScriptableObject existingAsset = (TileObjectScriptableObject)AssetDatabase.LoadAssetAtPath(path, typeof(TileObjectScriptableObject));
                if (existingAsset == null) {
                    TileObjectScriptableObject asset = ScriptableObject.CreateInstance<TileObjectScriptableObject>();
                    // if (InnerMapManager.Instance != null && InnerMapManager.Instance.assetManager.tileObjectTiles.ContainsKey(tileObjectType)) {
                    //     TileObjectTileSetting setting = InnerMapManager.Instance.assetManager.tileObjectTiles[tileObjectType];
                    //     asset.tileObjectAssets = setting;
                    //     asset.defaultSprite = setting.biomeAssets[BIOMES.NONE].activeTile.FirstOrDefault();
                    // } else {
                    //     missingTileObjects.Add(tileObjectType);
                    // }
                    // if (InnerMapManager.Instance != null && InnerMapManager.Instance.assetManager.corruptedTileObjectAssets.ContainsKey(tileObjectType)) {
                    //     TileObjectTileSetting corruptedSetting = InnerMapManager.Instance.assetManager.corruptedTileObjectAssets[tileObjectType];    
                    //     asset.corruptedTileObjectAssets = corruptedSetting;
                    // }
                    //
                    // TileBase tileBase = (TileBase)AssetDatabase.LoadAssetAtPath($"Assets/Tile Map Assets/Interior Map Tiles/Objects/{tileObjectType.ToString()}.asset", typeof(TileBase));
                    // if (tileBase == null) {
                    //     tileBase = (TileBase)AssetDatabase.LoadAssetAtPath($"Assets/Tile Map Assets/Interior Map Tiles/Objects/{tileObjectType.ToString()}#1.asset", typeof(TileBase));
                    //     if (tileBase == null && asset.defaultSprite != null) {
                    //         tileBase = GetTileBaseFor(asset.defaultSprite, allTileBases);
                    //     }
                    // }
                    // if (tileBase == null) {
                    //     //create tilebase for tile object
                    //     Sprite sprite = asset.defaultSprite != null ? asset.defaultSprite : GetSpriteFor(tileObjectType, allObjectSprites);
                    //     if (sprite != null) {
                    //         CustomTileBase createdTileBase = ScriptableObject.CreateInstance<CustomTileBase>();
                    //         createdTileBase.sprite = sprite;
                    //         AssetDatabase.CreateAsset(createdTileBase, $"Assets/Tile Map Assets/Interior Map Tiles/{sprite.name}.asset");
                    //         allTileBases.Add(createdTileBase);
                    //         tileBase = createdTileBase;
                    //         Debug.Log($"Created {createdTileBase.name}");
                    //     }
                    // }
                    // asset.defaultTileMapAsset = tileBase;
                    // if (asset.tileObjectAssets.biomeAssets == null) {
                    //     asset.tileObjectAssets.biomeAssets = new TileObjectBiomeAssetDictionary();
                    //     if (!asset.tileObjectAssets.biomeAssets.ContainsKey(BIOMES.NONE)) {
                    //         asset.tileObjectAssets.biomeAssets.Add(BIOMES.NONE, new BiomeTileObjectTileSetting());
                    //     }
                    // }
                    // if (tileBase != null) {
                    //     asset.tileObjectAssets.biomeAssets[BIOMES.NONE].tileBase.Add(tileBase);    
                    // }
                    AssetDatabase.CreateAsset(asset, path);
                    Debug.Log($"Created new TileObjectScriptableObject {asset.name}");
                }
            }
            Debug.LogWarning($"Missing Tile Objects: {missingTileObjects.ComafyList()}");
            AssetDatabase.SaveAssets();
        }

        private static Sprite GetSpriteFor(TILE_OBJECT_TYPE tileObjectType, List<Sprite> choices) {
            string tileObjectName = tileObjectType.ToString();
            string normalizedTileObjectName = Utilities.NormalizeStringUpperCaseFirstLetters(tileObjectName);
            string normalizedTileObjectName2 = Utilities.NormalizeNoSpaceString(tileObjectName);
            for (int i = 0; i < choices.Count; i++) {
                Sprite sprite = choices[i];
                if (sprite.name == tileObjectName || sprite.name == normalizedTileObjectName || sprite.name == normalizedTileObjectName2 ||
                    sprite.name.CaseInsensitiveContains(tileObjectName) || sprite.name.CaseInsensitiveContains(normalizedTileObjectName) || sprite.name.CaseInsensitiveContains(normalizedTileObjectName2)) {
                    return sprite;
                }
            }
            return null;
        }
        private static TileBase GetTileBaseFor(Sprite sprite, List<TileBase> tileBases) {
            for (int i = 0; i < tileBases.Count; i++) {
                TileBase tileBase = tileBases[i];
                if (tileBase.name == sprite.name) {
                    return tileBase;
                }
            }
            return null;
        }
        private static List<Sprite> GetAllObjectSprites() {
            List<Sprite> sprites = new List<Sprite>();
            string assetPath = "Assets/Textures/Interior Map/Objects/";
            string[] allFiles = Directory.GetFiles(assetPath, "*.png", SearchOption.AllDirectories);

            foreach (var file in allFiles) {
                FileInfo fileInfo = new FileInfo(file);
                string fullFilePath = fileInfo.FullName;
                fullFilePath = fullFilePath.Replace(@"F:\Repositories\FlyingWhales\", "");
                Sprite loadedSprite = (Sprite)UnityEditor.AssetDatabase.LoadAssetAtPath(fullFilePath, typeof(Sprite));
                if (loadedSprite != null) {
                    sprites.Add(loadedSprite);
                    
                }
            }
            return sprites;
        }
        private static List<TileBase> GetAllTileBases() {
            List<TileBase> sprites = new List<TileBase>();
            string assetPath = "Assets/Tile Map Assets/Interior Map Tiles/";
            string[] allFiles = Directory.GetFiles(assetPath, "*.asset", SearchOption.AllDirectories);

            foreach (var file in allFiles) {
                FileInfo fileInfo = new FileInfo(file);
                string fullFilePath = fileInfo.FullName;
                fullFilePath = fullFilePath.Replace(@"F:\Repositories\FlyingWhales\", "");
                TileBase loadedSprite = (TileBase)UnityEditor.AssetDatabase.LoadAssetAtPath(fullFilePath, typeof(TileBase));
                if (loadedSprite != null) {
                    sprites.Add(loadedSprite);
                    
                }
            }
            return sprites;
        }
    }
}
#endif