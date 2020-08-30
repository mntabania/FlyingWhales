using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WallResouceAssets {

    [SerializeField] private WallAssetDictionary wallAssets;

    public WallAsset GetWallAsset(string assetName) {
        if (wallAssets.ContainsKey(assetName)) {
            return wallAssets[assetName];    
        }
        throw new Exception($"Wall with asset name: {assetName} could not be found.");
    }
}
