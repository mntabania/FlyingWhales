using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class Troll : Summon {
    public override string raceClassName => "Troll";

    public bool isAwakened { get; private set; }
    public bool isAttackingPlayer { get; private set; }
    public bool willLeaveWorld { get; private set; }
    public LocationStructure targetStructure { get; private set; }

    public Troll() : base(SUMMON_TYPE.Troll, "Troll", RACE.TROLL, UtilityScripts.Utilities.GetRandomGender()) {
    }
    public Troll(string className) : base(SUMMON_TYPE.Troll, className, RACE.TROLL, UtilityScripts.Utilities.GetRandomGender()) {
    }
    public Troll(SaveDataCharacter data) : base(data) { }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        movementComponent.SetEnableDigging(true);
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Troll_Behaviour);
    }
    protected override void OnTickStarted() {
        base.OnTickStarted();
        CheckBecomeStone();
    }
    #endregion
    
    private void CheckBecomeStone() {
        if (!currentStructure.isInterior) {
            TIME_IN_WORDS timeInWords = GameManager.GetCurrentTimeInWordsOfTick(null);
            if (timeInWords == TIME_IN_WORDS.EARLY_NIGHT || timeInWords == TIME_IN_WORDS.LATE_NIGHT || timeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                traitContainer.RemoveTrait(this, "Stoned");
            } else {
                if (!traitContainer.HasTrait("Stoned")) {
                    traitContainer.AddTrait(this, "Stoned");
                }
            }
        }
    }
}