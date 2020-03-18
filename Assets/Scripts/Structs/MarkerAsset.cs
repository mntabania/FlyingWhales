using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MarkerAsset {
    public CharacterClassAssetDictionary characterClassAssets;

    public MarkerAsset() {
        characterClassAssets = new CharacterClassAssetDictionary();
    }
}
