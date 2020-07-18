using System;
using System.Collections.Generic;
using Interrupts;
using UtilityScripts;
using Random = UnityEngine.Random;

public class Revenant : Summon {
    public List<Character> betrayers { get; private set; }
    public int numOfSummonedGhosts { get; private set; }

    public override string raceClassName => characterClass.className;
    public Revenant() : base(SUMMON_TYPE.Revenant, "Revenant", RACE.REVENANT,
        UtilityScripts.Utilities.GetRandomGender()) {
        visuals.SetHasBlood(false);
        betrayers = new List<Character>();
    }
    public Revenant(string className) : base(SUMMON_TYPE.Revenant, className, RACE.REVENANT,
    UtilityScripts.Utilities.GetRandomGender()) {
        visuals.SetHasBlood(false);
        betrayers = new List<Character>();
    }
    public Revenant(SaveDataCharacter data) : base(data) {
        visuals.SetHasBlood(false);
        betrayers = new List<Character>();
    }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Revenant_Behaviour);
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
    public void AdjustNumOfSummonedGhosts(int amount) {
        numOfSummonedGhosts += amount;
    }
}
