using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class SampleSave {
    SampleSaveCharacter character1;
    SampleSaveFaction faction1;
    SampleSaveCharacter character2;
    SampleSaveFaction faction2;
    public List<SampleSaveCharacter> characters;
    public Dictionary<int, SampleSaveFaction> factions;

    public void Save() {
        SampleSaveCharacter char1 = new SampleSaveCharacter();
        SampleSaveCharacter char2 = new SampleSaveCharacter();
        SampleSaveCharacter char3 = new SampleSaveCharacter();

        SampleSaveFaction faction1 = new SampleSaveFaction();
        SampleSaveFaction faction2 = new SampleSaveFaction();

        char1.faction = faction1;
        char1.SetFaction2(faction1);

        char2.faction = faction1;
        char2.SetFaction2(faction2);

        char3.faction = faction2;
        char3.SetFaction2(faction2);

        faction1.AddMember(char1);
        faction1.AddMember(char2);

        faction2.AddMember(char3);
        faction2.AddMember(char2);

        faction1.SetLeader(char1);
        faction2.SetLeader(char3);

        characters = new List<SampleSaveCharacter>();
        characters.Add(char1);
        characters.Add(char2);
        characters.Add(char3);
        character1 = char1;
        character2 = char2;

        factions = new Dictionary<int, SampleSaveFaction>();
        factions.Add(faction1.id, faction1);
        factions.Add(faction2.id, faction2);
        this.faction1 = faction1;
        this.faction2 = faction2;
    }
}
