﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tutorial;
using UnityEditor;
using UnityEngine;

public class WorldConfigManager : MonoBehaviour {

    public static WorldConfigManager Instance;
    
    [Header("Item Generation")] 
    public ItemGenerationSetting worldWideItemGenerationSetting;
    public List<TILE_OBJECT_TYPE> initialArtifactChoices;

    [Header("Testing")] 
    [SerializeField] private bool _disableLogs;
    public bool useRandomGenerationForScenarioMaps;
    public MapGenerationData mapGenerationData;    
    
    
    #region Getters
    public bool isTutorialWorld => WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private bool disableLogs => _disableLogs;
#else
    public bool disableLogs => true;
#endif
    #endregion
    
    private void Awake() {
        if (disableLogs) {
            Debug.unityLogger.logEnabled = false; 
        }
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(Instance.gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

#if UNITY_EDITOR
    [MenuItem("Tools/Force Garbage Collection")]
    static void GarbageCollect() {
        EditorUtility.UnloadUnusedAssetsImmediate();
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }
#endif
}