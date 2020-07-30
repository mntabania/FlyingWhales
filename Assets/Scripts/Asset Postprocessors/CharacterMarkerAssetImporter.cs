#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetImporters {
    public class CharacterMarkerAssetImporter : AssetPostprocessor {

        private string[] foldersToIgnore = new string[] {
            "Dragon"
        };
        
        void OnPreprocessTexture() {
            bool shouldBeIgnored = false;
            for (int i = 0; i < foldersToIgnore.Length; i++) {
                string folderToIgnore = foldersToIgnore[i];
                if (assetPath.Contains(folderToIgnore)) {
                    shouldBeIgnored = true;
                    break;
                }
            }
            if (!shouldBeIgnored && assetPath.Contains("Character Marker") && !assetPath.Contains("Action Icons") && !assetPath.Contains("Hair")) {
                TextureImporter textureImporter = (TextureImporter)assetImporter;
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spriteImportMode = SpriteImportMode.Single;
                textureImporter.mipmapEnabled = false; // we don't need mipmaps for 2D/UI Atlases
                textureImporter.spritePixelsPerUnit = 80f;
                if (textureImporter.isReadable) {
                    textureImporter.isReadable = false; // make sure Read/Write is disabled
                }
                textureImporter.filterMode = FilterMode.Point;
                textureImporter.maxTextureSize = 128; 
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            }
        }
    }

}
#endif

