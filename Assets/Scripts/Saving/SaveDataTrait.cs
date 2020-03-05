using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

//IMPORTANT NOTE: Class name format for all derived classes of SaveDataTrait must be: "SaveData" + [Trait Name], example: SaveDataKleptomaniac
[System.Serializable]
public class SaveDataTrait {
    public string name;
    public int level;
    public int daysDuration;
    public bool isDisabled;
    public int responsibleCharacterID;
    public List<int> responsibleCharacterIDs;
    public List<TraitEffect> effects;

    //Date Established
    public int month;
    public int day;
    public int year;
    public int tick;

    public virtual void Save(Trait trait) {
        name = trait.name;
        level = trait.level;
        daysDuration = trait.ticksDuration;
        
        if(trait.responsibleCharacter == null) {
            responsibleCharacterID = -1;
        } else {
            responsibleCharacterID = trait.responsibleCharacter.id;
        }

        responsibleCharacterIDs = new List<int>();
        if (trait.responsibleCharacters != null) {
            for (int i = 0; i < trait.responsibleCharacters.Count; i++) {
                responsibleCharacterIDs.Add(trait.responsibleCharacters[i].id);
            }
        }

        //effects = trait.effects;

        month = trait.dateEstablished.month;
        day = trait.dateEstablished.day;
        year = trait.dateEstablished.year;
        tick = trait.dateEstablished.tick;
    }

    public virtual Trait Load(ref Character responsibleCharacter) {
        if(responsibleCharacterID != -1) {
            responsibleCharacter = CharacterManager.Instance.GetCharacterByID(responsibleCharacterID);
        }
        Trait trait = null;
        if (TraitManager.Instance.IsInstancedTrait(name)) {
            trait = TraitManager.Instance.CreateNewInstancedTraitClass(name);
        } else {
            trait = TraitManager.Instance.allTraits[name];
        }
        trait.SetLevel(level);
        //trait.OverrideDuration(daysDuration);
        //trait.SetTraitEffects(effects);
        trait.SetDateEstablished(new GameDate(month, day, year, tick));
        for (int i = 0; i < responsibleCharacterIDs.Count; i++) {
            Character currChar = CharacterManager.Instance.GetCharacterByID(responsibleCharacterIDs[i]);
            trait.AddCharacterResponsibleForTrait(currChar);
        }
        return trait;
        //poi.AddTrait(trait, responsibleCharacter);
    }

    //This is only for AlterEgoData
    public virtual Trait Load() {
        Character responsibleCharacter = null;
        if (responsibleCharacterID != -1) {
            responsibleCharacter = CharacterManager.Instance.GetCharacterByID(responsibleCharacterID);
        }
        Trait trait = null;
        if (TraitManager.Instance.IsInstancedTrait(name)) {
            trait = TraitManager.Instance.CreateNewInstancedTraitClass(name);
        } else {
            if (!TraitManager.Instance.allTraits.ContainsKey(name)) {
                Debug.Log("noooo!");
            }
            trait = TraitManager.Instance.allTraits[name];
        }
        trait.SetLevel(level);
        //trait.OverrideDuration(daysDuration);
        //trait.SetTraitEffects(effects);
        trait.SetDateEstablished(new GameDate(month, day, year, tick));
        for (int i = 0; i < responsibleCharacterIDs.Count; i++) {
            Character currChar = CharacterManager.Instance.GetCharacterByID(responsibleCharacterIDs[i]);
            trait.AddCharacterResponsibleForTrait(currChar);
        }
        trait.AddCharacterResponsibleForTrait(responsibleCharacter);
        //alterEgo.AddTrait(trait);
        return trait;
    }
}
