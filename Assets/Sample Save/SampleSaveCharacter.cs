using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[JsonObject(IsReference = true)]
public class SampleSaveCharacter {
    public int id;
    //private int hiddenID;
    public int exposedID;
    public string name;

    public SampleSaveFaction faction;
    public SampleSaveFaction faction2;

    public SampleSaveCharacter() {
        id = UtilityScripts.Utilities.SetID(this);
        //hiddenID = id;
        exposedID = id;
        //this.name = name;
    }

    public void SetFaction2(SampleSaveFaction faction2) {
        this.faction2 = faction2;
    }

    public string GetText() {
        string text = string.Empty;
        text = "Name: " + name;
        text += "\nID: " + id;
        //text += "\nHidden ID: " + hiddenID;
        text += "\nExposed ID: " + exposedID;
        text += "\nFaction 1: " + faction.name;
        text += "\nFaction 2: " + faction2.name;
        text += "\nFaction 1 == 2: " + (faction == faction2);
        return text;
    }
}