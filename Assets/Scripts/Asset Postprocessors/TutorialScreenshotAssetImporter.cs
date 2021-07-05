#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace AssetImporters {
    public class TutorialScreenshotAssetImporter : AssetPostprocessor {
        void OnPreprocessTexture() {
            if (assetPath.Contains("Tutorial Screenshots")) {
                TextureImporter textureImporter = (TextureImporter)assetImporter;
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spriteImportMode = SpriteImportMode.Single;
                textureImporter.mipmapEnabled = false; // we don't need mipmaps for 2D/UI Atlases
                if (textureImporter.isReadable) {
                    textureImporter.isReadable = false; // make sure Read/Write is disabled
                }
                textureImporter.filterMode = FilterMode.Point;
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            }
        }
    }
}
#endif