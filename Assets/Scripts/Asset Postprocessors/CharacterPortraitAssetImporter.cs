#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetImporters {
    public class CharacterPortraitAssetImporter : AssetPostprocessor {

        void OnPreprocessTexture() {
            if (assetPath.Contains("Portraits")) {
                TextureImporter textureImporter = (TextureImporter)assetImporter;
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spriteImportMode = SpriteImportMode.Single;
                textureImporter.mipmapEnabled = false; // we don't need mipmaps for 2D/UI Atlases
                textureImporter.spritePixelsPerUnit = 100f;
                if (textureImporter.isReadable) {
                    textureImporter.isReadable = false; // make sure Read/Write is disabled
                }
                textureImporter.filterMode = FilterMode.Point;
                textureImporter.maxTextureSize = 2048; 
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            }
        }
    }

}
#endif