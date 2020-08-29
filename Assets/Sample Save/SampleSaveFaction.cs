using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[JsonObject(IsReference = true)]
public class SampleSaveFaction {
    public int id;
    public string name;
    public SampleSaveCharacter characterLeader;
    public List<SampleSaveCharacter> members;

    public SampleSaveFaction() {
        id = UtilityScripts.Utilities.SetID(this);
        //this.name = name;
        members = new List<SampleSaveCharacter>();
    }

    public void AddMember(SampleSaveCharacter member) {
        members.Add(member);
    }
    public void SetLeader(SampleSaveCharacter member) {
        characterLeader = member;
    }


    public string GetText() {
        string text = string.Empty;
        text = "Name: " + name;
        text += "\nID: " + id;
        text += "\nLeader: " + characterLeader.name;

        text += "\nMembers:";
        for (int i = 0; i < members.Count; i++) {
            if(i > 0) {
                text += ", ";
            }
            text += members[i].name;
        }
        text += "\nIs Leader a member: " + members.Contains(characterLeader);
        return text;
    }
}
