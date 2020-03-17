#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetImporters {
    public class MapObjectAssetImporter : AssetPostprocessor {

        void OnPreprocessTexture() {
            if (assetPath.Contains("Textures/Interior Map")) {
                TextureImporter textureImporter = (TextureImporter)assetImporter;
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spriteImportMode = SpriteImportMode.Single;
                textureImporter.mipmapEnabled = false; // we don't need mipmaps for 2D/UI Atlases
                textureImporter.spritePixelsPerUnit = 64f;
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