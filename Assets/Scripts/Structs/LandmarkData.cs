﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;

[System.Serializable]
public struct LandmarkData {
    [Header("General Data")]
    public string landmarkTypeString;
    public LANDMARK_TYPE landmarkType;
    public int buildDuration; //how many ticks to build this landmark
    public string description;
    public List<LANDMARK_TAG> uniqueTags;
    public Sprite landmarkObjectSprite;
    [FormerlySerializedAs("landmarkPortrait")] public Sprite defaultLandmarkPortrait;
    public BiomeLandmarkSpriteListDictionary biomeTileSprites;
    public List<LandmarkStructureSprite> neutralTileSprites; //These are the sprites that will be used if landmark is not owned by a race
    public List<LandmarkStructureSprite> humansLandmarkTileSprites;
    public List<LandmarkStructureSprite> elvenLandmarkTileSprites;

    public void ConstructData() { }
}
