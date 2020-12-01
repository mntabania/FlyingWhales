using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterClassAsset {

    [Header("Sprites")]
    public Sprite stillSprite;

    [Header("Animation Sprites")]
    public List<Sprite> animationSprites;

    public CharacterClassAsset() {
        animationSprites = new List<Sprite>();
    }
}

[System.Serializable]
public class AdditionalMarkerAsset {
    public string identifier;
    public CharacterClassAsset asset;
}
