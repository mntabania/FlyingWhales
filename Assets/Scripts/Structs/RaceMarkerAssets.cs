using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RaceMarkerAsset {
    public string raceName;
    public RACE race;
    public MarkerAsset neutralAssets;
    public MarkerAsset maleAssets;
    public MarkerAsset femaleAssets;

    public RaceMarkerAsset(RACE race) {
        this.race = race;
        raceName = race.ToString();
        neutralAssets = new MarkerAsset();
        maleAssets = new MarkerAsset();
        femaleAssets = new MarkerAsset();
    }

    public MarkerAsset GetMarkerAsset(GENDER gender) {
        switch (gender) {
            case GENDER.MALE:
                return maleAssets;
            case GENDER.FEMALE:
                return femaleAssets;
            default:
                return null;
        }
    }
}
