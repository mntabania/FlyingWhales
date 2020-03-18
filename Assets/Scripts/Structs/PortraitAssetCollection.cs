using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PortraitAssetCollection {

    public List<Sprite> head;
    public List<Sprite> brows;
    public List<Sprite> eyes;
    public List<Sprite> mouth;
    public List<Sprite> nose;
    public List<Sprite> hair;
    public List<Sprite> mustache;
    public List<Sprite> beard;

    public PortraitAssetCollection() {
        head = new List<Sprite>();
        brows = new List<Sprite>();
        eyes = new List<Sprite>();
        mouth = new List<Sprite>();
        nose = new List<Sprite>();
        hair = new List<Sprite>();
        mustache = new List<Sprite>();
        beard = new List<Sprite>();
    }

    public void AddSpriteToCollection(string identifier, Sprite sprite) {
        switch (identifier) {
            case "Head":
                head.Add(sprite);
                break;
            case "Brows":
                brows.Add(sprite);
                break;
            case "Eyes":
                eyes.Add(sprite);
                break;
            case "Mouth":
                mouth.Add(sprite);
                break;
            case "Nose":
                nose.Add(sprite);
                break;
            case "Hair":
                hair.Add(sprite);
                break;
            case "Mustache":
                mustache.Add(sprite);
                break;
            case "Beard":
                beard.Add(sprite);
                break;
            default:
                break;
        }
    }
}
