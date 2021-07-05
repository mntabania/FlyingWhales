using System;
using System.Collections.Generic;
using Interrupts;
using UtilityScripts;
using Random = UnityEngine.Random;

public class Revenant : Summon {
    public override System.Type serializedData => typeof(SaveDataRevenant);

    public List<Character> betrayers { get; private set; }

    public override string raceClassName => characterClass.className;
    public Revenant() : base(SUMMON_TYPE.Revenant, "Revenant", RACE.REVENANT, UtilityScripts.Utilities.GetRandomGender()) {
        visuals.SetHasBlood(false);
        betrayers = new List<Character>();
    }
    public Revenant(string className) : base(SUMMON_TYPE.Revenant, className, RACE.REVENANT, UtilityScripts.Utilities.GetRandomGender()) {
        visuals.SetHasBlood(false);
        betrayers = new List<Character>();
    }
    public Revenant(SaveDataRevenant data) : base(data) {
        visuals.SetHasBlood(false);
        betrayers = new List<Character>();
    }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Revenant_Behaviour);
        isWildMonster = false;
    }
    public override void LoadReferences(SaveDataCharacter data) {
        if (data is SaveDataRevenant savedData) {
            for (int i = 0; i < savedData.betrayers.Count; i++) {
                Character character = CharacterManager.Instance.GetCharacterByPersistentID(savedData.betrayers[i]);
                if (character != null) {
                    betrayers.Add(character);
                }
            }
        }
        base.LoadReferences(data);
    }
    #endregion

    public void AddBetrayer(Character character) {
        betrayers.Add(character);
    }
    public Character GetRandomBetrayer() {
        if(betrayers.Count > 0) {
            return betrayers[UnityEngine.Random.Range(0, betrayers.Count)];
        }
        return null;
    }
}

[System.Serializable]
public class SaveDataRevenant : SaveDataSummon {
    public List<string> betrayers;

    public override void Save(Character data) {
        base.Save(data);
        if (data is Revenant summon) {

            betrayers = new List<string>();
            for (int i = 0; i < summon.betrayers.Count; i++) {
                betrayers.Add(summon.betrayers[i].persistentID);
            }
        }
    }
}