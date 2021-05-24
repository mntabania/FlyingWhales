using System;
using System.Collections.Generic;
using System.IO;
using Inner_Maps.Location_Structures;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UtilityScripts;

[CreateAssetMenu(fileName = "New Structure Data", menuName = "Scriptable Objects/Structure Data")]
public class StructureData : ScriptableObject {
    [FormerlySerializedAs("structureSprite")] [SerializeField] private Sprite _structureSprite;
    [FormerlySerializedAs("itemGenerationSetting")] [SerializeField] private ItemGenerationSetting _itemGenerationSetting;
    [SerializeField] private LocationStructurePrefabDictionary structurePrefabs;

    #region getters
    public ItemGenerationSetting itemGenerationSetting => _itemGenerationSetting;
    public Sprite structureSprite => _structureSprite;
    #endregion
    
    public StructureData() {
        structurePrefabs = new LocationStructurePrefabDictionary();
    }
    
    public void AddStructurePrefab(StructureSetting p_structureSetting, GameObject p_prefab) {
        if (!structurePrefabs.ContainsKey(p_structureSetting)) {
            structurePrefabs.Add(p_structureSetting, new List<GameObject>());
        }
        structurePrefabs[p_structureSetting].Add(p_prefab);
    }

    public List<GameObject> GetStructurePrefabs(StructureSetting p_structureSetting) {
        if (structurePrefabs.ContainsKey(p_structureSetting)) {
            return structurePrefabs[p_structureSetting];
        }
        throw new Exception($"No structure prefabs for {p_structureSetting}");
    }
}

#if UNITY_EDITOR
public class CreateStructureScriptableObjects {
    [MenuItem("Assets/Create Structure Data Assets")]
    public static void CreateStructureDataAssets() {
        STRUCTURE_TYPE[] arrStructureTypes = CollectionUtilities.GetEnumValues<STRUCTURE_TYPE>();
        string baseStructurePrefabsPath = "Assets/Prefabs/Area Maps/Structure Prefabs/";
        for (int i = 0; i < arrStructureTypes.Length; i++) {
            STRUCTURE_TYPE structureType = arrStructureTypes[i];
            if (structureType == STRUCTURE_TYPE.WILDERNESS || structureType == STRUCTURE_TYPE.CAVE || structureType == STRUCTURE_TYPE.OCEAN) {
                continue;
            }
            StructureData structureDataAsset = ScriptableObject.CreateInstance<StructureData>();
            string currentStructurePrefabsPath = $"{baseStructurePrefabsPath}{structureType.ToString()}/";
            if (Directory.Exists(currentStructurePrefabsPath)) {
                string[] resourceDirectories = Directory.GetDirectories(currentStructurePrefabsPath);
                if (resourceDirectories.Length > 0) {
                    for (int j = 0; j < resourceDirectories.Length; j++) {
                        string resourceDirectoryPath = resourceDirectories[j];
                        string resourceStr = new DirectoryInfo(resourceDirectoryPath).Name.ToUpper();
                        StructureSetting structureSetting;
                        if (resourceStr.Equals("Corrupted", StringComparison.InvariantCultureIgnoreCase)) {
                            //NOTE: For now corrupted structures are ALWAYS made of STONE! If that ever changes, then this will need to be changed.
                            structureSetting = new StructureSetting(structureType, RESOURCE.STONE, true);
                        } else {
                            if (Enum.TryParse(resourceStr, out RESOURCE resource)) {
                                structureSetting = new StructureSetting(structureType, resource);    
                            } else {
                                throw new Exception($"Cannot convert folder {resourceDirectoryPath} to a resource type!");
                            }
                        }
                        string[] structurePrefabForResourceFiles = Directory.GetFiles(resourceDirectoryPath, "*.prefab");
                        for (int k = 0; k < structurePrefabForResourceFiles.Length; k++) {
                            string currentStructurePrefabPath = structurePrefabForResourceFiles[k];
                            GameObject loadedPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(currentStructurePrefabPath, typeof(GameObject));
                            structureDataAsset.AddStructurePrefab(structureSetting, loadedPrefab);
                        }
                    }    
                }
                else {
                    //structure type does not use resources
                    string[] structurePrefabFiles = Directory.GetFiles(currentStructurePrefabsPath, "*.prefab");
                    StructureSetting structureSetting = new StructureSetting(structureType, RESOURCE.NONE);
                    for (int k = 0; k < structurePrefabFiles.Length; k++) {
                        string currentStructurePrefabPath = structurePrefabFiles[k];
                        GameObject loadedPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(currentStructurePrefabPath, typeof(GameObject));
                        structureDataAsset.AddStructurePrefab(structureSetting, loadedPrefab);
                    }
                }
                
            
                AssetDatabase.CreateAsset(structureDataAsset, $"Assets/Scriptable Object Assets/Structure Data/{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(structureType.ToString())} Data.asset");
                AssetDatabase.SaveAssets();    
            }
        }
    }
}
#endif